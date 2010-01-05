using System;
using Jv.Plugins;

namespace q2Tool
{
	public abstract class Plugin : Jv.Plugins.Plugin
	{
		internal protected Quake Quake { get; set; }
		internal void GameStart()
		{
			try
			{
				OnGameStart();
			}
			catch (Exception ex)
			{
				MessageToPlugin<PLog>(ex);
				throw;
			}
		}
		protected abstract void OnGameStart();
	}
}
