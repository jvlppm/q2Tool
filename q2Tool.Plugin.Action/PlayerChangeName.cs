using System;

namespace q2Tool
{
	public class PlayerChangeNameEventArgs : EventArgs
	{
		public PlayerChangeNameEventArgs(string oldName, Player player)
		{
			Player = player;
			OldName = oldName;
		}

		public string OldName { get; private set; }
		public Player Player { get; private set; }
	}

	public delegate void PlayerChangeNameEventHandler(Action sender, PlayerChangeNameEventArgs e);
}
