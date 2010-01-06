using System;

namespace q2Tool
{
	public class PlayerEventArgs : EventArgs
	{
		public PlayerEventArgs(Player player)
		{
			Player = player;
		}

		public Player Player { get; private set; }
	}

	public delegate void PlayerEventHandler(Action sender, PlayerEventArgs e);
}
