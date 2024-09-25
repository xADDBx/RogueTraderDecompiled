using System;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITextGlobalMapPassages
{
	public LocalizedString Safe;

	public LocalizedString Unsafe;

	public LocalizedString Dangerous;

	public LocalizedString Deadly;

	public string GetDifficultyString(SectorMapPassageEntity.PassageDifficulty difficulty)
	{
		return difficulty switch
		{
			SectorMapPassageEntity.PassageDifficulty.Safe => Safe, 
			SectorMapPassageEntity.PassageDifficulty.Unsafe => Unsafe, 
			SectorMapPassageEntity.PassageDifficulty.Dangerous => Dangerous, 
			SectorMapPassageEntity.PassageDifficulty.Deadly => Deadly, 
			_ => string.Empty, 
		};
	}
}
