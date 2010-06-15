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
		public abstract event PlayerDiedEventHandler OnPlayerDied;

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
		public abstract Player CurrentPlayer { get; }
		public abstract bool RoundActive { get; }
	}

	public class Action : PAction
	{
		bool _connected, _roundActive;

		public override event PlayerMessageEventHandler OnPlayerMessage;
		public override event ServerMessageEventHandler OnServerMessage;
		public override event RoundBeginEventHandler OnRoundBegin;
		public override event RoundEndEventHandler OnRoundEnd;
		public override event ConnectedToServerEventHandler OnConnectedToServer;
		public override event PlayerChangeNameEventHandler OnPlayerChangeName;
		public override event PlayerEventHandler OnPlayerDisconnected;
		public override event PlayerDiedEventHandler OnPlayerDied;

		public override bool RoundActive
		{
			get { return _roundActive; }
		}

		public override Player CurrentPlayer
		{
			get
			{
				if (!PlayersById.ContainsKey(_playerNum))
					return null;
				return PlayersById[_playerNum];
			}
		}

		Dictionary<int, Player> _newPlayersById;
		int _playerNum;

		public Action()
		{
			PlayersById = new Dictionary<int,Player>();
		}
		protected override void OnGameStart()
		{
			Quake.OnServerData += Quake_OnServerData;
			Quake.OnServerPrint += Quake_OnServerPrint;
			Quake.OnServerCenterPrint += Quake_OnServerCenterPrint;
			Quake.OnServerPlayerInfo += (s, e) => AddPlayer(e.Command.Id, e.Command.Name);
			OnServerMessage += Action_OnServerMessage;
		}

		void Quake_OnServerData(Quake sender, ServerCommandEventArgs<ServerData> e)
		{
			_playerNum = e.Command.PlayerNum;
		}

		void Action_OnServerMessage(Action sender, ServerMessageEventArgs e)
		{
			if (e.Message.Contains("is over"))
			{
				_roundActive = false;
				if (OnRoundEnd != null)
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
				var player = new Player(name, id);
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
				
				Player newPlayer = GetPlayerByName(playerName);
				if (newPlayer == null || newPlayer.Id < 0) newPlayer = new Player(playerName, playerNumber);
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

				if (OnConnectedToServer != null && !_connected)
					OnConnectedToServer(this, EventArgs.Empty);

				_connected = true;
				_newPlayersById = null;
			}
			else if(message.StartsWith("\n"))
			{
				e.Command.Message = null;
			}
		}

		void Quake_OnServerCenterPrint(Quake sender, ServerCommandEventArgs<CenterPrint> e)
		{
			if (e.Command.Message == "LIGHTS...\n")
			{
				_roundActive = true;
				if (OnRoundBegin != null)
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

				if(e.Command.Level == Print.PrintLevel.Medium)
					CheckPlayerDeath(e.Command.Message);
			}
			else
			{
				if (message.StartsWith("console: "))
				{
					if (message.Contains("Welcome to "))
					{
						_connected = false;
						UpdatePlayerList();
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
						OnPlayerMessage(this, new PlayerMessageEventArgs(player, message, dead, team, Players));
				}
			}
		}

		static List<string> deathMessages = new List<string>();

		private void CheckPlayerDeath(string serverFullMessage)
		{
			var ignore = new List<string> { "changed name to", "disconnected", "entered the game", "voted for", "joined", "is now known as" };
			if (OnPlayerDied == null || ignore.Exists(s => serverFullMessage.Contains(s)))
				return;


			Player first = Players.OrderByDescending(p => p.Name.Length)
							.Where(p => serverFullMessage.StartsWith(p.Name))
							.FirstOrDefault();

			if (first == null)
				return;

			string messageEnd = serverFullMessage.Substring(first.Name.Length);

			Player second = Players.OrderByDescending(p => p.Name.Length)
					.Where(p => messageEnd.Contains(p.Name))
					.FirstOrDefault() ?? first;

			string deathMessage = serverFullMessage.Replace(first.Name, "{0}").Replace(second.Name, "{1}").Replace("\n", "");

			switch (deathMessage)
			{
				#region Environment
				case "{0} ate too much glass":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.BreakingGlass, HitLocation.Unknown));
					break;

				case "{0} is done with the world":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Suicide, HitLocation.Unknown));
					break;

				case "{0} plummets to its death":
				case "{0} plummets to her death":
				case "{0} plummets to his death":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, null, MeansOfDeath.Falling, HitLocation.Unknown));
					break;

				case "{0} was flattened":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, null, MeansOfDeath.Crush, HitLocation.Unknown));
					break;

				case "{0} sank like a rock":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, null, MeansOfDeath.Water, HitLocation.Unknown));
					break;

				case "{0} melted":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, null, MeansOfDeath.Slime, HitLocation.Unknown));
					break;

				case "{0} does a back flip into the lava":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, null, MeansOfDeath.Lava, HitLocation.Unknown));
					break;

				case "{0} blew up":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, null, MeansOfDeath.Explosion, HitLocation.Unknown));
					break;

				case "{0} found a way out":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, null, MeansOfDeath.Exit, HitLocation.Unknown));
					break;

				case "{0} saw the light":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, null, MeansOfDeath.Laser, HitLocation.Unknown));
					break;

				case "{0} got blasted":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, null, MeansOfDeath.Blaster, HitLocation.Unknown));
					break;

				case "{0} was in the wrong place":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, null, MeansOfDeath.WrongPlace, HitLocation.Unknown));
					break;

				#endregion

				#region Suicide
				case "{0} tried to put the pin back in":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, first, MeansOfDeath.HeldGrenade, HitLocation.Unknown));
					break;
				case "{0} didn't throw his grenade far enough":
				case "{0} didn't throw her grenade far enough":
				case "{0} didn't throw its grenade far enough":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, first, MeansOfDeath.HandGrenade, HitLocation.Unknown));
					break;
				case "{0} killed himself":
				case "{0} killed herself":
				case "{0} killed itself":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, first, MeansOfDeath.Unknown, HitLocation.Unknown));
					break;
				#endregion

				#region Pistol
				case "{0} has a hole in his head from {1}'s Mark 23 pistol":
				case "{0} has a hole in her head from {1}'s Mark 23 pistol":
				case "{0} has a hole in its head from {1}'s Mark 23 pistol":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Pistol, HitLocation.Head));
					break;

				case "{0} loses a vital chest organ thanks to {1}'s Mark 23 pistol":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Pistol, HitLocation.Chest));
					break;

				case "{0} loses his lunch to {1}'s .45 caliber pistol round":
				case "{0} loses her lunch to {1}'s .45 caliber pistol round":
				case "{0} loses its lunch to {1}'s .45 caliber pistol round":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Pistol, HitLocation.Stomach));
					break;
				case "{0} is legless because of {1}'s .45 caliber pistol round":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Pistol, HitLocation.Legs));
					break;
				case "{0} was shot by {1}'s Mark 23 Pistol":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Pistol, HitLocation.Unknown));
					break;
				#endregion

				#region Mp5
				case "{0}'s brains are on the wall thanks to {1}'s 10mm MP5/10 round":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Mp5, HitLocation.Head));
					break;
				case "{0} feels some chest pain via {1}'s MP5/10 Submachinegun":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Mp5, HitLocation.Chest));
					break;

				case "{0} needs some Pepto Bismol after {1}'s 10mm MP5 round":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Mp5, HitLocation.Stomach));
					break;

				case "{0} had his legs blown off thanks to {1}'s MP5/10 Submachinegun":
				case "{0} had her legs blown off thanks to {1}'s MP5/10 Submachinegun":
				case "{0} had its legs blown off thanks to {1}'s MP5/10 Submachinegun":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Mp5, HitLocation.Legs));
					break;

				case "{0} was shot by {1}'s MP5/10 Submachinegun":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Mp5, HitLocation.Unknown));
					break;
				#endregion

				#region M4
				case "{0} had a makeover by {1}'s M4 Assault Rifle":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.M4, HitLocation.Head));
					break;

				case "{0} feels some heart burn thanks to {1}'s M4 Assault Rifle":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.M4, HitLocation.Chest));
					break;

				case "{0} has an upset stomach thanks to {1}'s M4 Assault Rifle":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.M4, HitLocation.Stomach));
					break;

				case "{0} is now shorter thanks to {1}'s M4 Assault Rifle":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.M4, HitLocation.Legs));
					break;

				case "{0} was shot by {1}'s M4 Assault Rifle":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.M4, HitLocation.Unknown));
					break;
				#endregion

				#region M3

				case "{0} is full of buckshot from {1}'s M3 Super 90 Assault Shotgun":
				case "{0} accepts {1}'s M3 Super 90 Assault Shotgun in hole-y matrimony":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.M3, HitLocation.Unknown));
					break;

				#endregion

				#region HandCannon

				case "{0} underestimated {1}'s single barreled handcannon shot":
				case "{0} won't be able to pass a metal detector anymore thanks to {1}'s single barreled handcannon shot":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.HandCannonSingle, HitLocation.Unknown));
					break;

				case "{0} ate {1}'s sawed-off 12 gauge":
				case "{0} is full of buckshot from {1}'s sawed off shotgun":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.HandCannonDouble, HitLocation.Unknown));
					break;

				#endregion

				#region Sniper

				case "{0} caught a sniper bullet between the eyes from {1}":
				case "{0} saw the sniper bullet go through his scope thanks to {1}":
				case "{0} saw the sniper bullet go through her scope thanks to {1}":
				case "{0} saw the sniper bullet go through its scope thanks to {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Sniper, HitLocation.Head));
					break;

				case "{0} was picked off by {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Sniper, HitLocation.Chest));
					break;

				case "{0} was sniped in the stomach by {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Sniper, HitLocation.Stomach));
					break;

				case "{0} was shot in the legs by {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Sniper, HitLocation.Legs));
					break;

				case "{0} was sniped by {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Sniper, HitLocation.Unknown));
					break;

				#endregion

				#region DualPistols

				case "{0} was trepanned by {1}'s akimbo Mark 23 pistols":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.DualPistols, HitLocation.Head));
					break;

				case "{0} was John Woo'd by {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.DualPistols, HitLocation.Chest));
					break;

				case "{0} needs some new kidneys thanks to {1}'s akimbo Mark 23 pistols":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.DualPistols, HitLocation.Stomach));
					break;

				case "{0} was shot in the legs by {1}'s akimbo Mark 23 pistols":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.DualPistols, HitLocation.Legs));
					break;

				case "{0} was shot by {1}'s pair of Mark 23 Pistols":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.DualPistols, HitLocation.Unknown));
					break;

				#endregion

				#region KnifeSlash

				case "{0} had his throat slit by {1}":
				case "{0} had her throat slit by {1}":
				case "{0} had its throat slit by {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.KnifeSlash, HitLocation.Head));
					break;

				case "{0} had open heart surgery, compliments of {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.KnifeSlash, HitLocation.Chest));
					break;

				case "{0} was gutted by {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.KnifeSlash, HitLocation.Stomach));
					break;

				case "{0} was stabbed repeatedly in the legs by {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.KnifeSlash, HitLocation.Legs));
					break;

				case "{0} was slashed apart by {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.KnifeSlash, HitLocation.Unknown));
					break;

				#endregion

				#region KnifeThrown

				case "{0} caught {1}'s flying knife with his forehead":
				case "{0} caught {1}'s flying knife with her forehead":
				case "{0} caught {1}'s flying knife with its forehead":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.KnifeThrown, HitLocation.Head));
					break;

				case "{0}'s ribs don't help against {1}'s flying knife":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.KnifeThrown, HitLocation.Chest));
					break;

				case "{0} sees the contents of its own stomach thanks to {1}'s flying knife":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.KnifeThrown, HitLocation.Stomach));
					break;

				case "{0} had his legs cut off thanks to {1}'s flying knife":
				case "{0} had her legs cut off thanks to {1}'s flying knife":
				case "{0} had its legs cut off thanks to {1}'s flying knife":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.KnifeThrown, HitLocation.Legs));
					break;

				case "{0} was hit by {1}'s flying Combat Knife":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.KnifeThrown, HitLocation.Unknown));
					break;
				#endregion

				#region Kick

				case "{0} was taught how to fly by {1}":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.TaughtToFly, HitLocation.Unknown));
					break;

				case "{0} got his ass kicked by {1}":
				case "{0} got her ass kicked by {1}":
				case "{0} got its ass kicked by {1}":

				case "{0} couldn't remove {1}'s boot from his ass":
				case "{0} couldn't remove {1}'s boot from her ass":
				case "{0} couldn't remove {1}'s boot from its ass":

				case "{0} had a Bruce Lee put on him by {1}, with a quickness":
				case "{0} had a Bruce Lee put on her by {1}, with a quickness":
				case "{0} had a Bruce Lee put on it by {1}, with a quickness":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Kick, HitLocation.Unknown));
					break;

				#endregion

				#region Punch

				case "{0} got a free facelift by {1}":
				case "{0} was knocked out by {1}":
				case "{0} caught {1}'s iron fist":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Punch, HitLocation.Unknown));
					break;

				#endregion

				#region Grenade

				case "{0} caught {1}'s handgrenade":
				case "{0} didn't see {1}'s handgrenade":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.HandGrenade, HitLocation.Unknown));
					break;

				case "{0} feels {1}'s pain":
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.HeldGrenade, HitLocation.Unknown));
					break;

				#endregion

				default:
					OnPlayerDied(this, new PlayerDiedEventArgs(first, second, MeansOfDeath.Unknown, HitLocation.Unknown));
					break;
			}
		}
	}
}
