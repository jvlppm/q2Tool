﻿using System;
using q2Tool.Commands;

namespace q2Tool
{
	public class CommandEventArgs : EventArgs { }
	public delegate void CommandEventHandler(Quake sender, CommandEventArgs e);

	public class CommandEventArgs<CommandType> : CommandEventArgs where CommandType : ICommand
	{
		public CommandEventArgs(CommandType command)
		{
			Command = command;
		}

		public CommandType Command { get; set; }
	}
	public class ServerCommandEventArgs<CommandType> : CommandEventArgs<CommandType> where CommandType : IServerCommand
	{
		public ServerCommandEventArgs(CommandType command) : base(command) { }
	}
	public class ClientCommandEventArgs<CommandType> : CommandEventArgs<CommandType> where CommandType : IClientCommand
	{
		public ClientCommandEventArgs(CommandType command) : base(command) { }
	}

	public delegate void CommandEventHandler<CommandType>(Quake sender, CommandEventArgs<CommandType> e) where CommandType : class, ICommand;
	public delegate void ServerCommandEventHandler<CommandType>(Quake sender, ServerCommandEventArgs<CommandType> e) where CommandType : class, IServerCommand;
	public delegate void ClientCommandEventHandler<CommandType>(Quake sender, ClientCommandEventArgs<CommandType> e) where CommandType : class, IClientCommand;

    public class ConnectionPackageEventArgs<CommandsType> : EventArgs where CommandsType : ICommand
	{
        public Package<CommandsType> Package { get; private set; }

        public ConnectionPackageEventArgs(Package<CommandsType> package)
		{
			Package = package;
		}
	}
    public delegate void ConnectionPackageEventHandler<CommandsType>(Quake sender, ConnectionPackageEventArgs<CommandsType> e) where CommandsType : ICommand;

	public static class CommandEventExtensions
	{
		public static void Fire<CommandType>(this CommandEventHandler<CommandType> eventHandler, Quake sender, CommandType command) where CommandType : class, ICommand
		{
			try
			{
				if (eventHandler != null)
					eventHandler(sender, new CommandEventArgs<CommandType>(command));
			}
			catch {}
		}

		public static void Fire<CommandType>(this ServerCommandEventHandler<CommandType> eventHandler, Quake sender, CommandType command) where CommandType : class, IServerCommand
		{
			try
			{
				if (eventHandler != null)
					eventHandler(sender, new ServerCommandEventArgs<CommandType>(command));
			}
			catch { }
		}

		public static void Fire<CommandType>(this ClientCommandEventHandler<CommandType> eventHandler, Quake sender, CommandType command) where CommandType : class, IClientCommand
		{
			try
			{
				if (eventHandler != null)
					eventHandler(sender, new ClientCommandEventArgs<CommandType>(command));
			}
			catch { }
		}

        public static void Fire<CommandsType>(this ConnectionPackageEventHandler<CommandsType> eventHandler, Quake sender, Package<CommandsType> package) where CommandsType : ICommand
		{
			try
			{
				if (eventHandler != null)
					eventHandler(sender, new ConnectionPackageEventArgs<CommandsType>(package));
			}
			catch { }
		}
	}
}
