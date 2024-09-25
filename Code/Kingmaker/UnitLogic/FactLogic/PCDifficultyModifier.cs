using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Settings;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("d354ef733c658134a8b5bb61d3dd8201")]
public class PCDifficultyModifier : UnitDifficultyModifiersManager, IHashable
{
	protected override void UpdateModifiers()
	{
		RemoveModifiers();
		if (base.Owner.IsPlayerEnemy)
		{
			StarshipEntity obj = base.Owner as StarshipEntity;
			if (obj == null || !obj.IsSoftUnit)
			{
				float num = SettingsHelper.CalculateCRModifier(SettingsRoot.Difficulty.EnemyHitPointsPercentModifier);
				AddPercentModifier(StatType.HitPoints, (int)((float)(int)SettingsRoot.Difficulty.EnemyHitPointsPercentModifier * num));
			}
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
