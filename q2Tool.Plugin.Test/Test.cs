using System;
using Jv.Plugins;
using q2Tool.Commands.Server;

namespace q2Tool
{
	public class Test : Plugin
	{
		protected override void OnGameStart()
		{
			Quake.OnClientStringPackage += Quake_OnClientStringPackage;
			Quake.OnServerStringPackage += Quake_OnServerStringPackage;
		}

		void Quake_OnServerStringPackage(Quake sender, ServerCommandEventArgs<IServerStringPackage> e)
		{
			string message;
			if (e.Command.Type == Commands.ServerCommand.ConfigString)
			{
				message = "Server: " + e.Command.Type + " (" + ((ConfigString)e.Command).ConfigType +  "): " + e.Command.Message;
			}
			else
				message = "Server: " + e.Command.Type + ": " + e.Command.Message;
			MessageToPlugin<PLog>((object)message);
			Console.WriteLine(message);
		}

		void Quake_OnClientStringPackage(Quake sender, ClientCommandEventArgs<IClientStringPackage> e)
		{
			string message = "Client: " + e.Command.Type + ": " + e.Command.Message;
			MessageToPlugin<PLog>((object)message);
			Console.WriteLine(message);
		}
	}
}
