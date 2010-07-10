using q2Tool.Commands.Server;
namespace q2Tool
{
	public class PlayerChangeNameEventArgs : PlayerEventArgs
	{
		public PlayerChangeNameEventArgs(string oldName, Player player, ServerCommandEventArgs<PlayerInfo> playerInfo)
			: base(player)
		{
			OldName = oldName;
			PlayerInfo = playerInfo;
		}

		public string OldName { get; private set; }
		public ServerCommandEventArgs<PlayerInfo> PlayerInfo { get; set; }
	}

	public delegate void PlayerChangeNameEventHandler(Action sender, PlayerChangeNameEventArgs e);
}
