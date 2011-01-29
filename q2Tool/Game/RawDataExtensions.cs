using System;
using Jv.Networking;
using q2Tool.Commands;
using q2Tool.Commands.Client;
using q2Tool;
using q2Tool.Commands.Server;

namespace q2Tool
{
	public static class RawDataExtensions
	{
		public static char ReadChar(this RawData data)
		{
			return (char)data.ReadByte();
		}

		public static string ReadQuotedString(this RawData data)
		{
			if (data.ReadChar() != '\"')
			{
				data.CurrentPosition--;
				throw new Exception("No quoted string found at current position");
			}

			return data.ReadString('\"');
		}

		public static byte[] ReadZPacket(this RawData data, int compressedLength, int uncompressedLength)
		{
			byte[] uncompressedData = new byte[uncompressedLength];

			var zs = new zlib.ZStream
			{
				next_in_index = data.CurrentPosition,
				next_in = data.Data,
				avail_in = 0,

				next_out = uncompressedData,
				avail_out = uncompressedLength
			};

			var res = zs.inflateInit(-15);
			if (res != zlib.zlibConst.Z_OK)
				throw new Exception("Inflate init failed: " + res);

			zs.avail_in = compressedLength;
			res = zs.inflate(zlib.zlibConst.Z_FINISH);
			if (res != zlib.zlibConst.Z_STREAM_END)
				throw new Exception("Inflate failed: " + res);

			res = zs.inflateEnd();
			if (res != zlib.zlibConst.Z_OK)
				throw new Exception("Inflate end failed: " + res);

			data.CurrentPosition += compressedLength;
			return uncompressedData;
		}

		public static Package<IClientCommand> ReadClientPackage(this RawData data)
		{
			Package<IClientCommand> nPackage = new Package<IClientCommand>();
			bool ignoreEndOfData = false;

			while (!(ignoreEndOfData || data.EndOfData))
			{
				ClientCommand cmd = (ClientCommand)(data.ReadByte());

				switch (cmd)
				{
					case ClientCommand.Nop:
						nPackage.Commands.Enqueue(new Nop());
						break;

					case ClientCommand.StringCmd:
						nPackage.Commands.Enqueue(new StringCmd(data));
						break;

					case ClientCommand.UserInfo:
						nPackage.Commands.Enqueue(new UserInfo(data));
						break;

					case ClientCommand.Setting:
						nPackage.Commands.Enqueue(new Setting(data));
						break;

					default:
						ignoreEndOfData = true;
						data.CurrentPosition--;
						break;
				}
			}

			while (!data.EndOfData)
				nPackage.RemainingData.Add(data.ReadByte());

			return nPackage;
		}

		public static Package<IServerCommand> ReadServerPackage(this RawData data)
		{
			const int ServerCmdBits = 5;
			const int ServerCmdMask = ((1 << ServerCmdBits) - 1);

			Package<IServerCommand> nPackage = new Package<IServerCommand>();
			bool ignoreEndOfData = false;

			while (!(ignoreEndOfData || data.EndOfData))
			{
				byte cmdCode = data.ReadByte();
				int extrabits = (cmdCode) >> ServerCmdBits;
				ServerCommand cmd = (ServerCommand)(cmdCode & ServerCmdMask);

				switch (cmd)
				{
					case ServerCommand.Disconnect:
						nPackage.Commands.Enqueue(new Disconnect());
						break;

					case ServerCommand.ServerData:
						nPackage.Commands.Enqueue(new ServerData(data));
						break;

					//case ServerCommand.Frame:
					//nPackage.Commands.Enqueue(new Frame(data, extrabits));
					//break;

					case ServerCommand.Print:
						nPackage.Commands.Enqueue(new Print(data));
						break;

					case ServerCommand.CenterPrint:
						nPackage.Commands.Enqueue(new CenterPrint(data));
						break;

					case ServerCommand.StuffText:
						nPackage.Commands.Enqueue(new StuffText(data));
						break;

					case ServerCommand.Layout:
						nPackage.Commands.Enqueue(new Layout(data));
						break;

					case ServerCommand.ZPacket:
						var compressedLength = data.ReadShort();
						var uncompressedLength = data.ReadShort();
						var compressedPackage = ReadServerPackage(new RawData(data.ReadZPacket(compressedLength, uncompressedLength)));

						foreach (var zPackage in compressedPackage.Commands)
							nPackage.Commands.Enqueue(zPackage);

						nPackage.RemainingData.AddRange(compressedPackage.RemainingData);
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

							default:
								nPackage.Commands.Enqueue(new ConfigString(configStringType, configStringSubCode, data));
								break;
						}
						break;

					default:
						ignoreEndOfData = true;
						data.CurrentPosition--;
						break;
				}
			}

			while (!data.EndOfData)
				nPackage.RemainingData.Add(data.ReadByte());

			return nPackage;
		}
	}
}
