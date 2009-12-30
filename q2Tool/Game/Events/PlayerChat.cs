/*namespace q2Tool
{
	public class PlayerChatEventArgs : ServerCommandEventArgs<Print>
	{
		public PlayerChatEventArgs(Print command) : base(command) { }

		public string Message
		{
			get
			{
				if (Dead)
					return Command.Message.Substring(6);
				
			}
			set
			{
				
			}
		}

		public bool Dead
		{
			get
			{
				return Command.Message.StartsWith("[DEAD]");
			}
		}
	}
}
*/