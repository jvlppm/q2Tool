using q2Tool.Commands.Server;
namespace q2Tool
{
	public class ForceProxy : Plugin
	{
		string _serverIp, _localIp;

		protected override void OnGameStart()
		{
			_serverIp = Quake.Server.Ip + ":" + Quake.Server.Port;
			_localIp = Quake.Client.Ip + ":" + Quake.Client.Port;
			Quake.OnServerStuffText += QuakeOnStuffText;
			Quake.OnClientStringCmd += Quake_OnClientStringCmd;
		}

		void Quake_OnClientStringCmd(Quake sender, ClientCommandEventArgs<Commands.Client.StringCmd> e)
		{
			e.Command.Message = e.Command.Message.Replace(_localIp, _serverIp);
		}

		void QuakeOnStuffText(Quake sender, CommandEventArgs<StuffText> e)
		{
			e.Command.Message = e.Command.Message.Replace(_serverIp, _localIp);
		}
	}
}
