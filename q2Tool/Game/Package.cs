using System.Collections.Generic;
using q2Tool.Commands;
using q2Tool.Commands.Client;
using q2Tool.Commands.Server;
using System;
using Jv.Networking;

namespace q2Tool
{
    public class Package<CommandsType> : ICommand where CommandsType : ICommand
    {
        public Queue<CommandsType> Commands { get; private set; }
        public List<byte> RemainingData { get; set; }

        public Package()
        {
            Commands = new Queue<CommandsType>();
            RemainingData = new List<byte>();
        }

        public void Clear()
        {
            Commands.Clear();
            RemainingData.Clear();
        }

        #region ICommand
        public int Size()
        {
            int size = 0;

            foreach (ICommand cmd in Commands)
                size += cmd.Size();

            return size + RemainingData.Count;
        }

        public void WriteTo(RawData data)
        {
            foreach (ICommand cmd in Commands)
                cmd.WriteTo(data);

            if (RemainingData.Count != 0)
                data.WriteBytes(RemainingData.ToArray());
        }
        #endregion
    }
	/*public class Package : ICommand
	{
		//const int ServerCmdBits = 5;
		//const int ServerCmdMask = ((1 << ServerCmdBits) - 1);

		public Queue<ICommand> Commands { get; private set; }
		public List<byte> RemainingData { get; private set; }

        public static Package ParseClientData(RawData data)
		{
			Package nPackage = new Package();
			bool ignoreEndOfData = false;

			while(!(ignoreEndOfData || data.EndOfData))
			{
				ClientCommand cmd = (ClientCommand) (data.ReadByte());

				switch(cmd)
				{
					case ClientCommand.StringCmd:
						nPackage.Commands.Enqueue(new StringCmd(data));
						break;

					case ClientCommand.UserInfo:
						nPackage.Commands.Enqueue(new UserInfo(data));
						break;

					default:
						//System.Console.WriteLine("Command Type: {0}", cmd);
						ignoreEndOfData = true;
						data.CurrentPosition--;
						break;
				}
			}

			if (!data.EndOfData)
			{
				nPackage.RemainingData = new List<byte>();
				for (int i = 0; !data.EndOfData; i++)
					nPackage.RemainingData.Add(data.ReadByte());
			}

			return nPackage;
		}

        public static Package ParseServerData(RawData data)
		{
			Package nPackage = new Package();
			bool ignoreEndOfData = false;

			while (!(ignoreEndOfData || data.EndOfData))
			{
				byte cmdCode = data.ReadByte();
				//int extrabits = (cmdCode) >> ServerCmdBits;
				//ServerCommand cmd = (ServerCommand) (cmdCode & ServerCmdMask);
				ServerCommand cmd = (ServerCommand)cmdCode;
				
				switch (cmd)
				{
					case ServerCommand.ServerData:
						nPackage.Commands.Enqueue(new ServerData(data));
						break;

					//case ServerCommand.Frame:
					//	nPackage.Commands.Add(new Frame(data, extrabits));
					//	break;

					case ServerCommand.Print:
						nPackage.Commands.Enqueue(new Print(data));
						break;

					case ServerCommand.CenterPrint:
						nPackage.Commands.Enqueue(new CenterPrint(data));
						break;

                    case ServerCommand.ZPacket:
                        var compressedLength = data.ReadShort();
                        var uncompressedLength = data.ReadShort();
                        var compressedPackage = ParseServerData(new RawData(data.ReadZPacket(compressedLength, uncompressedLength)));

                        foreach (var zPackage in compressedPackage.Commands)
                            nPackage.Commands.Enqueue(zPackage);

                        nPackage.RemainingData.AddRange(compressedPackage.RemainingData);
                        break;

					case ServerCommand.StuffText:
						nPackage.Commands.Enqueue(new StuffText(data));
						break;

					case ServerCommand.ConfigString:
						var configStringType = (ConfigStringType)data.ReadShort();
						int configStringSubCode = 0;

						var configTypes = Enum.GetValues(typeof(ConfigStringType));
						for (int i = configTypes.Length - 1; i >= 0; i--)
						{
							ConfigStringType current = (ConfigStringType)configTypes.GetValue(i);
							if (configStringType >= current)
							{
								configStringSubCode = configStringType - current;
								configStringType = current;
								break;
							}
						}

						switch (configStringType)
						{
							case ConfigStringType.PlayerInfo:
								nPackage.Commands.Enqueue(new PlayerInfo(configStringSubCode, data));
								break;

							case ConfigStringType.Name:
							case ConfigStringType.CdTrack:
							case ConfigStringType.Sky:
							case ConfigStringType.SkyAxis:
							case ConfigStringType.SkyRotate:
							case ConfigStringType.StatusBar:

							case ConfigStringType.AirAccel:
							case ConfigStringType.MaxClients:
							case ConfigStringType.MapChecksum:

							case ConfigStringType.Models:
							case ConfigStringType.Sounds:
							case ConfigStringType.Images:
							case ConfigStringType.Lights:
							case ConfigStringType.Items:
							case ConfigStringType.General:
							case ConfigStringType.Bad:
								nPackage.Commands.Enqueue(new ConfigString(configStringType, configStringSubCode, data));
								break;
						}
						break;

					default:
						//System.Console.WriteLine(cmd);
						ignoreEndOfData = true;
						data.CurrentPosition--;
						break;
				}
			}

			if (!data.EndOfData)
			{
				nPackage.RemainingData = new List<byte>();
				for (int i = 0; !data.EndOfData; i++)
					nPackage.RemainingData.Add(data.ReadByte());
			}

			return nPackage;
		}

		public Package()
		{
			Commands = new Queue<ICommand>();
		}

		#region ICommand
		public int Size()
		{
			int size = 0;

			foreach (ICommand cmd in Commands)
				size += cmd.Size();

			if(RemainingData != null)
				size += RemainingData.Count;

			return size;
		}

        public void WriteTo(RawData data)
		{
			foreach (ICommand cmd in Commands)
			{
				cmd.WriteTo(data);
			}

			if (RemainingData != null)
				data.WriteBytes(RemainingData.ToArray());
		}

		public ServerCommand Type { get { return ServerCommand.Bad; } }
		#endregion
	}*/
}