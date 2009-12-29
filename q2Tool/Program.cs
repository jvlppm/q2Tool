using System;
using System.IO;
using Update;
using System.Linq;
using System.Diagnostics;
using Jv.Plugins;

namespace q2Tool
{
	class Program
	{
		static readonly Manager PluginManager = new Manager();

		static void Main()
		{
			#region Carrega Plugins
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
			#endregion
		}
	}
}
