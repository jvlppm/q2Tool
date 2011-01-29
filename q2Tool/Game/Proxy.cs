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

		public ConnectionPoint Server { get; private set; }
		public ConnectionPoint Client { get; private set; }

		void StartProxy(int localPort, string serverIp, int serverPort)
		{
			Client = new ConnectionPoint(new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, localPort), false);
			Server = new ConnectionPoint(serverIp, serverPort, true);
			_proxy.ForwardConnections(Client, Server);
		}

		#region Events Definitions
		#region Server
		public event ConnectionPackageEventHandler<IServerCommand> OnServerPackage;
		public event ServerCommandEventHandler<IServerStringPackage> OnServerStringPackage;
		public event ServerCommandEventHandler<ServerData> OnServerData;
		public event ServerCommandEventHandler<CenterPrint> OnServerCenterPrint;
		public event ServerCommandEventHandler<Print> OnServerPrint;
		public event ServerCommandEventHandler<StuffText> OnServerStuffText;
		public event ServerCommandEventHandler<ConfigString> OnServerConfigString;
		public event ServerCommandEventHandler<PlayerInfo> OnServerPlayerInfo;
		#endregion
		#region Client
		public event ConnectionPackageEventHandler<IClientCommand> OnClientPackage;
		public event ClientCommandEventHandler<IClientStringPackage> OnClientStringPackage;
		public event ClientCommandEventHandler<StringCmd> OnClientStringCmd;
		public event ClientCommandEventHandler<UserInfo> OnClientUserInfo;
		public event ClientCommandEventHandler<Setting> OnClientSetting;
		#endregion
		#endregion

		ServerData.ServerProtocol Protocol;

		#region Fire events for each connection command
		void ParseClientData(IProxy sender, MessageEventArgs e)
		{
			var outcomingData = new RawData(e.Data);
            int sequence = outcomingData.ReadInt();
            int outId =  sequence & ~(1 << 31);

            if (sequence == -1)
            {
                string cmd = outcomingData.ReadString(' ');
                if (cmd == "connect")
                    Protocol = (ServerData.ServerProtocol)int.Parse(outcomingData.ReadString(' '));
                return;
            }

            if (outId <= _lastSentMessageId)
                return;
            int ack = outcomingData.ReadInt();

            short qPort;
            if (Protocol == ServerData.ServerProtocol.R1Q2)
                qPort = outcomingData.ReadByte();
            else
                qPort = outcomingData.ReadShort();

            _lastSentMessageId = outId;

            Package<IClientCommand> package = outcomingData.ReadClientPackage();

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
						OnClientStringCmd.Fire(this, (StringCmd)cmd);
						OnClientStringPackage.Fire(this, (IClientStringPackage) cmd);
						break;
					case ClientCommand.UserInfo:
						OnClientUserInfo.Fire(this, (UserInfo)cmd);
						OnClientStringPackage.Fire(this, (IClientStringPackage)cmd);
						break;
					case ClientCommand.Setting:
						OnClientSetting.Fire(this, (Setting)cmd);
						break;
				}
			}

            var finalPackage = new RawData(8 + (Protocol == ServerData.ServerProtocol.R1Q2 ? 1 : 2) + package.Size());
            finalPackage.WriteInt(sequence);
            finalPackage.WriteInt(ack);

            if (Protocol == ServerData.ServerProtocol.R1Q2)
                finalPackage.WriteByte((byte)qPort);
            else
                finalPackage.WriteShort(qPort);

            package.WriteTo(finalPackage);

			e.Data = finalPackage.Data;
		}

		void ParseServerData(IProxy sender, MessageEventArgs e)
		{
            RawData incomingData = new RawData(e.Data);

            int sequence = incomingData.ReadInt();
            int inId = sequence & ~(1 << 31);

            if (inId <= _lastReceivedMessageId || sequence == -1)
                return;

            _lastReceivedMessageId = inId;
            int ack = incomingData.ReadInt();

			Package<IServerCommand> package = incomingData.ReadServerPackage();

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
						OnServerCenterPrint.Fire(this, (CenterPrint)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;

					case ServerCommand.Print:
						OnServerPrint.Fire(this, (Print)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;

					case ServerCommand.StuffText:
						OnServerStuffText.Fire(this, (StuffText)cmd);
						OnServerStringPackage.Fire(this, (IServerStringPackage)cmd);
						break;

					case ServerCommand.ConfigString:
						switch (((ConfigString)cmd).ConfigType)
						{
							case ConfigStringType.PlayerInfo:
								OnServerPlayerInfo.Fire(this, (PlayerInfo)cmd);
								OnServerConfigString.Fire(this, (PlayerInfo)cmd);
								OnServerStringPackage.Fire(this, (PlayerInfo)cmd);
								break;

							default:
								OnServerConfigString.Fire(this, (ConfigString)cmd);
								OnServerStringPackage.Fire(this, (ConfigString)cmd);
								break;
						}
						break;
				}
			}

            var finalServerPackage = new RawData(8 + package.Size());
            finalServerPackage.WriteInt(sequence);
            finalServerPackage.WriteInt(ack);
            package.WriteTo(finalServerPackage);

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
