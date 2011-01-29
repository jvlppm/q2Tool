using Jv.Networking;

namespace q2Tool.Commands.Server
{
	public class Disconnect : IServerCommand
	{
		#region ICommand
		public int Size()
		{
			return 1;
		}

		public void WriteTo(RawData data)
		{
			data.WriteByte((byte)Type);
		}

		public ServerCommand Type { get { return ServerCommand.Disconnect; } }
		#endregion
	}
}
