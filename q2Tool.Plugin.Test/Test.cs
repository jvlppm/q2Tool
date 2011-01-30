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
			Quake.OnServerStuffText += Quake_OnServerStuffText;
		}

		void Quake_OnServerStuffText(Quake sender, CommandEventArgs<StuffText> e)
		{
			e.Command.Message = e.Command.Message.Replace("allow_download 1", "allow_download 0");
		}
	}
}
