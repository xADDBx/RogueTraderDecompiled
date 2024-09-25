namespace Kingmaker.Visual.CharacterSystem;

public static class CharacterExtensions
{
	public static CharacterStudio.Race DetermineRace(this Character character)
	{
		return CharacterStudio.DetermineRace(character);
	}

	public static CharacterStudio.Gender DetermineGender(this Character character)
	{
		return CharacterStudio.DetermineGender(character.gameObject);
	}
}
