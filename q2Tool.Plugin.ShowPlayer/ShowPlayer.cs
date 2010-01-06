using System.Collections.Generic;
using q2Tool.Commands.Client;
using q2Tool.Commands.Server;
using System.Linq;

namespace q2Tool
{
	public class ShowPlayer : Plugin
	{
		readonly Dictionary<int, string> _skins;

		public ShowPlayer()
		{
			_skins = new Dictionary<int, string>();
		}

		protected override void OnGameStart()
		{
			Quake.OnClientStringCmd += Quake_OnStringCmd;
			Quake.OnServerPlayerInfo += Quake_OnPlayerInfo;
		}

		void Quake_OnPlayerInfo(Quake sender, ServerCommandEventArgs<PlayerInfo> e)
		{
			if (_skins.ContainsKey(e.Command.Id))
				e.Command.Skin = _skins[e.Command.Id];
		}

		void Quake_OnStringCmd(Quake sender, ClientCommandEventArgs<StringCmd> e)
		{
			if (e.Command.Message.StartsWith("show"))
			{
				string[] cmd = e.Command.Message.Split(' ');

				if (cmd.Length == 1)
					Quake.SendToClient(new Print(Print.PrintLevel.High, GetPlayerList()));
				else if (cmd.Length > 3)
					Quake.SendToClient(new Print(Print.PrintLevel.High, "Usage: show [# [skin] ]\n"));
				else
				{
					string showSkin = "nut";
					if (cmd.Length == 3)
						showSkin = cmd[2];

					int playerId = int.Parse(cmd[1]);
					if (!GetPlugin<PAction>().PlayersById.ContainsKey(playerId))
						Quake.SendToClient(new Print(Print.PrintLevel.High, "Bad player number\n"));
					else
					{
						Quake.SendToClient(new PlayerInfo(playerId, GetPlugin<PAction>().PlayersById[playerId].Name, "male", showSkin));
						if (!_skins.ContainsKey(playerId))
							_skins.Add(playerId, showSkin);
						else
							_skins[playerId] = showSkin;
					}
				}

				e.Command.Message = string.Empty;
			}
		}

		string GetPlayerList()
		{
			var playersList = new System.Text.StringBuilder();

			playersList.Append("Usage: show [# [skin] ]\n\n");
			playersList.Append("Available players:\n");
			playersList.Append(" #  Name\n");
			playersList.Append("------------------------------------\n");
			foreach (var player in GetPlugin<PAction>().Players.OrderBy(p => p.Id))
			{
				if (_skins.ContainsKey(player.Id))
					playersList.Append(" " + player.Id + "  " + player.Name + "  " + _skins[player.Id] + "\n");
				else
					playersList.Append(" " + player.Id + "  " + player.Name + "\n");
			}

			return playersList.ToString() + "\n";
		}
	}
}
