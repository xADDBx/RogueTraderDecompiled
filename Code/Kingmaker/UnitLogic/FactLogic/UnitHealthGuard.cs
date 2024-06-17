using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("0dc054950e8a441f85a27a27e021d947")]
public class UnitHealthGuard : UnitFactComponentDelegate, IHashable
{
	[Tooltip("0 means 1 HP")]
	public int HealthPercent;

	protected override void OnActivateOrPostLoad()
	{
		int value = Math.Max(1, (int)(0.01 * (double)HealthPercent * (double)base.Owner.Health.MaxHitPoints));
		base.Owner.Features.Immortality.Retain();
		base.Owner.Health.AddHealthGuard(base.Fact, this, value);
		base.Owner.Health.HitPoints.AddDependentComponent(base.Runtime);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.Immortality.Release();
		base.Owner.Health.RemoveHealthGuard(base.Fact, this);
		base.Owner.Health.HitPoints.RemoveDependentComponent(base.Runtime);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
