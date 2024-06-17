using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public static class SoundUtility
{
	private const string SexGroupName = "CharacterSex";

	private const string TitleGroupName = "CharacterTitle";

	private const string RaceGroupName = "CharacterRace";

	public static void SetGenderFlags(GameObject go)
	{
		BlueprintUnlockableFlag kingFlag = BlueprintRoot.Instance.KingFlag;
		if (kingFlag != null)
		{
			if (!Game.Instance.Player.UnlockableFlags.IsUnlocked(kingFlag))
			{
				AkSoundEngine.SetSwitch("CharacterTitle", "Baron", go);
			}
			else
			{
				AkSoundEngine.SetSwitch("CharacterTitle", "King", go);
			}
		}
		switch (Game.Instance.Player.MainCharacter.Entity.Gender)
		{
		case Gender.Male:
			AkSoundEngine.SetSwitch("CharacterSex", "Male", go);
			break;
		case Gender.Female:
			AkSoundEngine.SetSwitch("CharacterSex", "Female", go);
			break;
		}
	}

	public static void SetRaceFlags(GameObject go)
	{
		string text = Game.Instance.Player.MainCharacterEntity.Progression.Race?.SoundKey;
		if (text != null)
		{
			AkSoundEngine.SetSwitch("CharacterRace", text, go);
		}
	}
}
