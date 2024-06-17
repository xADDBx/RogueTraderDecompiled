using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;

namespace Kingmaker.UnitLogic;

public readonly struct FeatureSource
{
	[JsonProperty]
	public readonly BlueprintScriptableObject Blueprint;

	public FeatureSource(BlueprintProgression progression)
	{
		Blueprint = progression;
	}

	public FeatureSource(BlueprintCharacterClass @class)
	{
		Blueprint = @class;
	}

	public FeatureSource(BlueprintRace race)
	{
		Blueprint = race;
	}

	public static implicit operator FeatureSource([NotNull] BlueprintProgression progression)
	{
		return new FeatureSource(progression);
	}

	public static implicit operator FeatureSource([NotNull] BlueprintCharacterClass @class)
	{
		return new FeatureSource(@class);
	}

	public static implicit operator FeatureSource([NotNull] BlueprintRace race)
	{
		return new FeatureSource(race);
	}

	[CanBeNull]
	public static BlueprintCharacterClass GetMythicSource(BlueprintScriptableObject blueprint)
	{
		if (blueprint is BlueprintCharacterClass blueprintCharacterClass)
		{
			if (!blueprintCharacterClass.IsMythic)
			{
				return null;
			}
			return blueprintCharacterClass;
		}
		if (blueprint is BlueprintProgression blueprintProgression)
		{
			return blueprintProgression.MythicSource;
		}
		return null;
	}
}
