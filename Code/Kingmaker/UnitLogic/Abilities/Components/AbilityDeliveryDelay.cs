using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Serializable]
[TypeId("87a59fcc03db47398e0e17cf0a2abde2")]
public class AbilityDeliveryDelay : AbilityDeliverEffect
{
	public float DelaySeconds = 1f;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		TimeSpan startTime = Game.Instance.TimeController.GameTime;
		while (Game.Instance.TimeController.GameTime - startTime < DelaySeconds.Seconds())
		{
			yield return null;
		}
		yield return new AbilityDeliveryTarget(target);
	}
}
