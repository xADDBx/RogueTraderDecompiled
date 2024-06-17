using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterStudio : MonoBehaviour
{
	public enum Gender
	{
		Male,
		Female,
		None,
		NotDetermined
	}

	public enum Race
	{
		Human,
		Spacemarine,
		Eldar,
		NotDetermined
	}

	public static Gender DetermineGender(GameObject characterGo)
	{
		if (!(Character.FindBone(characterGo.transform, "R_B") == null))
		{
			return Gender.Female;
		}
		return Gender.Male;
	}

	public static Race DetermineRace(Character character)
	{
		return Race.NotDetermined;
	}
}
