using q2Tool.Commands;
using q2Tool.Commands.Server;

namespace q2Tool
{
	public class RawClientPackage : RawPackage
	{
		ServerData.ServerProtocol Protocol;
		public short QPort { get; private set; }

		public RawClientPackage(byte[] data, ServerData.ServerProtocol protocol) : base(data)
		{
			if (Id != -1)
			{
				if (protocol == ServerData.ServerProtocol.R1Q2)
					QPort = ReadByte();
				else
					QPort = ReadShort();
			}

		}

		public RawClientPackage(int size) : base(size) { }

		public RawClientPackage(int id, int ack, short qPort, ICommand package, ServerData.ServerProtocol protocol)
			: base((id != -1 ? (protocol == ServerData.ServerProtocol.R1Q2? 9 : 10) : 8) + package.Size())
		{
			Id = id;
			Ack = ack;
			QPort = qPort;

			WriteInt(id);

			if (Id != -1)
			{
				WriteInt(ack);
				if (protocol == ServerData.ServerProtocol.R1Q2)
					WriteByte((byte)qPort);
				else
					WriteShort(qPort);
			}

			package.WriteTo(this);
		}
	}
}
