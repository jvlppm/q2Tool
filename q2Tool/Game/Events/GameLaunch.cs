using System;

namespace q2Tool
{
	public class GameLaunchEventArgs : EventArgs
	{
		public GameLaunchEventArgs()
		{
			CustomArgs = string.Empty;
		}

		public void AddCustomArg(string arg)
		{
			if (CustomArgs == string.Empty)
				CustomArgs += arg;
			else
				CustomArgs += " " + arg;
		}

		internal string CustomArgs { get; set; }
	}

	public delegate void GameLaunchEventHandler(Quake sender, GameLaunchEventArgs e);
}
