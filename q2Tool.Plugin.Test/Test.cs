using System.Collections.Generic;
using System.Linq;

namespace q2Tool
{
	public class Test : Plugin
	{
		private readonly Dictionary<Player, int> _kills;

		public Test()
		{
			_kills = new Dictionary<Player, int>();
		}

		protected override void OnGameStart()
		{
			GetPlugin<PAction>().OnPlayerDisconnected += Test_OnPlayerDisconnected;
			GetPlugin<PAction>().OnPlayerDied += Test_OnPlayerDied;
			GetPlugin<PAction>().OnRoundEnd += Test_OnRoundEnd;
		}

		void Test_OnRoundEnd(Action sender, System.EventArgs e)
		{
			foreach (var kill in _kills.OrderByDescending(k => k.Value))
			{
				Quake.ExecuteCommand(kill.Value >= 4 ? "say {0} - {1}" : "echo {0} - {1}", kill.Value, kill.Key.Name);
			}
		}

		void Test_OnPlayerDied(Action sender, PlayerDiedEventArgs e)
		{
			if (!_kills.ContainsKey(e.Killer))
				_kills.Add(e.Killer, 1);
			else
				_kills[e.Killer]++;

			if (_kills.ContainsKey(e.Player))
				_kills.Remove(e.Player);

			if(e.Killer != e.Player)
				Quake.ExecuteCommand("echo {0} - {1}", _kills[e.Killer], e.Killer.Name);
		}

		void Test_OnPlayerDisconnected(Action sender, PlayerEventArgs e)
		{
			if (_kills.ContainsKey(e.Player))
				_kills.Remove(e.Player);
		}
	}
}
