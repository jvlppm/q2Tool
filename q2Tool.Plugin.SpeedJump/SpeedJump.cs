using q2Tool.Commands.Server;
namespace q2Tool
{
	public class SpeedJump : Plugin
	{
		bool ignore, active;

		protected override void OnGameStart()
		{
			Quake.OnServerData += (s, c) => active = false;
			Quake.OnUserInfo += Quake_OnUserInfo;
		}

		void Quake_OnUserInfo(Quake sender, ClientCommandEventArgs<Commands.Client.UserInfo> e)
		{
			if (!active)
			{
				Quake.SendToClient(new ConfigString(ConfigStringType.MaxClients, "1"));
				Quake.ExecuteCommand("alias +sjump \"timescale 3.5;+moveup\"");
				Quake.ExecuteCommand("alias -sjump \"timescale 1;-moveup\"");
				e.Command.Message = string.Empty;
				active = true;
			}

			if (ignore)
			{
				e.Command.Message = string.Empty;
				ignore = false;
			}
			else if (e.Command["timescale"] != null && e.Command["timescale"] != "1")
			{
				e.Command.Message = string.Empty;
				ignore = true;
			}
		}
	}
}
