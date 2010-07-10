using System;
using Jv.Plugins;
using System.Linq;
using Update;
using System.Diagnostics;
using System.IO;
using Jv.Threading;

namespace q2Tool
{
	public class Program
	{
		static readonly Manager PluginManager = new Manager();

		static void BackgroundUpdateCheck()
		{
			Parallel.Start(() =>
			{
				try
				{
					var updater = new Updater(Settings.ReadValue("Update", "UpdateUrl"));
					if (MainUpdateRequired(updater))
					{
						Settings.WriteValue("Update", "CheckUpdates", "True");
						return;
					}
					EnableOnlyPlugins(updater);
					if (updater.GetUpdateFileList().Count > 0)
						Settings.WriteValue("Update", "CheckUpdates", "Plugins");
				}
				catch
				{
				}
			});
		}

		public static void Main(string[] args)
		{
			try
			{
				if (!args.ToList().Contains("--disable-updates"))
				{
					bool msgOk = false;
					if (Settings.ReadValue("Update", "CheckUpdates") == "True" || Settings.ReadValue("Update", "CheckUpdates") == string.Empty)
					{
						msgOk = true;
						Console.Write("Checking updates...");
						var updater = new Updater(Settings.ReadValue("Update", "UpdateUrl"));
						Settings.WriteValue("Update", "CheckUpdates", "Plugins");

						if (MainUpdateRequired(updater))
						{
							Console.WriteLine("Update required!");
							updater = new Updater(Settings.ReadValue("Update", "UpdaterUrl"));
							foreach (var file in updater.GetUpdateFileList())
								updater.DownloadFile(file);

							Process.Start("Update.exe", "--wait-process q2Tool --auto-execute \"q2Tool.exe\"");
							return;
						}
					}
					if (Settings.ReadValue("Update", "CheckUpdates") == "Plugins")
					{
						if(!msgOk)
							Console.Write("Checking updates...");
						var updater = new Updater(Settings.ReadValue("Update", "UpdateUrl"));
						Settings.WriteValue("Update", "CheckUpdates", "False");
						EnableOnlyPlugins(updater);
						var pluginFiles = updater.GetUpdateFileList();
						if (pluginFiles.Count > 0)
						{
							Console.Write("Downloading plugins..");
							foreach (var plugin in pluginFiles)
							{
								Console.Write(".");
								updater.DownloadFile(plugin);
							}
						}
						Console.WriteLine("OK");
					}
					else
					{
						BackgroundUpdateCheck();
					}
				}
				else
				{
					BackgroundUpdateCheck();
				}

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
					try
					{
						plugin.Quake = quake;
						plugin.GameStart();
					}
					catch (Exception ex)
					{
						Console.WriteLine("Plugin {0} could not be loaded: {1}.", plugin.GetType().Name, ex.Message);
						PluginManager.UnLoadPlugin(plugin);
					}
				}
			}
			catch (FileNotFoundException ex)
			{
				Settings.WriteValue("Update", "CheckUpdates", "True");
				Console.WriteLine("O arquivo \"{0}\" não foi encontrado.", ex.FileName);
				PluginManager.MessageToPlugin<PLog>(ex);
			}
			catch (Exception ex)
			{
				Settings.WriteValue("Update", "CheckUpdates", "True");
				Exception inner = ex.InnerException;
				Console.WriteLine("Error: {0}", ex.Message);
				while (inner != null)
				{
					Console.WriteLine(inner.Message);
					inner = inner.InnerException;
				}
				PluginManager.MessageToPlugin<PLog>(ex);
			}
		}

		private static void EnableOnlyPlugins(Updater updater)
		{
			foreach (var module in updater.Modules)
			{
				if (module.Name.StartsWith("q2Tool.Plugin."))
					module.Enabled = Settings.ReadValue("q2Tool.Plugin", module.Name.Substring(14)).ToLower() == "enabled";
				else
					module.Enabled = false;
			}
		}

		static bool MainUpdateRequired(Updater updater)
		{
			foreach (var module in updater.Modules)
			{
				if (module.Name.StartsWith("q2Tool.Plugin."))
				{
					module.Enabled = false;
					if (module.Enabled && Settings.ReadValue("q2Tool.Plugin", module.Name.Substring(14)) == string.Empty)
						Settings.WriteValue("q2Tool.Plugin", module.Name.Substring(14), "enabled");
				}
			}

			return updater.GetUpdateFileList().Count > 0;
		}

		private static void LoadPlugins()
		{
			foreach (string dll in System.IO.Directory.GetFiles(".", "q2Tool.Plugin.*.dll"))
			{
				string fileName = dll.Replace('\\', '/').Replace("./", "");
				try
				{
					string pluginName = fileName.Substring(14, fileName.LastIndexOf('.') - 14);
					string pluginStatus = Settings.ReadValue("q2Tool.Plugin", pluginName);

					if (!string.IsNullOrEmpty(pluginStatus) && pluginStatus.ToLower() != "enabled")
					{
						Settings.WriteValue("q2Tool.Plugin", pluginName, "disabled");
						continue;
					}
					Settings.WriteValue("q2Tool.Plugin", pluginName, "enabled");

					PluginManager.LoadPlugin<Plugin>(fileName);
				}
				catch (Jv.Plugins.Exceptions.CouldNotInstantiate) { throw; }
				catch (Jv.Plugins.Exceptions.CouldNotLoad) { }
			}
		}
	}
}
