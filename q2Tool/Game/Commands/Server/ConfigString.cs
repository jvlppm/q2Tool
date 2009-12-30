using System;
namespace q2Tool.Commands.Server
{
	public enum ConfigType : short
	{
		Name = 0,
		CdTrack = 1,
		Sky = 2,
		SkyAxis = 3,	// %f %f %f format
		SkyRotate = 4,
		StatusBar = 5,	// display program string

		AirAccel = 29,	// air acceleration control
		MaxClients = 30,
		MapChecksum = 31,	// for catching cheater maps

		Models = 32,
		Sounds = Models + Quake.MaxModels,
		Images = Sounds + Quake.MaxSounds,
		Lights = Images + Quake.MaxImages,
		Items = Lights + Quake.MaxLights,
		PlayerInfo = Items + Quake.MaxItems,
		General = PlayerInfo + Quake.MaxClients,
		Bad = General + Quake.MaxGeneral
	}

	public class ConfigString : IServerCommand, IStringPackage
	{
		ConfigType _configType;
		public ConfigType ConfigType
		{
			get
			{
				return _configType;
			}
			set
			{
				var configTypes = Enum.GetValues(typeof (ConfigType));
				for (int i = configTypes.Length - 1; i >= 0; i-- )
				{
					ConfigType current = (ConfigType) configTypes.GetValue(i);
					if (value >= current)
					{
						_configType = current;
						SubCode = value - current;
						break;
					}
				}
			}
		}
		public int SubCode { get; set; }
		public string Message { get; set; }

		//[short configType][string message]
		public ConfigString(RawPackage data)
		{
			ConfigType = (ConfigType)data.ReadShort();
			Message = data.ReadString();
			if(Message == string.Empty)
				ConfigType = ConfigType.Bad;
		}

		public ConfigString(ConfigType type, string message)
		{
			ConfigType = type;
			Message = message;
		}

		public ConfigString(ConfigType type, int subCode, string message)
		{
			ConfigType = type;
			SubCode = subCode;
			Message = message;
		}

		#region ICommand
		public int Size()
		{
			if (_configType == ConfigType.Bad)
				return 0;
			return Message.Length + 4;
		}

		public void WriteTo(RawPackage data)
		{
			if (_configType == ConfigType.Bad)
				return;

			data.WriteByte((byte)Type);
			data.WriteShort((short) ((short)ConfigType + SubCode));
			data.WriteString(Message);
		}

		public ServerCommand Type { get { return ServerCommand.ConfigString; } }
		#endregion
	}
}
