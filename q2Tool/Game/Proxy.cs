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
		public event CommandEventHandler<Package<IServerCommand>> OnServerPackage;
		public event CommandEventHandler<IServerStringPackage> OnServerStringPackage;
		public event CommandEventHandler<ServerData> OnServerData;
		public event CommandEventHandler<CenterPrint> OnServerCenterPrint;
		public event CommandEventHandler<Print> OnServerPrint;
		public event CommandEventHandler<StuffText> OnServerStuffText;
		public event CommandEventHandler<ConfigString> OnServerConfigString;
		public event CommandEventHandler<PlayerInfo> OnServerPlayerInfo;
		public event CommandEventHandler<Layout> OnServerLayout;
		public event CommandEventHandler<Disconnect> OnServerDisconnect;
		#endregion
		#region Client
		public event CommandEventHandler<Package<IClientCommand>> OnClientPackage;
		public event CommandEventHandler<IClientStringPackage> OnClientStringPackage;
		public event CommandEventHandler<StringCmd> OnClientStringCmd;
		public event CommandEventHandler<UserInfo> OnClientUserInfo;
		public event CommandEventHandler<Setting> OnClientSetting;
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
			Package<IClientCommand> okPackage = new Package<IClientCommand>();

			lock (_fakeClientCommands)
			{
				while (_fakeClientCommands.Count > 0)
					okPackage.Commands.Enqueue(_fakeClientCommands.Dequeue());
			}

			if (OnClientPackage.Check(this, package))
			{
				foreach (IClientCommand cmd in package.Commands)
				{
					switch (cmd.Type)
					{
						case ClientCommand.StringCmd:
							if (OnClientStringCmd.Check(this, (StringCmd)cmd) &&
								OnClientStringPackage.Check(this, (IClientStringPackage)cmd))
								okPackage.Commands.Enqueue(cmd);
							break;
						case ClientCommand.UserInfo:
							if (OnClientUserInfo.Check(this, (UserInfo)cmd) &&
								OnClientStringPackage.Check(this, (IClientStringPackage)cmd))
								okPackage.Commands.Enqueue(cmd);
							break;
						case ClientCommand.Setting:
							if (OnClientSetting.Check(this, (Setting)cmd))
								okPackage.Commands.Enqueue(cmd);
							break;
						default:
							okPackage.Commands.Enqueue(cmd);
							break;
					}
				}
			}

			okPackage.RemainingData = package.RemainingData;

			var finalPackage = new RawData(8 + (Protocol == ServerData.ServerProtocol.R1Q2 ? 1 : 2) + okPackage.Size());
            finalPackage.WriteInt(sequence);
            finalPackage.WriteInt(ack);

            if (Protocol == ServerData.ServerProtocol.R1Q2)
                finalPackage.WriteByte((byte)qPort);
            else
                finalPackage.WriteShort(qPort);

			okPackage.WriteTo(finalPackage);

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
			Package<IServerCommand> okPackage = new Package<IServerCommand>();

			lock (_fakeServerCommands)
			{
				while (_fakeServerCommands.Count > 0)
					okPackage.Commands.Enqueue(_fakeServerCommands.Dequeue());
			}

			if (OnServerPackage.Check(this, package))
			{
				foreach (IServerCommand cmd in package.Commands)
				{
					switch (cmd.Type)
					{
						case ServerCommand.Disconnect:
							_lastReceivedMessageId = 0;
							_lastSentMessageId = 0;
							if (OnServerDisconnect.Check(this, (Disconnect)cmd))
								okPackage.Commands.Enqueue(cmd);
							break;

						case ServerCommand.Layout:
							if(OnServerLayout.Check(this, (Layout)cmd))
								okPackage.Commands.Enqueue(cmd);
							break;

						case ServerCommand.ServerData:
							if(OnServerData.Check(this, (ServerData)cmd))
								okPackage.Commands.Enqueue(cmd);
							break;

						case ServerCommand.CenterPrint:
							if(OnServerCenterPrint.Check(this, (CenterPrint)cmd) &&
								OnServerStringPackage.Check(this, (IServerStringPackage)cmd))
								okPackage.Commands.Enqueue(cmd);
							break;

						case ServerCommand.Print:
							if(OnServerPrint.Check(this, (Print)cmd) &&
								OnServerStringPackage.Check(this, (IServerStringPackage)cmd))
								okPackage.Commands.Enqueue(cmd);
							break;

						case ServerCommand.StuffText:
							if(OnServerStuffText.Check(this, (StuffText)cmd) &&
								OnServerStringPackage.Check(this, (IServerStringPackage)cmd))
								okPackage.Commands.Enqueue(cmd);
							break;

						case ServerCommand.ConfigString:
							switch (((ConfigString)cmd).ConfigType)
							{
								case ConfigStringType.PlayerInfo:
									if(OnServerPlayerInfo.Check(this, (PlayerInfo)cmd) &&
										OnServerConfigString.Check(this, (PlayerInfo)cmd) &&
										OnServerStringPackage.Check(this, (PlayerInfo)cmd))
										okPackage.Commands.Enqueue(cmd);
									break;

								default:
									if(OnServerConfigString.Check(this, (ConfigString)cmd) &&
										OnServerStringPackage.Check(this, (ConfigString)cmd))
										okPackage.Commands.Enqueue(cmd);
									break;
							}
							break;
					}
				}
			}
			okPackage.RemainingData = package.RemainingData;

			var finalServerPackage = new RawData(8 + okPackage.Size());
            finalServerPackage.WriteInt(sequence);
            finalServerPackage.WriteInt(ack);
			okPackage.WriteTo(finalServerPackage);

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
