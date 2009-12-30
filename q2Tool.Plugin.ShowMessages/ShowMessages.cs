using q2Tool.Commands.Server;
using System;

namespace q2Tool
{
	public partial class ShowMessages : Plugin
	{
		readonly static object SyncLines = new object();

		protected override void OnGameStart()
		{
			Quake.OnServerData += QuakeOnServerData;
			Quake.OnPrint += QuakeOnPrint;
			Quake.OnCenterPrint += QuakeOnCenterPrint;
		}

		static void QuakeOnServerData(Quake sender, ServerCommandEventArgs<ServerData> e)
		{
			lock (SyncLines)
			{
				ShowLine(ConsoleColor.DarkGreen, "******************************");
				ShowLine(ConsoleColor.DarkGreen, e.Command.LevelName);
			}
		}

		static void QuakeOnCenterPrint(Quake sender, ServerCommandEventArgs<CenterPrint> e)
		{
			ConsoleColor messageColor;
			switch (e.Command.Message)
			{
				case "LIGHTS...\n":
					messageColor = ConsoleColor.Green;
					break;
				case "CAMERA...\n":
					messageColor = ConsoleColor.Yellow;
					break;
				case "ACTION!\n":
					messageColor = ConsoleColor.Red;
					break;
				default:
					messageColor = ConsoleColor.DarkMagenta;
					break;
			}

			lock (SyncLines)
				ShowCentralizedLine(messageColor, e.Command.Message);
		}

		static void QuakeOnPrint(Quake sender, ServerCommandEventArgs<Print> e)
		{
			if (e.Command.Level == Print.PrintLevel.Chat)
			{
				string message = e.Command.Message;
				bool deadMessage = message.StartsWith("[DEAD] ");

				if (deadMessage)
					message = message.Substring(7);
				bool teamMessage = message.StartsWith("(");

				string player = message.Split(':')[0];
				if (teamMessage) player = player.Trim('(', ')');

				message = string.Join(":", message.Split(':'), 1, message.Split(':').Length - 1).Trim(' ');

				ConsoleColor nickColor = deadMessage ? ConsoleColor.DarkGray : ConsoleColor.Gray;
				lock (SyncLines)
				{
					Show(nickColor, " {1}{0}{2}: ", player, teamMessage ? "(" : "", teamMessage ? ")" : "");
					ShowLine(ConsoleColor.DarkGray, message);
				}
			}
			else if (e.Command.Level == Print.PrintLevel.High)
			{
				lock (SyncLines)
					ShowLine((ConsoleColor)e.Command.Level + 1, e.Command.Message);
			}
		}
	}
}
