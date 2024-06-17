using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("dd15ee9b51e103e428b6ac965fd1a737")]
public class AbilityCustomStarshipMultipleCasts : AbilityCustomLogic
{
	[SerializeField]
	private BlueprintAbilityReference m_AttackAbility;

	[SerializeField]
	private int repeatsMin;

	[SerializeField]
	private int repeatsMax;

	[SerializeField]
	private float pauseAfterEachCast;

	[SerializeField]
	private float finalPause;

	public BlueprintAbility AttackAbility => m_AttackAbility.Get();

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (!(maybeCaster is BaseUnitEntity caster) || AttackAbility == null)
		{
			yield break;
		}
		BaseUnitEntity lastTarget = caster;
		AbilityData abilityData = caster.Facts.Get<Ability>(AttackAbility)?.Data ?? new AbilityData(AttackAbility, caster);
		int runCount = PFStatefulRandom.SpaceCombat.Range(repeatsMin, repeatsMax + 1);
		double startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
		int q = 0;
		while (q < runCount)
		{
			BaseUnitEntity[] array = Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity unit) => abilityData.CanTarget(unit)).ToArray();
			if (array.Length == 0)
			{
				break;
			}
			BaseUnitEntity baseUnitEntity = array[PFStatefulRandom.SpaceCombat.Range(0, array.Length)];
			Rulebook.Trigger(new RulePerformAbility(abilityData, baseUnitEntity)
			{
				IgnoreCooldown = true,
				ForceFreeAction = true
			});
			lastTarget = baseUnitEntity;
			while ((Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000.0 < (double)(pauseAfterEachCast * (float)(q + 1)))
			{
				yield return null;
			}
			int num = q + 1;
			q = num;
		}
		if (lastTarget != caster)
		{
			startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
			while ((Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000.0 < (double)finalPause)
			{
				yield return null;
			}
		}
		yield return new AbilityDeliveryTarget(lastTarget);
	}
}
