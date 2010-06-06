using q2Tool.Commands.Server;
using System;

namespace q2Tool
{
	public class SpeedJump : Plugin
	{
		bool _ignore;

		protected override void OnGameStart()
		{
			GetPlugin<PAction>().OnConnectedToServer += Activate;
			Quake.OnClientUserInfo += Quake_OnUserInfo;
		}

		void Quake_OnUserInfo(Quake sender, ClientCommandEventArgs<Commands.Client.UserInfo> e)
		{
			if (_ignore)
			{
				e.Command.Message = string.Empty;
				_ignore = false;
			}
			else if (e.Command["timescale"] != null && e.Command["timescale"] != "1")
			{
				e.Command.Message = string.Empty;
				_ignore = true;
			}
		}

		void Activate(Action sender, EventArgs e)
		{
			Quake.SendToClient(new ConfigString(ConfigStringType.MaxClients, "1"));
			Quake.ExecuteCommand("alias +sjump \"timescale 3.5;+moveup\"");
			Quake.ExecuteCommand("alias -sjump \"timescale 1;-moveup\"");
		}
	}
}
