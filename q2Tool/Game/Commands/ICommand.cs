﻿using Jv.Networking;

namespace q2Tool.Commands
{
	public interface ICommand
	{
		int Size();
		void WriteTo(RawData package);
	}

	public interface IServerCommand : ICommand
	{
		ServerCommand Type { get; }
	}

	public interface IClientCommand : ICommand
	{
		ClientCommand Type { get; }
	}
}
