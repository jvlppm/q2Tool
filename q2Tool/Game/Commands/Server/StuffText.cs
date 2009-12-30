namespace q2Tool.Commands.Server
{
	public class StuffText : StringPackage, IServerStringPackage
	{
		//[string message]
		public StuffText(RawPackage data) : base((byte)ServerCommand.StuffText, data) { }
		public StuffText(string message) : base((byte)ServerCommand.StuffText, message) { }

		#region ICommand Members
		public ServerCommand Type { get { return ServerCommand.StuffText; } }
		#endregion
	}
}
