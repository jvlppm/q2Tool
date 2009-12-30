using System;

namespace q2Tool
{
	public partial class ShowMessages
	{
		#region Show Colored Messages
		static void ShowLine(ConsoleColor color, string format, params object[] args)
		{
			string text = string.Format(format, args);
			if (format.EndsWith("\n") || text.Length % Console.WindowWidth == 0)
				Show(color, format, args);
			else
				Show(color, text + "\n");
		}

		static void Show(ConsoleColor color, string format, params object[] args)
		{
			ConsoleColor oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(format, args);
			Console.ForegroundColor = oldColor;
		}
		#endregion

		static void ShowCentralizedLine(ConsoleColor color, string message)
		{
			message = message.TrimEnd('\n');
			if (message.Contains("\n"))
			{
				string final = string.Empty;
				foreach (string line in message.Split('\n'))
				{
					if (line != string.Empty)
					{
						int left = (int)Math.Floor((double)(Console.WindowWidth - line.Length) / 2);
						int right = (int)Math.Ceiling((double)(Console.WindowWidth - line.Length) / 2);
						final += string.Format("{0}{1}{2}", new string(' ', left), line, new string(' ', right));
					}
				}
				final = final.TrimEnd('\n');
				ShowLine(color, final);
			}
			else
				ShowLine(color, message);
		}
	}
}
