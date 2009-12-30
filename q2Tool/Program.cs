using System;
using Jv.Plugins;

namespace q2Tool
{
	class Program
	{
		static readonly Manager PluginManager = new Manager();

		static void Main(string[] args)
		{
			try
			{
				LoadPlugins();

				var quake = new Quake(Settings.ReadValue("Game", "Path"))
				{
					CFG = Settings.ReadValue("Game", "CFG")
				};

				string server = Settings.ReadValue("Game", "DefaultServer");

				for (int i = 0; i < args.Length; i++)
				{
					if (args[i] == "+connect")
					{
						server = args[++i];
						break;
					}
				}

				quake.Run(server);

				foreach (Plugin plugin in PluginManager.GetPlugins<Plugin>())
				{
					plugin.Quake = quake;
					plugin.OnGameStart();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error: {0}", ex.Message);
				PluginManager.MessageToPlugin<PLog>(ex);
			}
		}

		private static void LoadPlugins()
		{
			foreach (string dll in System.IO.Directory.GetFiles(".", "q2Tool.Plugin.*.dll"))
			{
				string fileName = dll.Replace('\\', '/').Replace("./", "");
				try
				{
					string pluginName = fileName.Substring(14, fileName.LastIndexOf('.') - 14);
					string pluginStatus = Settings.ReadValue("q2Tool.Plugins", pluginName);

					if (!string.IsNullOrEmpty(pluginStatus) && pluginStatus.ToLower() != "enabled")
					{
						Settings.WriteValue("q2Tool.Plugins", pluginName, "disabled");
						continue;
					}
					Settings.WriteValue("q2Tool.Plugins", pluginName, "enabled");

					PluginManager.LoadPlugin<Plugin>(fileName);
				}
				catch (Jv.Plugins.Exceptions.CouldNotInstantiate) { throw; }
				catch (Jv.Plugins.Exceptions.CouldNotLoad) { }
			}
		}
	}
}
