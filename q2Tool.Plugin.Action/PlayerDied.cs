using System;

namespace q2Tool
{
	public enum MeansOfDeath
	{
		Unknown,
		BreakingGlass,
		Suicide,
		Falling,
		Crush,
		Water,
		Slime,
		Lava,
		Explosion,
		Exit,
		Laser,
		Blaster,
		WrongPlace,
		HeldGrenade,
		HandGrenade,
		TaughtToFly,
		Pistol,
		Mp5,
		M4,
		M3,
		HandCannonSingle,
		HandCannonDouble,
		Sniper,
		DualPistols,
		KnifeSlash,
		KnifeThrown,
		Kick,
		Punch
	}

	public enum HitLocation
	{
		Unknown,
		Legs,
		Stomach,
		KevlarVest,
		Chest,
		Head
	}

	public class PlayerDiedEventArgs : PlayerEventArgs
	{
		public PlayerDiedEventArgs(Player player, Player killer, MeansOfDeath meansOfDeath, HitLocation hitLocation)
			: base(player)
		{
			Location = hitLocation;
			MeansOfDeath = meansOfDeath;
			Killer = killer;
		}

		public MeansOfDeath MeansOfDeath { get; private set; }
		public HitLocation Location { get; private set; }
		public Player Killer { get; private set; }
	}

	public delegate void PlayerDiedEventHandler(Action sender, PlayerDiedEventArgs e);
}
