using Core.Cheats;
using UnityEngine;

namespace Kingmaker.Cheats;

internal class CheatsFinalArenaPlague
{
	[Cheat(Name = "off_FinalArenaPlagueOff", Description = "Disable final arena plague")]
	public static void FinalArenaPlagueOff()
	{
		GameObject.Find("/Stage_2_1/Plague_Crowd").gameObject.SetActive(value: false);
	}
}
