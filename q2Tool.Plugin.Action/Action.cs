using System.Collections.Generic;
using q2Tool.Commands.Server;
using System;
using System.Linq;

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
		public abstract event PlayerEventHandler OnPlayerDisconnected;

		public Player GetPlayerByName(string name)
		{
			var players = Players.Where(p => p.Name == name);

			if (players.Count() == 0)
				return null;

			if (players.Count() > 1)
				return new Player(players.First().Name, -1);
			
			return players.First();
		}

		protected void RemPlayerByName(string name)
		{
			Player player = GetPlayerByName(name);
			if (player != null)
				PlayersById.Remove(player.Id);
		}

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
		public override event PlayerEventHandler OnPlayerDisconnected;

		Dictionary<int, Player> _newPlayersById;

		public Action()
		{
			PlayersById = new Dictionary<int, Player>();
		}
		protected override void OnGameStart()
		{
			Quake.OnServerPrint += Quake_OnServerPrint;
			Quake.OnServerCenterPrint += Quake_OnServerCenterPrint;
			Quake.OnServerPlayerInfo += (s, e) => AddPlayer(e.Command.Id, e.Command.Name);
			OnConnectedToServer += (s, e) => UpdatePlayerList();
			OnServerMessage += Action_OnServerMessage;
		}

		void Action_OnServerMessage(Action sender, ServerMessageEventArgs e)
		{
			if (OnRoundEnd != null)
			{
				if (e.Message.Contains("is over"))
					OnRoundEnd(this, EventArgs.Empty);
			}
			if(e.Message.EndsWith(" disconnected\n"))
			{
				string nick = e.Message.Substring(0, e.Message.Length - 14);
				Player player = GetPlayerByName(nick);
				if (player != null)
				{
					PlayersById.Remove(player.Id);

					if (OnPlayerDisconnected != null)
						OnPlayerDisconnected(this, new PlayerEventArgs(player));
				}
				else UpdatePlayerList();
			}
		}

		void AddPlayer(int id, string name)
		{
			if (!PlayersById.ContainsKey(id))
			{
				Player player = new Player(name, id);
				PlayersById.Add(id, player);
			}
			else if (PlayersById[id].Name != name)
			{
				string oldName = PlayersById[id].Name;
				PlayersById[id].Name = name;
				if (OnPlayerChangeName != null)
					OnPlayerChangeName(this, new PlayerChangeNameEventArgs(oldName, PlayersById[id]));
			}
		}

		void UpdatePlayerList()
		{
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
				string playerName = message.Substring(0, message.LastIndexOf(' ')).Trim(' ');
				
				Player newPlayer = new Player(playerName, playerNumber);
				_newPlayersById.Add(playerNumber, newPlayer);

				e.Command.Message = null;
			}
			else if (message == "PlayerID  Name                  Accuracy\n")
				e.Command.Message = null;
			else if (message == "\n  Use \"stats <PlayerID>\" for\n  individual stats\n\n")
			{
				e.Command.Message = null;
				Quake.OnServerPrint -= GetPlayerList;

				PlayersById = _newPlayersById;
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
					OnServerMessage(this, new ServerMessageEventArgs(e.Command.Message, e.Command.Level));
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
					bool team = nick.StartsWith("(") && nick.EndsWith(")") && GetPlayerByName(nick) == null;
					if (team)
						nick = nick.Substring(1, nick.Length - 2);
					if (nick.EndsWith(" "))
					{
						Player fixPlayer = GetPlayerByName(nick.Trim(' '));
						if(fixPlayer != null)
							fixPlayer.Name = nick;
					}

					message = message.Substring(message.IndexOf(": ") + 2).Replace("\n", "").Replace("\r", "");
					Player player = GetPlayerByName(nick);
					if (player != null)
						OnPlayerMessage(this, new PlayerMessageEventArgs(player, message, dead, team));
				}
			}
		}
	}
}
