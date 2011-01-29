using Jv.Networking;

namespace q2Tool.Commands.Server
{
	public class Layout : IServerCommand, IServerStringPackage
	{
		public string Message { get; set; }

		//[string message]
		public Layout(RawData data)
		{
			Message = data.ReadString();
		}
		public Layout(string message)
		{
			Message = message;
		}

		#region ICommand
		public int Size()
		{
			if (string.IsNullOrEmpty(Message))
				return 0;
			return Message.Length + 2;
		}

		public void WriteTo(RawData data)
		{
			if (string.IsNullOrEmpty(Message))
				return;

			data.WriteByte((byte)Type);
			data.WriteString(Message);
		}

		public ServerCommand Type { get { return ServerCommand.Layout; } }
		#endregion
	}
}
