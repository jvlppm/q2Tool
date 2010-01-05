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
			using (StreamWriter nickChanges = File.AppendText(Quake.Directory + "Action/Binds/nickChanges.txt"))
				nickChanges.WriteLine("{0} -> {1}", e.OldName, e.Player.Name);
		}

		void PlayerMessage(string nick, string message)
		{
			if (!_binds.ContainsKey(nick))
				LoadPlayerBinds(nick);

			foreach (Player player in GetPlugin<PAction>().Players)
				message = message.Replace(player.Name, "%K");

			while (message.Contains("%K, %K"))
				message = message.Replace("%K, %K", "%K");

			while (message.Contains("%K and %K"))
				message = message.Replace("%K and %K", "%K");


			if(_binds[nick].ContainsKey(message))
				_binds[nick][message]++;
			else
				_binds[nick].Add(message, 1);
		}

		void LoadPlayerBinds(string nick)
		{
			if (File.Exists(Quake.Directory + "Action/Binds/" + nick + ".txt"))
			{
				Dictionary<string, int> playerBinds = new Dictionary<string, int>();

				using (StreamReader file = new StreamReader(Quake.Directory + "Action/Binds/" + nick + ".txt"))
				{
					while (!file.EndOfStream)
					{
						string line = file.ReadLine();
						playerBinds.Add(line.Substring(2), int.Parse(line.Substring(0, line.IndexOf(' '))));
					}
				}

				_binds.Add(nick, playerBinds);
			}
			else
				_binds.Add(nick, new Dictionary<string, int>());
		}

		void SavePlayersBinds()
		{
			foreach (string player in _binds.Keys)
			{
				string filePath = Quake.Directory + "Action/Binds/" + player.Replace("/", "") + ".txt";
				filePath = filePath.Replace("*", "");
				using (StreamWriter file = new StreamWriter(filePath))
				{
					var playerBinds = from bind in _binds[player]
									  orderby bind.Value descending
									  select bind;
					
					foreach (var bind in playerBinds)
						file.WriteLine("{0} {1}", bind.Value, bind.Key);
				}
			}
		}
	}
}
