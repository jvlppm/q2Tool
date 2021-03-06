using Jv.Networking;

namespace q2Tool.Commands.Client
{
	public class StringCmd : StringPackage, IClientStringPackage
	{
		//[string message]
        public StringCmd(RawData data) : base((byte)ClientCommand.StringCmd, data) { }
		public StringCmd(string message) : base((byte)ClientCommand.StringCmd, message) { }
		
		#region ICommand Members
		public ClientCommand Type { get { return ClientCommand.StringCmd; } }
		#endregion
	}
}
