using System.Collections.Generic;
using q2Tool.Commands.Client;
using q2Tool.Commands.Server;
using System.Linq;

namespace q2Tool
{
	public class ShowPlayer : Plugin
	{
		readonly Dictionary<string, string> _skins;

		public ShowPlayer()
		{
			_skins = new Dictionary<string, string>();
		}

		protected override void OnGameStart()
		{
			Quake.OnClientStringCmd += Quake_OnStringCmd;
			Quake.OnServerPlayerInfo += Quake_OnPlayerInfo;
			GetPlugin<PAction>().OnPlayerChangeName += ShowPlayer_OnPlayerChangeName;
			GetPlugin<PAction>().OnConnectedToServer += UpdateSkins;
		}

		void UpdateSkins(Action sender, System.EventArgs e)
		{
			foreach (var v in _skins)
			{
				Player player = GetPlugin<PAction>().GetPlayerByName(v.Key);
				if(player != null)
					Quake.SendToClient(new PlayerInfo(player.Id, player.Name, "male", v.Value));
			}
		}

		void ShowPlayer_OnPlayerChangeName(Action sender, PlayerChangeNameEventArgs e)
		{
			if (_skins.ContainsKey(e.OldName))
			{
				string oldSkin = _skins[e.OldName];
				_skins.Remove(e.OldName);
				_skins.Add(e.Player.Name, oldSkin);
				Quake.SendToClient(new PlayerInfo(e.Player.Id, e.Player.Name, "male", oldSkin));
			}
		}

		void Quake_OnPlayerInfo(Quake sender, ServerCommandEventArgs<PlayerInfo> e)
		{
			if (_skins.ContainsKey(e.Command.Name))
				e.Command.Skin = _skins[e.Command.Name];
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
						var player = GetPlugin<PAction>().PlayersById[playerId];
						Quake.SendToClient(new PlayerInfo(player.Id, player.Name, "male", showSkin));
						if (!_skins.ContainsKey(player.Name))
							_skins.Add(player.Name, showSkin);
						else
							_skins[player.Name] = showSkin;
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
				if (_skins.ContainsKey(player.Name))
					playersList.Append(" " + player.Id + "  " + player.Name + "  " + _skins[player.Name] + "\n");
				else
					playersList.Append(" " + player.Id + "  " + player.Name + "\n");
			}

			return playersList + "\n";
		}
	}
}
