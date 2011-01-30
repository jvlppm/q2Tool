using System;
using q2Tool.Commands;

namespace q2Tool
{
	public class CommandEventArgs<CommandType> : EventArgs where CommandType : ICommand
	{
		public CommandEventArgs(CommandType command)
		{
			Command = command;
		}
		public bool Abort { get; set; }
		public CommandType Command { get; set; }
	}
	public delegate void CommandEventHandler<CommandType>(Quake sender, CommandEventArgs<CommandType> e) where CommandType : class, ICommand;

	public static class CommandEventExtensions
	{
		public static bool Check<CommandType>(this CommandEventHandler<CommandType> eventHandler, Quake sender, CommandType command) where CommandType : class, ICommand
		{
			var eventArgs = new CommandEventArgs<CommandType>(command);
			try
			{
				if (eventHandler != null)
					eventHandler(sender, eventArgs);
			}
			catch {}
			return !eventArgs.Abort;
		}
	}
}
