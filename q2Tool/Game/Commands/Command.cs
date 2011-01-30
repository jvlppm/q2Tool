namespace q2Tool.Commands
{
	public enum ClientCommand : byte
	{
		Bad = 0x00,
		Nop = 0x01,

		Move = 0x02,				// [[usercmd_t]
		UserInfo = 0x03,			// [string userinfo]
		StringCmd = 0x04,			// [string message]
		Setting = 0x05,				// [short setting, short value] R1Q2 settings support.
		MultiMoves = 0x06
	}

	public enum ServerCommand : byte
	{
		Bad = 0x00,

		// these ops are known to the game dll
		MuzzleFlash = 0x01,
		MuzzleFlash2 = 0x02,
		TempEntity = 0x03,
		Layout = 0x04,
		Inventory = 0x05,

		// the rest are private to the client and server
		Nop = 0x06,
		Disconnect = 0x07,
		Reconnect = 0x08,
		Sound = 0x09,				// --<see code>
		Print = 0x0A,				// [byte printLevel, string message]
		StuffText = 0x0B,			// [string message] (stuffed into client's console buffer, should be \n terminated)
		ServerData = 0x0C,			// [int protocol][int serverCount][byte attractLoop][string gameDir][string levelName] ...
		ConfigString = 0x0D,		// [short configType][string message]
		SpawnBaseLine = 0x0E,
		CenterPrint = 0x0F,			// [string message]
		Download = 0x10,			// --[short] size [size bytes]
		PlayerInfo = 0x11,			// --variable
		PacketEntities = 0x12,		// --[...]
		DeltaPacketEntities = 0x13,	// --[...]
		Frame = 0x14,

		// ********** r1q2 specific ***********
		ZPacket = 0x15,
		ZDownload = 0x16,
		PlayerUpdate = 0x17,
		Setting = 0x18,
		// ********** end r1q2 specific *******
	}
}