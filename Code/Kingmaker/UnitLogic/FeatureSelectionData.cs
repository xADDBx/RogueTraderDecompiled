using JetBrains.Annotations;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public readonly struct FeatureSelectionData : IHashable
{
	[JsonProperty]
	[NotNull]
	public readonly BlueprintPath Path;

	[JsonProperty]
	public readonly int Level;

	[JsonProperty]
	[NotNull]
	public readonly BlueprintSelectionFeature Selection;

	[JsonProperty]
	[NotNull]
	public readonly BlueprintFeature Feature;

	[JsonProperty]
	public readonly int Rank;

	public FeatureSelectionData(BlueprintPath path, int level, BlueprintSelectionFeature selection, BlueprintFeature feature, int rank)
	{
		Path = path;
		Level = level;
		Selection = selection;
		Feature = feature;
		Rank = rank;
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Path);
		result.Append(ref val);
		int val2 = Level;
		result.Append(ref val2);
		Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Selection);
		result.Append(ref val3);
		Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Feature);
		result.Append(ref val4);
		int val5 = Rank;
		result.Append(ref val5);
		return result;
	}
}
