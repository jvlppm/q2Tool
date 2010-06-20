using System.Collections.Generic;
using System.Linq;

namespace q2Tool
{
	public class FixBinds : Plugin
	{
		public List<PlayerDiedEventArgs> Kills { get; set; }

		public FixBinds()
		{
			Kills = new List<PlayerDiedEventArgs>();
		}

		protected override void OnGameStart()
		{
			GetPlugin<PAction>().OnRoundBegin += delegate
			{
				Kills.Clear();
			};
			GetPlugin<PAction>().OnPlayerDied += (s, e) =>
			{
				if (e.Killer == GetPlugin<PAction>().CurrentPlayer && GetPlugin<PAction>().RoundActive)
					Kills.Add(e);
			};
			Quake.OnClientStringCmd += (s, e) =>
			{
				string toRead = e.Command.Message;
				string finalMessage = string.Empty;
				bool ignore = false;

				do
				{
					int pos = toRead.IndexOf("%");
					if (pos >= 0)
					{
						int endPos = pos;
						while (endPos < toRead.Length && toRead[endPos] != ' ') endPos++;
						while (pos >= 0 && toRead[pos] != ' ') pos--;

						string beggining = toRead.Substring(0, pos + 1);
						string variable = toRead.Substring(pos + 1, endPos - (pos + 1));
						string value = GetVariableValue(variable);

						if (value == string.Empty)
						{
							ignore = true;
							break;
						}

						finalMessage += beggining + value;
						toRead = toRead.Substring(endPos);
					}
					else
					{
						finalMessage += toRead;
						break;
					}
				} while (toRead != string.Empty);

				if (ignore)
					e.Command.Message = string.Empty;
				else
					e.Command.Message = finalMessage; 
			};
		}

		private string GetVariableValue(string variable)
		{
			int pos = variable.IndexOf("%");

			if (pos + 1 >= variable.Length)
				return variable;

			string prefix = variable.Substring(0, pos);
			string cmd = variable.Substring(pos + 1, 1);
			pos += 2;

			string location = string.Empty, weapon = string.Empty;

			while (pos < variable.Length)
			{
				if (location.Length == 0 && pos < variable.Length && variable[pos] == '[')
				{
					for (pos++; pos < variable.Length && variable[pos] != ']'; pos++)
						location += variable[pos];
					if (pos < variable.Length)
						pos++;
				}
				else if (weapon.Length == 0 && pos < variable.Length && variable[pos] == '(')
				{
					for (pos++; pos < variable.Length && variable[pos] != ')'; pos++)
						weapon += variable[pos];
					if (pos < variable.Length)
						pos++;
				}
				else break;
			}

			string suffix = variable.Substring(pos);

			bool upper = cmd.ToUpper() == cmd;

			switch (cmd.ToUpper())
			{
				case "K":
					return GetKilledPlayers(location, weapon, prefix, suffix, upper);
				default:
					return variable;
			}
		}

		string GetKilledPlayers(string location, string weapon, string prefix, string suffix, bool upper)
		{
			IEnumerable<Player> playersList = from p in Kills
										  where p.MeansOfDeath.ToString().ToLower().Contains(weapon) &&
										  p.Location.ToString().ToLower().Contains(location)
										  select p.Player;

			prefix = prefix.Replace("_", " ");
			suffix = suffix.Replace("_", " ");
			int i;
			string players = prefix;
			for (i = 0; i < playersList.Count() - 2; i++)
				players += FixName(playersList.ElementAt(i).Name, upper) + suffix + ", " + prefix;
			for (; i < playersList.Count() - 1; i++)
				players += FixName(playersList.ElementAt(i).Name, upper) + suffix + " e " + prefix;

			if (i < playersList.Count())
				players += FixName(playersList.ElementAt(i).Name, upper) + suffix;

			if (players == prefix)
				return string.Empty;
			return players;
		}

		static string FixName(string name, bool upper)
		{
			name = name.ToLower();
			int pontos = 0;
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '.')
					pontos++;
			}

			name = pontos > 2 ? name.Replace(".", "") : name.Trim('.', '^', '_');

			string[] words = name.Split('^', '.', '-', '[', ']', '(', ')', '*');
			string biggest = words[0];

			foreach (string word in words)
			{
				if (biggest.Length < word.Length)
					biggest = word;
			}

			biggest = biggest.Trim('!');

			if (!biggest.Contains('2') && !biggest.Contains('8') && !biggest.Contains('9'))
			{
				biggest = biggest.Replace('0', 'o');
				biggest = biggest.Replace('1', 'i');
				biggest = biggest.Replace('3', 'e');
				biggest = biggest.Replace('4', 'a');
				biggest = biggest.Replace('!', 'i');
				biggest = biggest.Replace('@', 'a');
				biggest = biggest.Replace('5', 's');
				biggest = biggest.Replace('6', 'g');
				biggest = biggest.Replace('7', 't');
			}

			//if(!biggest.Contains("you"))
			//	biggest = biggest.Replace("ou", "o");

			if (biggest == string.Empty)
				biggest = name;

			if (upper)
				return biggest.ToUpper();
			return biggest;
		}
	}
}
