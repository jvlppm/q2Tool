using System.Collections.Generic;
using Jv.Networking;
using q2Tool.Commands;
using q2Tool.Commands.Client;
using q2Tool.Commands.Server;

namespace q2Tool
{
	public partial class Quake
	{
		public UdpProxy Proxy { get; private set; }
		int _lastReceivedMessageId, _lastSentMessageId;

		readonly Queue<IServerCommand> FakeServerCommands;
		readonly Queue<IClientCommand> FakeClientCommands;

		void StartProxy(int localPort, string serverIp, int serverPort)
		{
			Proxy.ForwardLocalConnections(localPort, new ConnectionPoint(serverIp, serverPort, true));
		}

		#region Events Definitions
		#region Server
		public event ConnectionPackageEventHandler OnServerPackage;
		public event CommandEventHandler<IStringPackage> OnServerStringPackage;
		public event ServerCommandEventHandler<ServerData> OnServerData;
		public event ServerCommandEventHandler<CenterPrint> OnCenterPrint;
		public event ServerCommandEventHandler<Print> OnPrint;
		public event ServerCommandEventHandler<StuffText> OnStuffText;
		public event ServerCommandEventHandler<ConfigString> OnConfigString;
		#endregion
		#region Client
		public event ConnectionPackageEventHandler OnClientPackage;
		public event CommandEventHandler<IStringPackage> OnClientStringPackage;
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

			lock(FakeClientCommands)
			{
				while(FakeClientCommands.Count > 0)
					package.Commands.Enqueue(FakeClientCommands.Dequeue());
			}

			foreach(IClientCommand cmd in package.Commands)
			{
				switch(cmd.Type)
				{
					case ClientCommand.StringCmd:
						OnStringCmd.Fire(this, (StringCmd)cmd);
						OnClientStringPackage.Fire(this, (IStringPackage) cmd);
						break;
					case ClientCommand.UserInfo:
						OnUserInfo.Fire(this, (UserInfo)cmd);
						OnClientStringPackage.Fire(this, (IStringPackage)cmd);
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

			lock (FakeServerCommands)
			{
				while(FakeServerCommands.Count > 0)
					package.Commands.Enqueue(FakeServerCommands.Dequeue());
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
						OnServerStringPackage.Fire(this, (IStringPackage)cmd);
						break;

					case ServerCommand.Print:
						OnPrint.Fire(this, (Print)cmd);
						OnServerStringPackage.Fire(this, (IStringPackage)cmd);
						break;

					case ServerCommand.StuffText:
						OnStuffText.Fire(this, (StuffText)cmd);
						OnServerStringPackage.Fire(this, (IStringPackage)cmd);
						break;

					case ServerCommand.ConfigString:
						OnConfigString.Fire(this, (ConfigString)cmd);
						OnServerStringPackage.Fire(this, (IStringPackage)cmd);
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
			lock (FakeClientCommands)
				FakeClientCommands.Enqueue(command);
		}

		public void SendCommand(string message)
		{
			lock (FakeClientCommands)
				FakeClientCommands.Enqueue(new StringCmd(message));
		}

		public void SendToClient(IServerCommand command)
		{
			lock(FakeServerCommands)
				FakeServerCommands.Enqueue(command);
		}

		public void ExecuteCommand(string command, params object[] args)
		{
			lock (FakeServerCommands)
				FakeServerCommands.Enqueue(new StuffText(string.Format(command + "\n", args)));
		}

		public void ReceiveChat(string command, params object[] args)
		{
			lock (FakeServerCommands)
				FakeServerCommands.Enqueue(new Print(Print.PrintLevel.Chat, string.Format(command, args)));
		}
		#endregion
	}
}
