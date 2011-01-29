using Jv.Networking;

namespace q2Tool.Commands.Server
{
	public class PlayerInfo : ConfigString
	{
		public PlayerInfo(int playerId, RawData data) : base(ConfigStringType.PlayerInfo, playerId, data) { }

		public PlayerInfo(int playerId, string name, string model, string skin)
			: base(ConfigStringType.PlayerInfo, playerId, string.Format("{0}\\{1}/{2}", name, model, skin)) { }

		public int Id
		{
			get { return SubCode; }
			set { SubCode = value; }
		}

		public string Name
		{
			get { return Message.Split('\\')[0]; }
			set { Message = value + "\\" + Model + "/" + Skin; }
		}

		public string Model
		{
			get { return Message.Split('\\')[1].Split('/')[0]; }
			set { Message = Name + "\\" + value + "/" + Skin; }
		}

		public string Skin
		{
			get { return Message.Split('/')[1]; }
			set { Message = Name + "\\" + Model + "/" + value; }
		}
	}
}
