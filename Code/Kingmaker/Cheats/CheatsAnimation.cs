using Core.Cheats;

namespace Kingmaker.Cheats;

internal class CheatsAnimation
{
	[Cheat(Name = "am_forcespeed", Description = "Set to override movement speed for player characters, in feet per standard action. Set to 0 to revert to default speed")]
	public static float SpeedForce { get; set; }

	[Cheat(Name = "am_movetype", Description = "Set to override default move type for player characters.\n0 = charge\n1 = walk\n2 = run\n3 = crouch\n")]
	public static int MoveType { get; set; } = 2;


	[Cheat(Name = "am_speedlock", Description = "When true, all player characters move with the same speed out of combat.\nWhen false, everyone uses their own default speed.")]
	public static bool SpeedLock { get; set; } = true;

}
