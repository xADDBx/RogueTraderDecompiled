using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("afb82a8851b254243b2a3144d74b0ccf")]
public class NPCDifficultyModifiersManager : UnitDifficultyModifiersManager, IHashable
{
	[SerializeField]
	private BlueprintFeatureReference m_TroopFeature;

	[SerializeField]
	private BlueprintFeatureReference m_EliteFeature;

	[SerializeField]
	private BlueprintFeatureReference m_BossFeature;

	public BlueprintFeature TroopFeature => m_TroopFeature?.Get();

	public BlueprintFeature EliteFeature => m_EliteFeature?.Get();

	public BlueprintFeature BossFeature => m_BossFeature?.Get();

	protected override void UpdateModifiers()
	{
		RemoveModifiers();
		if (base.Owner.Blueprint.Army != null && !base.Owner.Facts.Contains(TroopFeature) && (base.Owner.Blueprint.DifficultyType == UnitDifficultyType.Swarm || base.Owner.Blueprint.DifficultyType == UnitDifficultyType.Common))
		{
			base.Owner.AddFact(TroopFeature);
		}
		if (base.Owner.Blueprint.Army != null && !base.Owner.Facts.Contains(EliteFeature) && (base.Owner.Blueprint.DifficultyType == UnitDifficultyType.Hard || base.Owner.Blueprint.DifficultyType == UnitDifficultyType.Elite))
		{
			base.Owner.AddFact(EliteFeature);
		}
		if (base.Owner.Blueprint.Army != null && !base.Owner.Facts.Contains(BossFeature) && (base.Owner.Blueprint.DifficultyType == UnitDifficultyType.MiniBoss || base.Owner.Blueprint.DifficultyType == UnitDifficultyType.Boss || base.Owner.Blueprint.DifficultyType == UnitDifficultyType.ChapterBoss))
		{
			base.Owner.AddFact(BossFeature);
		}
		if (base.Owner.IsPlayerEnemy)
		{
			AddPercentModifier(StatType.HitPoints, SettingsRoot.Difficulty.EnemyHitPointsPercentModifier);
		}
		if (base.Owner.IsPlayerFaction)
		{
			AddModifier(StatType.Resolve, SettingsRoot.Difficulty.AllyResolveModifier);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
