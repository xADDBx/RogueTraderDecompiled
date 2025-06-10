using Kingmaker.Blueprints;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;

namespace Kingmaker.Controllers;

public class PsychicPhenomenaController : IController, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		if (context.Ability.Blueprint.IsPsykerAbility && context.MaybeCaster?.GetPsykerOptional() != null)
		{
			RuleCalculatePsychicPhenomenaEffect ruleCalculatePsychicPhenomenaEffect = new RuleCalculatePsychicPhenomenaEffect(context.Caster, context);
			Rulebook.Trigger(ruleCalculatePsychicPhenomenaEffect);
			RunPsychicPhenomenaEffectOnTarget(ruleCalculatePsychicPhenomenaEffect.OverrideTarget?.Entity ?? context.Caster, context, ruleCalculatePsychicPhenomenaEffect.ResultPerilsEffect, ruleCalculatePsychicPhenomenaEffect.ResultPsychicPhenomena);
		}
	}

	public static void TriggerFakePsychicPhenomenaEffectOnTarget(MechanicEntity target, MechanicsContext context, BlueprintAbilityReference abilityReference, BlueprintPsychicPhenomenaRoot.PsychicPhenomenaData psychicPhenomenaData)
	{
		Rulebook.Trigger(new RuleFakeCalculatePsychicPhenomenaEffect(target, abilityReference, psychicPhenomenaData));
		RunPsychicPhenomenaEffectOnTarget(target, context, abilityReference, psychicPhenomenaData);
	}

	private static void RunPsychicPhenomenaEffectOnTarget(MechanicEntity target, MechanicsContext context, BlueprintAbilityReference abilityReference, BlueprintPsychicPhenomenaRoot.PsychicPhenomenaData psychicPhenomenaData)
	{
		if (target == null)
		{
			return;
		}
		if (abilityReference != null)
		{
			RulePerformAbility rulePerformAbility = new RulePerformAbility(new AbilityData(abilityReference, target), target);
			rulePerformAbility.IgnoreCooldown = true;
			rulePerformAbility.ForceFreeAction = true;
			rulePerformAbility.Context.ExecutionFromPsychicPhenomena = true;
			Rulebook.Trigger(rulePerformAbility);
			rulePerformAbility.Context.RewindActionIndex();
		}
		if (psychicPhenomenaData == null)
		{
			return;
		}
		if (psychicPhenomenaData.OptionalMinorFX != null)
		{
			FxHelper.SpawnFxOnEntity(psychicPhenomenaData.OptionalMinorFX, target.View);
		}
		if (psychicPhenomenaData.Bark != null)
		{
			psychicPhenomenaData.Bark.Chance = 1f;
			psychicPhenomenaData.Bark.ShowOnScreen = true;
			new BarkWrapper(psychicPhenomenaData.Bark, target.View.Asks).Schedule();
		}
		bool flag = false;
		if (psychicPhenomenaData.CheckConditionOnAllPartyMembers)
		{
			UnitGroupEnumerator enumerator = target.GetCombatGroupOptional().GetEnumerator();
			while (enumerator.MoveNext())
			{
				BaseUnitEntity current = enumerator.Current;
				using (context.GetDataScope(current.ToITargetWrapper()))
				{
					if (psychicPhenomenaData.ConditionsChecker.Check())
					{
						flag = true;
						break;
					}
				}
			}
		}
		else if (context != null)
		{
			using (context.GetDataScope(target.ToITargetWrapper()))
			{
				if (psychicPhenomenaData.ConditionsChecker.Check())
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			Rulebook.Trigger(RulePerformMomentumChange.CreatePsychicPhenomena(target, psychicPhenomenaData.MomentumPenalty));
		}
		else
		{
			Rulebook.Trigger(RulePerformMomentumChange.CreatePsychicPhenomena(target, psychicPhenomenaData.DefaultMomentumPenalty));
		}
	}
}
