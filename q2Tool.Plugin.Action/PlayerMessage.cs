using System;
using System.Collections.Generic;

namespace q2Tool
{
	public class PlayerMessageEventArgs : EventArgs
	{
		public bool Dead { get; private set; }
		public Player Player { get; private set; }
		public string Message { get; private set; }
		public string CodedMessage { get; private set; }
		public bool TeamMessage { get; private set; }

		public PlayerMessageEventArgs(Player player, string message, bool dead, bool team, IEnumerable<Player> players)
		{
			Dead = dead;
			Player = player;
			Message = message;
			TeamMessage = team;

			CodedMessage = ParsePlayerMessage(message, players);
		}

		static string ParsePlayerMessage(string message, IEnumerable<Player> players)
		{
			return IdentifyHitLocations(IdentifyWeapons(IdentifyPlayers(message, players)));
		}

		static string IdentifyHitLocations(string message)
		{
			message = message.Replace("head", "%D");
			message = message.Replace("stomach", "%D");
			message = message.Replace("legs", "%D");
			message = message.Replace("chest", "%D");
			message = message.Replace("body", "%D");
			message = message.Replace("kevlar vest", "%D");
			message = message.Replace("nothing", "%D");
			return message;
		}

		static string IdentifyWeapons(string message)
		{
			message = message.Replace("Sniper Rifle", "%W");
			message = message.Replace("M4 Assault Rifle", "%W");
			message = message.Replace("MK23 Pistol", "%W");
			return message;
		}

		static string IdentifyPlayers(string message, IEnumerable<Player> players)
		{
			foreach (Player player in players)
				message = message.Replace(player.Name, "%K");

			message = message.Replace("nobody", "%K");

			while (message.Contains("%K, %K"))
				message = message.Replace("%K, %K", "%K");

			while (message.Contains("%K and %K"))
				message = message.Replace("%K and %K", "%K");

			return message;
		}
	}

	public delegate void PlayerMessageEventHandler(Action sender, PlayerMessageEventArgs e);
}
