using q2Tool.Commands.Server;
namespace q2Tool
{
	public class ForceProxy : Plugin
	{
		string _serverIp;

		protected override void OnGameStart()
		{
			_serverIp = Quake.Ip + ":" + Quake.Port;
			Quake.OnStuffText += QuakeOnStuffText;
		}

		void QuakeOnStuffText(Quake sender, CommandEventArgs<StuffText> e)
		{
			e.Command.Message = e.Command.Message.Replace(_serverIp, "localhost:27910");
		}
	}
}
