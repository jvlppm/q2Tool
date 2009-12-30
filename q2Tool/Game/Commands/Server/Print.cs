namespace q2Tool.Commands.Server
{
	public class Print : IServerCommand, IServerStringPackage
	{
		public enum PrintLevel : byte
		{
			Low = 0x00,		// pickup messages
			Medium = 0x01,	// death messages
			High = 0x02,	// critical messages
			Chat = 0x03		// chat messages
		}

		public string Message { get; set; }
		public PrintLevel Level { get; set; }

		//[byte printLevel, string message]
		public Print(RawPackage data)
		{
			Level = (PrintLevel)data.ReadByte();
			Message = data.ReadString();
		}
		public Print(PrintLevel level, string message)
		{
			Level = level;
			Message = message;
		}

		#region ICommand
		public int Size() { return Message.Length + 3; }

		public void WriteTo(RawPackage data)
		{
			data.WriteByte((byte)Type);
			data.WriteByte((byte)Level);
			data.WriteString(Message);
		}

		public ServerCommand Type { get { return ServerCommand.Print; } }
		#endregion
	}
}
