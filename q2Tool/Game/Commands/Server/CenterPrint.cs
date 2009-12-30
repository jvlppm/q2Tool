namespace q2Tool.Commands.Server
{
	public class CenterPrint : IServerCommand, IServerStringPackage
	{
		public string Message { get; set; }

		//[string message]
		public CenterPrint(RawPackage data)
		{
			Message = data.ReadString();
		}
		public CenterPrint(string message)
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

		public void WriteTo(RawPackage data)
		{
			if (string.IsNullOrEmpty(Message))
				return;

			data.WriteByte((byte)Type);
			data.WriteString(Message);
		}

		public ServerCommand Type { get { return ServerCommand.CenterPrint; } }
		#endregion
	}
}
