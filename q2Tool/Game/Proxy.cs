using System.Collections.Generic;
using Jv.Networking;
using q2Tool.Commands;
using q2Tool.Commands.Client;
using q2Tool.Commands.Server;

namespace q2Tool
{
	public partial class Quake
	{
		readonly UdpProxy _proxy;
		int _lastReceivedMessageId, _lastSentMessageId;

		readonly Queue<IServerCommand> _fakeServerCommands;
		readonly Queue<IClientCommand> _fakeClientCommands;

		void StartProxy(int localPort, string serverIp, int serverPort)
		{
			_proxy.ForwardLocalConnections(localPort, new ConnectionPoint(serverIp, serverPort, true));
		}

		#region Events Definitions
		#region Server
		public event ConnectionPackageEventHandler OnServerPackage;
		public event ServerCommandEventHandler<IServerStringPackage> OnServerStringPackage;
		public event ServerCommandEventHandler<ServerData> OnServerData;
		public event ServerCommandEventHandler<CenterPrint> OnCenterPrint;
		public event ServerCommandEventHandler<Print> OnPrint;
		public event ServerCommandEventHandler<StuffText> OnStuffText;
		public event ServerCommandEventHandler<ConfigString> OnConfigString;
		#endregion
		#region Client
		public event ConnectionPackageEventHandler OnClientPackage;
		public event ClientCommandEventHandler<IClientStringPackage> OnClientStringPackage;
		public event ClientCommandEventHandler<StringCmd> OnStringCmd;
		public event ClientCommandEventHandler<UserInfo> OnUserInfo;
		#endregion
		#endregion

		#region Fire events for each connection command
		void ParseClientData(IProxy sender, MessageEventArgs e)
		{
			var outcomingData = new RawClientPackage(e.Data);
			if (outcomingData.Id == _lastSentMessageId) return;
			
			_lastSentMessageId = outcomingData.Id;
			
			Package package = Package.ParseClientData(outcomingData);

			OnClientPackage.Fire(this, package);

			lock(_fakeClientCommands)
			{
				while (_fakeClientCommands.Count > 0)
					package.Commands.Enqueue(_fakeClientCommands.Dequeue());
			}

			foreach(IClientCommand cmd in package.Commands)
			{
				switch(cmd.Type)
				{
					case ClientCommand.StringCmd:
						OnStringCmd.Fire(this, (StringCmd)cmd);
						OnClientStringPackage.Fire(this, (IClientStringPackage) cmd);
						break;
					case ClientCommand.UserInfo:
						OnUserInfo.Fire(this, (UserInfo)cmd);
						OnClientStringPackage.Fire(this, (IClientStringPackage)cmd);
						break;
				}
			}

			var finalPackage = new RawClientPackage(_lastSentMessageId, outcomingData.Ack, outcomingData.QPort, package);
			e.Data = finalPackage.Data;
		}

		void ParseServerData(IProxy sender, MessageEventArgs e)
		{
			RawServerPackage incomingData = new RawServerPackage(e.Data);
			if (incomingData.Id == _lastReceivedMessageId) return;
			_lastReceivedMessageId = incomingData.Id;
			
			Package package = Package.ParseServerData(incomingData);

			OnServerPackage.Fire(this, package);

			lock (_fakeServerCommands)
			{
				while (_fakeServerCommands.Count > 0)
					package.Commands.Enqueue(_fakeServerCommands.Dequeue());
			}

			foreach (IServerCommand cmd in package.Commands)
			{
				switch (cmd.Type)
				{
					case ServerCommand.ServerData:
						OnServerData.Fire(this, (ServerData)cmd);
						break;

					case ServerCommand.CenterPrint:
						OnCenterPrint.Fire(this, (CenterPrint)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;

					case ServerCommand.Print:
						OnPrint.Fire(this, (Print)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;

					case ServerCommand.StuffText:
						OnStuffText.Fire(this, (StuffText)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;

					case ServerCommand.ConfigString:
						OnConfigString.Fire(this, (ConfigString)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;
				}
			}

			var finalServerPackage = new RawServerPackage(_lastReceivedMessageId, incomingData.Ack, package);
			e.Data = finalServerPackage.Data;
		}
		#endregion

		#region Public
		public void SendToServer(IClientCommand command)
		{
			lock (_fakeClientCommands)
				_fakeClientCommands.Enqueue(command);
		}

		public void SendCommand(string message)
		{
			lock (_fakeClientCommands)
				_fakeClientCommands.Enqueue(new StringCmd(message));
		}

		public void SendToClient(IServerCommand command)
		{
			lock (_fakeServerCommands)
				_fakeServerCommands.Enqueue(command);
		}

		public void ExecuteCommand(string command, params object[] args)
		{
			lock (_fakeServerCommands)
				_fakeServerCommands.Enqueue(new StuffText(string.Format(command + "\n", args)));
		}

		public void ReceiveChat(string command, params object[] args)
		{
			lock (_fakeServerCommands)
				_fakeServerCommands.Enqueue(new Print(Print.PrintLevel.Chat, string.Format(command, args)));
		}
		#endregion
	}
}
