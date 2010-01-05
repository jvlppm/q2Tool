using System.Collections.Generic;
using q2Tool.Commands.Server;
using System;
namespace q2Tool
{
	public abstract class PAction : Plugin
	{
		public abstract event PlayerMessageEventHandler OnPlayerMessage;
		public abstract event ServerMessageEventHandler OnServerMessage;
		public abstract event RoundBeginEventHandler OnRoundBegin;
		public abstract event RoundEndEventHandler OnRoundEnd;
		public abstract event ConnectedToServerEventHandler OnConnectedToServer;
		public abstract event PlayerChangeNameEventHandler OnPlayerChangeName;

		public Dictionary<string, Player> PlayersByName { get; protected set; }
		public Dictionary<int, Player> PlayersById { get; protected set; }
		public Dictionary<int, Player>.ValueCollection Players { get { return PlayersById.Values; } }
	}

	public class Action : PAction
	{
		public override event PlayerMessageEventHandler OnPlayerMessage;
		public override event ServerMessageEventHandler OnServerMessage;
		public override event RoundBeginEventHandler OnRoundBegin;
		public override event RoundEndEventHandler OnRoundEnd;
		public override event ConnectedToServerEventHandler OnConnectedToServer;
		public override event PlayerChangeNameEventHandler OnPlayerChangeName;

		Dictionary<string, Player> _newPlayersByName;
		Dictionary<int, Player> _newPlayersById;

		public Action()
		{
			PlayersByName = new Dictionary<string, Player>();
			PlayersById = new Dictionary<int, Player>();
		}
		protected override void OnGameStart()
		{
			Quake.OnServerPrint += Quake_OnServerPrint;
			Quake.OnServerCenterPrint += Quake_OnServerCenterPrint;
			Quake.OnServerPlayerInfo += (s, e) => AddPlayer(e.Command.Id, e.Command.Name);
			OnConnectedToServer += (s, e) => UpdatePlayerList();
		}

		void AddPlayer(int id, string name)
		{
			if (!PlayersById.ContainsKey(id))
			{
				Player player = new Player(name, id);
				PlayersById.Add(id, player);
				PlayersByName.Add(name, player);
			}
			else if (PlayersById[id].Name != name)
			{
				string oldName = PlayersById[id].Name;
				PlayersById[id].Name = name;
				PlayersByName.Remove(oldName);
				PlayersByName.Add(name, PlayersById[id]);
				if (OnPlayerChangeName != null)
					OnPlayerChangeName(this, new PlayerChangeNameEventArgs(oldName, PlayersById[id]));
			}
		}

		void UpdatePlayerList()
		{
			_newPlayersByName = new Dictionary<string, Player>();
			_newPlayersById = new Dictionary<int, Player>();

			Quake.OnServerPrint += GetPlayerList;
			Quake.ExecuteCommand("stats list");
		}

		void GetPlayerList(Quake sender, CommandEventArgs<Print> e)
		{
			string message = e.Command.Message;
			if (e.Command.Message.StartsWith(" "))
			{
				message = message.Trim(' ');
				int playerNumber = int.Parse(message.Substring(0, message.IndexOf(' ')));
				message = message.Substring(message.IndexOf(' ')).Trim(' ');
				string playerName = message.Substring(0, message.IndexOf(' '));

				Player newPlayer = new Player(playerName, playerNumber);
				_newPlayersByName.Add(playerName, newPlayer);
				_newPlayersById.Add(playerNumber, newPlayer);

				e.Command.Message = null;
			}
			else if (message == "PlayerID  Name                  Accuracy\n")
				e.Command.Message = null;
			else if (message == "\n  Use \"stats <PlayerID>\" for\n  individual stats\n\n")
			{
				e.Command.Message = null;
				Quake.OnServerPrint -= GetPlayerList;

				if (OnPlayerChangeName != null)
				{
					foreach (var newPlayer in _newPlayersByName.Values)
					{
						foreach (var player in Players)
						{
							if (player.Id == newPlayer.Id)
							{
								if (newPlayer.Name != player.Name)
									OnPlayerChangeName(this, new PlayerChangeNameEventArgs(player.Name, newPlayer));
								break;
							}
						}
					}
				}

				PlayersByName = _newPlayersByName;
				PlayersById = _newPlayersById;
				_newPlayersByName = null;
				_newPlayersById = null;
			}
			else if(message.StartsWith("\n"))
			{
				e.Command.Message = null;
			}
		}

		void Quake_OnServerCenterPrint(Quake sender, ServerCommandEventArgs<CenterPrint> e)
		{
			if(OnRoundBegin != null)
			{
				if (e.Command.Message == "LIGHTS...\n")
					OnRoundBegin(this, EventArgs.Empty);
			}
		}

		void Quake_OnServerPrint(Quake sender, ServerCommandEventArgs<Print> e)
		{
			string message = e.Command.Message;

			if (e.Command.Level != Print.PrintLevel.Chat)
			{
				if (OnServerMessage != null)
				{
					OnServerMessage(this, new ServerMessageEventArgs(e.Command.Message, e.Command.Level));
				}

				if (OnRoundEnd != null)
				{
					if (message.Contains("is over"))
						OnRoundEnd(this, EventArgs.Empty);
				}

				if (OnPlayerChangeName != null)
				{
					if (message.Contains("changed name to ") && message.EndsWith(".\n"))
					{
						message = message.Replace(" changed name to ", "¬").TrimEnd('.', '\n');
						string[] nicks = message.Split('¬');
						if (nicks[0] != "pwsnskle" && nicks[1] != "pwsnskle")
						{
							if (PlayersByName.ContainsKey(nicks[0]))
							{
								Player player = PlayersByName[nicks[0]];
								player.Name = nicks[1];
								OnPlayerChangeName(this, new PlayerChangeNameEventArgs(nicks[0], player));
							}
						}
					}
				}
			}
			else
			{
				if (message.StartsWith("console: "))
				{
					if (OnConnectedToServer != null)
					{
						if (message.Contains("Welcome to "))
						{
							OnConnectedToServer(this, EventArgs.Empty);
						}
					}
				}
				else if (OnPlayerMessage != null)
				{
					bool dead = false;
					if (message.StartsWith("[DEAD] "))
					{
						message = message.Substring(7);
						dead = true;
					}

					string nick = message.Substring(0, message.IndexOf(": "));
					message = message.Substring(message.IndexOf(": ") + 2).Replace("\n", "").Replace("\r", "");
					if (PlayersByName.ContainsKey(nick))
						OnPlayerMessage(this, new PlayerMessageEventArgs(PlayersByName[nick], message, dead));
				}
			}
		}
	}
}
