using System;
namespace q2Tool.Commands.Server
{
	public enum ConfigStringType : short
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

	public class ConfigString : IServerCommand, IServerStringPackage
	{
		ConfigStringType _configType;
		public ConfigStringType ConfigType
		{
			get
			{
				return _configType;
			}
			set
			{
				var configTypes = Enum.GetValues(typeof (ConfigStringType));
				for (int i = configTypes.Length - 1; i >= 0; i-- )
				{
					ConfigStringType current = (ConfigStringType) configTypes.GetValue(i);
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
		public ConfigString(ConfigStringType configType, int subCode, RawPackage data)
		{
			ConfigType = configType;
			SubCode = subCode;
			Message = data.ReadString();

			if(Message == string.Empty)
				ConfigType = ConfigStringType.Bad;
		}

		public ConfigString(ConfigStringType type, string message)
		{
			ConfigType = type;
			Message = message;
		}

		public ConfigString(ConfigStringType type, int subCode, string message)
		{
			ConfigType = type;
			SubCode = subCode;
			Message = message;
		}

		#region ICommand
		public int Size()
		{
			if (_configType == ConfigStringType.Bad)
				return 0;
			return Message.Length + 4;
		}

		public void WriteTo(RawPackage data)
		{
			if (_configType == ConfigStringType.Bad)
				return;

			data.WriteByte((byte)Type);
			data.WriteShort((short) ((short)ConfigType + SubCode));
			data.WriteString(Message);
		}

		public ServerCommand Type { get { return ServerCommand.ConfigString; } }
		#endregion
	}
}
