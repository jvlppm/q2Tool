using System;

namespace q2Tool
{
	public class PlayerDiedEventArgs : PlayerEventArgs
	{
		public PlayerDiedEventArgs(Player player, Player killer)
			: base(player)
		{
			Killer = killer;
		}

		public Player Killer { get; private set; }
	}

	public delegate void PlayerDiedEventHandler(Action sender, PlayerDiedEventArgs e);
}
