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
			var separators = new List<char> { '^', '.', '-', '[', ']', '(', ')', '*', '=' };

			int pontos = 0;
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '.')
					pontos++;
			}

			name = pontos > 2 ? name.Replace(".", "") : name.Trim('.', '^', '_');

			string[] words = name.Split(separators.ToArray());
			string bigger = words[0];

			foreach (string word in words)
			{
				if (bigger.Length <= word.Length)
					bigger = word;
			}

			bigger = bigger.Trim('!');

			if (!bigger.Contains('2') && !bigger.Contains('8') && !bigger.Contains('9'))
			{
				bigger = bigger.Replace('0', 'o');
				bigger = bigger.Replace('1', 'i');
				bigger = bigger.Replace('3', 'e');
				bigger = bigger.Replace('4', 'a');
				bigger = bigger.Replace('!', 'i');
				bigger = bigger.Replace('@', 'a');
				bigger = bigger.Replace('5', 's');
				bigger = bigger.Replace('6', 'g');
				bigger = bigger.Replace('7', 't');
			}

			//if(!biggest.Contains("you"))
			//	biggest = biggest.Replace("ou", "o");

			if (bigger == string.Empty)
				bigger = name;

			string final = string.Empty;
			string current = string.Empty;
			for (int i = 0; i < bigger.Length; i++)
			{
				if (char.IsUpper(bigger[i]))
				{
					if (final.Length <= current.Length)
						final = current;

					current = bigger[i].ToString();
				}
				else current += bigger[i];
			}

			if (final.Length <= current.Length)
				final = current;

			if (final == string.Empty || final.Length <= bigger.Length / 2)
				final = bigger;

			if (upper)
				return final.ToUpper();
			return final.ToLower();
		}
	}
}
