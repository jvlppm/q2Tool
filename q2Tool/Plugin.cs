namespace q2Tool
{
	public abstract class Plugin : Jv.Plugins.Plugin
	{
		internal protected Quake Quake { get; set; }
		internal protected abstract void OnGameStart();
	}
}
