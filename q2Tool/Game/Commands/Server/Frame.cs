using System;

namespace q2Tool.Commands.Server
{
	public class Frame : IServerCommand
	{
		public Frame(RawPackage serverPackage, int extrabits)
		{
			throw new NotImplementedException();
		}

		#region ICommand
		public int Size()
		{
			throw new NotImplementedException();
		}

		public void WriteTo(RawPackage data)
		{
			throw new NotImplementedException();
		}

		public ServerCommand Type { get { return ServerCommand.Frame; } }
		#endregion
	}
}