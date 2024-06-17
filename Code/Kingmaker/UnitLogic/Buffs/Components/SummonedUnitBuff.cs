using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.TurnBased;
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
		if (!base.Owner.Destroyed && !base.Owner.Facts.Contains(BlueprintRoot.Instance.SystemMechanics.DismemberedUnitBuff))
		{
			GameObject gameObject = base.Owner.Blueprint.VisualSettings.BloodPuddleFx.Load();
			DismemberUnitFX dismemberUnitFX = ((gameObject != null) ? gameObject.GetComponent<DismemberUnitFX>() : null);
			TimeSpan timeSpan = (dismemberUnitFX ? dismemberUnitFX.Delay.Seconds() : 5f.Seconds());
			if (TurnController.IsInTurnBasedCombat())
			{
				timeSpan = TimeSpan.Zero;
			}
			base.Owner.Buffs.Add(BlueprintRoot.Instance.SystemMechanics.DismemberedUnitBuff, base.Owner, timeSpan);
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
