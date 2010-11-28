using q2Tool.Commands;

namespace q2Tool
{
	public class RawClientPackage : RawPackage
	{
		public byte QPort { get; private set; }

		public RawClientPackage(byte[] data) : base(data)
		{
			//if (Id != -1)
				QPort = ReadByte();
		}

		public RawClientPackage(int size) : base(size) { }

		public RawClientPackage(int id, int ack, byte qPort, ICommand package) : base(9 + package.Size())
		{
			Id = id;
			Ack = ack;
			QPort = qPort;

			WriteInt(id);
			WriteInt(ack);
			WriteByte(qPort);

			package.WriteTo(this);
		}
	}
}
