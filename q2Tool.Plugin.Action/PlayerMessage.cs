﻿using System;

namespace q2Tool
{
	public class PlayerMessageEventArgs : EventArgs
	{
		public bool Dead { get; private set; }
		public Player Player { get; private set; }
		public string Message { get; private set; }

		public PlayerMessageEventArgs(Player player, string message, bool dead)
		{
			Dead = dead;
			Player = player;
			Message = message;
		}
	}

	public delegate void PlayerMessageEventHandler(Action sender, PlayerMessageEventArgs e);
}
