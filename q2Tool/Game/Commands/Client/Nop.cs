using Jv.Networking;

namespace q2Tool.Commands.Client
{
	public class Nop : IClientCommand
	{
        public ClientCommand Type
        {
            get { return ClientCommand.Nop; }
        }

        public int Size()
        {
            return 1;
        }

        public void WriteTo(RawData package)
        {
            package.WriteByte((byte)Type);
        }
    }
}
