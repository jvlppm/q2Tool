namespace q2Tool.Commands.Server
{
	public class PlayerInfo : IServerCommand, IStringPackage
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Model { get; set; }
		public string Skin { get; set; }

		public PlayerInfo(ConfigString configString)
		{
			string[] words = configString.Message.Split('\\');
			string[] look = words[1].Split('/');
			Name = words[0];
			Model = look[0];
			Skin = look[1];
			Id = configString.SubCode;
		}

		public PlayerInfo(int playerId, string name, string model, string skin)
		{
			Id = playerId;
			Name = name;
			Model = model;
			Skin = skin;
		}

		#region ICommand
		public int Size()
		{
			if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Model) || string.IsNullOrEmpty(Skin))
				return 0;

			return Name.Length + Model.Length + Skin.Length + 6;
		}

		public void WriteTo(RawPackage data)
		{
			if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Model) || string.IsNullOrEmpty(Skin))
				return;

			data.WriteByte((byte)Type);
			data.WriteShort((short)((short)ConfigStringType.PlayerInfo + Id));
			data.WriteString(Message);
		}

		public string Message
		{
			get { return string.Format("{0}\\{1}/{2}", Name, Model, Skin); }
		}

		public ServerCommand Type { get { return ServerCommand.ConfigString; } }

		#endregion
	}
}
