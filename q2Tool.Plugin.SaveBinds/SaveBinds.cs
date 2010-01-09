using q2Tool.Commands.Server;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace q2Tool
{
	public class SaveBinds : Plugin
	{
		readonly Dictionary<string, Dictionary<string, int>> _binds;

		public SaveBinds()
		{
			_binds = new Dictionary<string, Dictionary<string, int>>();
		}

		protected override void OnGameStart()
		{
			GetPlugin<PAction>().OnPlayerMessage += (s, e) => PlayerMessage(e.Player.Name, e.Message);
			GetPlugin<PAction>().OnRoundEnd += (s, e) => SavePlayersBinds();
			GetPlugin<PAction>().OnPlayerChangeName += aq2_OnPlayerChangeName;
		}

		void aq2_OnPlayerChangeName(Action sender, PlayerChangeNameEventArgs e)
		{
			string filePath = Quake.Directory + "Action/Binds/nickChanges.txt";

			using (StreamWriter nickChanges = File.Exists(filePath) ? File.AppendText(filePath) : new StreamWriter(filePath))
				nickChanges.WriteLine("{0} -> {1}", e.OldName, e.Player.Name);
		}

		void PlayerMessage(string nick, string message)
		{
			if (!_binds.ContainsKey(nick))
				LoadPlayerBinds(nick);

			message = IdentifyPlayers(message);
			message = IdentifyHitLocations(message);
			message = IdentifyWeapons(message);

			if(_binds[nick].ContainsKey(message))
				_binds[nick][message]++;
			else
				_binds[nick].Add(message, 1);
		}

		string IdentifyHitLocations(string message)
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

		string IdentifyWeapons(string message)
		{
			message = message.Replace("Sniper Rifle", "%W");
			message = message.Replace("M4 Assault Rifle", "%W");
			message = message.Replace("MK23 Pistol", "%W");
			return message;
		}

		string IdentifyPlayers(string message)
		{
			foreach (Player player in GetPlugin<PAction>().Players)
				message = message.Replace(player.Name, "%K");

			message = message.Replace("nobody", "%K");

			while (message.Contains("%K, %K"))
				message = message.Replace("%K, %K", "%K");

			while (message.Contains("%K and %K"))
				message = message.Replace("%K and %K", "%K");

			return message;
		}

		void LoadPlayerBinds(string nick)
		{
			string playerBindsPath = Quake.Directory + "Action/Binds/" + nick.NormalizePath() + ".txt";
			if (File.Exists(playerBindsPath))
			{
				Dictionary<string, int> playerBinds = new Dictionary<string, int>();

				using (StreamReader file = new StreamReader(playerBindsPath))
				{
					while (!file.EndOfStream)
					{
						string line = file.ReadLine();
						playerBinds.Add(line.Substring(line.IndexOf(" ") + 1), int.Parse(line.Substring(0, line.IndexOf(' '))));
					}
				}

				_binds.Add(nick, playerBinds);
			}
			else
				_binds.Add(nick, new Dictionary<string, int>());
		}

		void SavePlayersBinds()
		{
			foreach (string name in _binds.Keys)
			{
				string filePath = Quake.Directory + "Action/Binds/" + name.NormalizePath() + ".txt";
				using (StreamWriter file = new StreamWriter(filePath))
				{
					var playerBinds = from bind in _binds[name]
									  orderby bind.Value descending
									  select bind;
					
					foreach (var bind in playerBinds)
						file.WriteLine("{0} {1}", bind.Value, bind.Key);
				}
			}
		}
	}
}
