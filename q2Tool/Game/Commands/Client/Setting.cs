using Jv.Networking;
namespace q2Tool.Commands.Client
{
	public enum SettingType : short
	{
		NoGun,
		NoBlend,
		Recording,
		PlayerUpdateRequests,
		Fps
	}

	public class Setting : IClientCommand
	{
		public SettingType SubType { get; set; }
		public short Value { get; set; }

		//[short setting, short value]
        public Setting(RawData data)
		{
			SubType = (SettingType)data.ReadShort();
			Value = data.ReadShort();
		}
		public Setting(SettingType subType, short value)
		{
			SubType = subType;
			Value = value;
		}
		
		#region ICommand Members
		public ClientCommand Type { get { return ClientCommand.Setting; } }
		#endregion

		public int Size()
		{
			return 5; // type + 2 bytes
		}

        public void WriteTo(RawData package)
		{
			package.WriteByte((byte)Type);
			package.WriteShort((short)SubType);
			package.WriteShort(Value);
		}
	}
}
