namespace q2Tool
{
	public class PlayerChangeNameEventArgs : PlayerEventArgs
	{
		public PlayerChangeNameEventArgs(string oldName, Player player)
			: base(player)
		{
			OldName = oldName;
		}

		public string OldName { get; private set; }
	}

	public delegate void PlayerChangeNameEventHandler(Action sender, PlayerChangeNameEventArgs e);
}
