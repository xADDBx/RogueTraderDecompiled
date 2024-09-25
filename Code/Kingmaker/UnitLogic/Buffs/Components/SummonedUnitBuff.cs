using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[ComponentName("Buffs/Special/Summoned unit")]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("38da9c69d457e7f4f88a857ad14d8cb0")]
public class SummonedUnitBuff : UnitBuffComponentDelegate, IHashable
{
	protected override void OnRemoved()
	{
		base.OnRemoved();
		if (!base.Owner.Destroyed)
		{
			GameObject gameObject = base.Owner.Blueprint.VisualSettings.BloodPuddleFx.Load();
			DismemberUnitFX dismemberUnitFX = ((gameObject != null) ? gameObject.GetComponent<DismemberUnitFX>() : null);
			if (!dismemberUnitFX)
			{
				5f.Seconds();
			}
			else
			{
				dismemberUnitFX.Delay.Seconds();
			}
			if ((bool)gameObject && (bool)dismemberUnitFX)
			{
				FxHelper.SpawnFxOnEntity(gameObject, base.Owner.View);
				return;
			}
			PFLog.Default.Error(base.Owner.Blueprint, "Missing DismemberUnitFX in OnDeathEffect: {0}", base.Owner);
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
