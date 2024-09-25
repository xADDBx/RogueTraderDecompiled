using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[CreateAssetMenu(fileName = "CharacterStudioRebakeAllConfig", menuName = "ScriptableObjects/CharacterStudioRebakeAllConfig")]
public class CharacterStudioRebakeAllConfig : ScriptableObject
{
	public string[] IncludePaths = new string[1] { "Assets/Mechanics/Bundles/Prefabs/Characters/BakedCharacters" };

	public string[] ExcludePaths = new string[1] { "Assets/Mechanics/Bundles/Prefabs/Characters/BakedCharacters/!Obsolete" };

	public string[] IncludeOnlyCharacters;

	public string[] SkipCharacters;
}
