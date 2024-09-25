using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Networking.Serialization;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Actions;

[HashRoot]
[Obsolete]
public class SelectFeature : ILevelUpAction, IHashable
{
	[NotNull]
	[JsonProperty]
	[GameStateIgnore("hmmvot: сделай это поле не сериализуемым, оно вообще значения никакого не имеет - эти объекты не инстанциируются")]
	public readonly IFeatureSelection Selection;

	[JsonProperty]
	public readonly int SelectionIndex;

	[NotNull]
	[JsonProperty]
	private readonly BlueprintFeature m_ItemFeature;

	[CanBeNull]
	[JsonProperty]
	private readonly FeatureParam m_ItemParam;

	public LevelUpActionPriority Priority => CalculatePriority(Selection);

	[CanBeNull]
	public IFeatureSelectionItem Item { get; private set; }

	[JsonConstructor]
	public SelectFeature()
	{
	}

	public SelectFeature([NotNull] FeatureSelectionState selection, [NotNull] IFeatureSelectionItem item)
	{
		Selection = selection.Selection;
		SelectionIndex = selection.Index;
		Item = item;
		m_ItemFeature = item.Feature;
		m_ItemParam = item.Param;
	}

	public bool Check(LevelUpState state, BaseUnitEntity unit)
	{
		return false;
	}

	public void Apply(LevelUpState state, BaseUnitEntity unit)
	{
	}

	public void PostLoad()
	{
	}

	public static LevelUpActionPriority CalculatePriority(IFeatureSelection selection)
	{
		return LevelUpActionPriority.Features;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		int val = SelectionIndex;
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_ItemFeature);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<FeatureParam>.GetHash128(m_ItemParam);
		result.Append(ref val3);
		return result;
	}
}
