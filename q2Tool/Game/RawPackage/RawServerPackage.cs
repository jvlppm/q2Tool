using q2Tool.Commands;

namespace q2Tool
{
	public class RawServerPackage : RawPackage
	{
		public RawServerPackage(byte[] data) : base(data) { }

		public RawServerPackage(int size) : base(size) { }
		
		public RawServerPackage(int id, int ack, ICommand package) : base(id, ack, package) { }
	}
}
