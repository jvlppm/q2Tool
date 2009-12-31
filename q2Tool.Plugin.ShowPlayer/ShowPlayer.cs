using System.Collections.Generic;
using q2Tool.Commands.Server;
using q2Tool.Commands.Client;
namespace q2Tool
{
	public class ShowPlayer : Plugin
	{
		readonly Dictionary<int, string> _players;
		readonly Dictionary<int, string> _skins;

		public ShowPlayer()
		{
			_players = new Dictionary<int, string>();
			_skins = new Dictionary<int, string>();
		}

		protected override void OnGameStart()
		{
			Quake.OnPrint += Quake_OnPrint;
			Quake.OnStringCmd += Quake_OnStringCmd;
			Quake.OnPlayerInfo += Quake_OnPlayerInfo;
		}

		void Quake_OnPlayerInfo(Quake sender, ServerCommandEventArgs<PlayerInfo> e)
		{
			if (!_players.ContainsKey(e.Command.Id + 1))
				_players.Add(e.Command.Id + 1, e.Command.Name);
			else
			{
				if (_players[e.Command.Id + 1] != e.Command.Name)
					e.Command.Skin = _skins[e.Command.Id + 1];
				else if(_skins[e.Command.Id + 1] != e.Command.Skin)
					_skins.Remove(e.Command.Id + 1);
				
				_players[e.Command.Id + 1] = e.Command.Name;
			}
		}

		void Quake_OnStringCmd(Quake sender, ClientCommandEventArgs<StringCmd> e)
		{
			if (e.Command.Message.StartsWith("show")) // split ' ' cop
			{
				string[] cmd = e.Command.Message.Split(' ');

				if (cmd.Length == 1)
				{
					showPlayers++;
					Quake.SendToServer(new StringCmd("kicklist"));
				}
				else if (cmd.Length > 3)
				{
					Quake.SendToClient(new Print(Print.PrintLevel.High, "Usage: show [# [skin] ]\n"));
				}
				else
				{
					string showSkin = "nut";
					if (cmd.Length == 3)
						showSkin = cmd[2];

					int playerId = int.Parse(cmd[1]);
					if (!_players.ContainsKey(playerId))
						Quake.SendToClient(new Print(Print.PrintLevel.High, "Bad player number\n"));
					else
					{
						Quake.SendToClient(new PlayerInfo(playerId - 1, _players[playerId], "male", showSkin));
						if (!_skins.ContainsKey(playerId))
							_skins.Add(playerId, showSkin);
						else
							_skins[playerId] = showSkin;
					}
				}

				e.Command.Message = string.Empty;
			}
		}

		int showPlayers = 0;

		void Quake_OnPrint(Quake sender, ServerCommandEventArgs<Print> e)
		{
			if (e.Command.Message.StartsWith("\nAvailable players to kick:"))
			{
				_players.Clear();
				string[] lines = e.Command.Message.Split('\n');

				int i;
				for (i = 0; !lines[i].StartsWith("-"); i++) ;
				for (i++; i < lines.Length; i++)
				{
					string line = lines[i].Trim(' ');
					if (!line.Contains(" "))
						break;
					int playerNumber = int.Parse(line.Substring(0, line.IndexOf(' ')));
					string playerName = line.Substring(line.IndexOf(' ') + 2);

					if (!_players.ContainsKey(playerNumber))
						_players.Add(playerNumber, playerName);
					else
						_players[playerNumber] = playerName;
				}

				if (showPlayers > 0)
				{
					e.Command.Message = string.Empty;
					showPlayers--;
					var playersList = new System.Text.StringBuilder();
					
					if (_players.Count > 0)
					{
						playersList.Append("Usage: show [# [skin] ]\n\n");
						playersList.Append("Available players:\n");
						playersList.Append(" #  Name\n");
						playersList.Append("------------------------------------\n");
						foreach (var player in _players)
						{
							if(_skins.ContainsKey(player.Key))
								playersList.Append(" " + player.Key + "  " + player.Value + "  " + _skins[player.Key] + "\n");
							else
								playersList.Append(" " + player.Key + "  " + player.Value + "\n");
						}
					}
					else
					{
						playersList.Append("No players available\n");
					}

					Quake.SendToClient(new Print(Print.PrintLevel.High, playersList.ToString() + "\n"));
				}
			}
		}
	}
}
