using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment;

public class DismembermentBatchSliceConfig : ScriptableObject
{
	public string[] IncludePaths = new string[1] { "Assets/Mechanics/Bundles/Prefabs/Characters/BakedCharacters" };

	public string[] ExcludePaths = new string[1] { "Assets/Mechanics/Bundles/Prefabs/Characters/BakedCharacters/!Obsolete" };

	public string[] IncludeOnlyCharacters;

	public string[] SkipCharacters;

	public bool RebakeOnlyBroken;

	public string[] BrokenCharacters;

	public DismembermentTemplate DismembermentTemplateMale;

	public DismembermentTemplate DismembermentTemplateFemale;
}
