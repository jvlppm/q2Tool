using System.Collections.Generic;
using System.Linq;
using q2Tool.Commands.Server;
using System;

namespace q2Tool
{
	public class Test : Plugin
	{
		protected override void OnGameStart()
		{
			Quake.OnClientSetting += new ClientCommandEventHandler<Commands.Client.Setting>(Quake_OnClientSetting);
		}

		void Quake_OnClientSetting(Quake sender, ClientCommandEventArgs<Commands.Client.Setting> e)
		{
			Console.WriteLine("Setting: {0} = {1}", e.Command.SubType, e.Command.Value);
		}
	}
}
