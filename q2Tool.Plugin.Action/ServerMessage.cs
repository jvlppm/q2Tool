using q2Tool.Commands.Server;
using System;
namespace q2Tool
{
	public class ServerMessageEventArgs : EventArgs
	{
		public Print.PrintLevel Level { get; private set; }
		public string Message { get; private set; }

		public ServerMessageEventArgs(string message, Print.PrintLevel level)
		{
			Level = level;
			Message = message;
		}
	}

	public delegate void ServerMessageEventHandler(Action sender, ServerMessageEventArgs e);
}
