namespace q2Tool.Commands.Client
{
	public class UserInfo : StringPackage, IClientStringPackage
	{
		//[string message]
		public UserInfo(RawPackage data) : base((byte)ClientCommand.UserInfo, data) { }
		public UserInfo(string message) : base((byte)ClientCommand.UserInfo, message) { }

		public string this[string parameter]
		{
			get
			{
				if (Message == null)
					return null;

				int pos = Message.IndexOf(@"\" + parameter + @"\");

				if (pos < 0)
					return null;

				string value = string.Empty;

				for (pos += parameter.Length + 2; pos < Message.Length && Message[pos] != '\\'; pos++)
					value += Message[pos];

				return value;
			}
		}

		#region ICommand Members
		public ClientCommand Type { get { return ClientCommand.UserInfo; } }
		#endregion
	}
}
