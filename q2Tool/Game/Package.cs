using System.Collections.Generic;
using q2Tool.Commands;
using q2Tool.Commands.Client;
using q2Tool.Commands.Server;

namespace q2Tool
{
	public class Package : ICommand
	{
		//const int ServerCmdBits = 5;
		//const int ServerCmdMask = ((1 << ServerCmdBits) - 1);

		public Queue<ICommand> Commands { get; private set; }
		public byte[] RemainingData { get; private set; }

		public static Package ParseClientData(RawClientPackage data)
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
				nPackage.RemainingData = new byte[data.Data.Length - data.CurrentPosition];
				for (int i = 0; !data.EndOfData; i++)
					nPackage.RemainingData[i] = data.ReadByte();
			}

			return nPackage;
		}

		public static Package ParseServerData(RawServerPackage data)
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
						
					case ServerCommand.ConfigString:
						nPackage.Commands.Enqueue(new ConfigString(data));
						break;

					/*case ServerCommand.Frame:
						nPackage.Commands.Add(new Frame(data, extrabits));
						break;*/

					case ServerCommand.Print:
						nPackage.Commands.Enqueue(new Print(data));
						break;

					case ServerCommand.CenterPrint:
						nPackage.Commands.Enqueue(new CenterPrint(data));
						break;

					case ServerCommand.StuffText:
						nPackage.Commands.Enqueue(new StuffText(data));
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
				nPackage.RemainingData = new byte[data.Data.Length - data.CurrentPosition];
				for (int i = 0; !data.EndOfData; i++)
					nPackage.RemainingData[i] = data.ReadByte();
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
				size += RemainingData.Length;

			return size;
		}

		public void WriteTo(RawPackage data)
		{
			foreach (ICommand cmd in Commands)
			{
				cmd.WriteTo(data);
			}

			if (RemainingData != null)
				data.WriteBytes(RemainingData);
		}

		public ServerCommand Type { get { return ServerCommand.Bad; } }
		#endregion
	}
}