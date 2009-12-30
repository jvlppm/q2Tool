using System;

namespace q2Tool.Commands.Server
{
	public class ServerData : IServerCommand
	{
		public enum ServerProtocol
		{
			Old = 26,
			Default = 34,
			R1Q2 = 35
		}

		public ServerProtocol Protocol { get; private set; }
		public int ServerCount { get; private set; }
		public byte AttractLoop { get; private set; }
		public string GameDir { get; private set; }
		public short PlayerNum { get; private set; }
		public string LevelName { get; private set; }
		public byte StrafeHack { get; private set; }
		public short ProtocolVersion { get; private set; }
		public byte EnhancedVersion { get; private set; }
		public byte Unknown { get; private set; }

		//[int protocol][int serverCount][byte attractLoop][string gameDir][string levelName] ...
		public ServerData(RawPackage serverPackage)
		{
			Protocol = (ServerProtocol) serverPackage.ReadInt();
			ServerCount = serverPackage.ReadInt();
			AttractLoop = serverPackage.ReadByte();
			GameDir = serverPackage.ReadString();
			PlayerNum = serverPackage.ReadShort();
			LevelName = serverPackage.ReadString();

			if(Protocol == ServerProtocol.R1Q2)
			{
				EnhancedVersion = serverPackage.ReadByte();
				if (EnhancedVersion != 0)
					throw new Exception("Protocol not supported (Enhanced r1q2)");
				ProtocolVersion = serverPackage.ReadShort();
				if (ProtocolVersion >= 1903)
				{
					Unknown = serverPackage.ReadByte();
					StrafeHack = serverPackage.ReadByte();
				}
			}
		}

		#region ICommand
		public int Size()
		{
			int size = 12;
			if (!string.IsNullOrEmpty(GameDir))
				size += GameDir.Length + 1;
			else size++;

			if (!string.IsNullOrEmpty(LevelName))
				size += LevelName.Length + 1;
			else size++;

			if (Protocol == ServerProtocol.R1Q2)
			{
				if (EnhancedVersion != 0)
					throw new Exception("Protocol not supported (Enhanced r1q2)");
				size += 3;
				if (ProtocolVersion >= 1903)
					size += 2;
			}

			return size;
		}

		public void WriteTo(RawPackage data)
		{
			data.WriteByte((byte)Type);
			data.WriteInt((int)Protocol);
			data.WriteInt(ServerCount);
			data.WriteByte(AttractLoop);
			data.WriteString(GameDir);
			data.WriteShort(PlayerNum);
			data.WriteString(LevelName);

			if (Protocol != ServerProtocol.R1Q2)
				return;

			data.WriteByte(EnhancedVersion);
			data.WriteShort(ProtocolVersion);

			if (ProtocolVersion < 1903)
				return;

			data.WriteByte(Unknown);
			data.WriteByte(StrafeHack);
		}

		public ServerCommand Type { get { return ServerCommand.ServerData; } }
		#endregion
	}
}