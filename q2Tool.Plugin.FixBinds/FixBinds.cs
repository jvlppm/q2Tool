using System.Collections.Generic;
using System.Linq;

namespace q2Tool
{
	public class FixBinds : Plugin
	{
		public List<Player> Kills { get; set; }

		public FixBinds()
		{
			Kills = new List<Player>();
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
					Kills.Add(e.Player);
			};
			Quake.OnClientStringCmd += (s, e) =>
			{
				string p = e.Command.Message;
				if (p.Contains("%k") || p.Contains("%K"))
				{
					if (Kills.Count == 0)
						p = string.Empty;
					else
					{
						int pos = 0, testPos;

						testPos = p.IndexOf("%k", pos);
						if (testPos < 0 || testPos > p.IndexOf("%K", pos) && p.IndexOf("%K") >= 0)
							pos = p.IndexOf("%K", pos);
						else
							pos = testPos;

						while (pos >= 0)
						{
							int i, j;
							for (i = pos - 1; i >= 0 && (char.IsLetterOrDigit(p, i) || p[i] == '_'); i--) ;
							for (j = pos + 2; j < p.Length && (char.IsLetterOrDigit(p, j) || p[j] == '_'); j++) ;

							string prefix = string.Empty, suffix = string.Empty;
							if (i >= 0)
								prefix = p.Substring(i + 1, pos - 1 - i);
							if (j < p.Length)
								suffix = p.Substring(pos + 2, j - pos - 2);

							string players = GetKilledPlayers(prefix, suffix, p[pos + 1] == 'K');

							p = p.Substring(0, pos - prefix.Length) +
												players +
												p.Substring(pos + 2 + suffix.Length);
							pos = i + players.Length;

							testPos = p.IndexOf("%k", pos);
							if (testPos < 0 || testPos > p.IndexOf("%K", pos) && p.IndexOf("%K") >= 0)
								pos = p.IndexOf("%K", pos);
							else
								pos = testPos;
						}

						Kills.Clear();
					}
				}
				e.Command.Message = p;
			};
		}

		string GetKilledPlayers(string prefix, string suffix, bool upper)
		{
			prefix = prefix.Replace("_", " ");
			suffix = suffix.Replace("_", " ");
			int i;
			string players = prefix;
			for (i = 0; i < Kills.Count - 2; i++)
				players += FixName(Kills[i].Name, upper) + suffix + ", " + prefix;
			for (; i < Kills.Count - 1; i++)
				players += FixName(Kills[i].Name, upper) + suffix + " e " + prefix;

			if (i < Kills.Count)
				players += FixName(Kills[i].Name, upper) + suffix;

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
