using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Enums;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Random;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculatePsychicPhenomenaEffect : RulebookEvent
{
	private static BlueprintPsychicPhenomenaRoot PsychicPhenomenaRoot => BlueprintRoot.Instance.WarhammerRoot.PsychicPhenomenaRoot;

	private AbilityExecutionContext AbilityContext { get; }

	public BlueprintAbilityReference ResultPerilsEffect { get; protected set; }

	public BlueprintPsychicPhenomenaRoot.PsychicPhenomenaData ResultPsychicPhenomena { get; protected set; }

	public bool IsPsychicPhenomena { get; protected set; }

	public bool IsPerilsOfTheWarp { get; protected set; }

	public RuleCalculatePsychicPhenomenaEffect([NotNull] IMechanicEntity initiator)
		: base(initiator)
	{
	}

	public RuleCalculatePsychicPhenomenaEffect([NotNull] MechanicEntity initiator, [NotNull] AbilityExecutionContext abilityContext)
		: base(initiator)
	{
		AbilityContext = abilityContext;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		int value = Game.Instance.TurnController.VeilThicknessCounter.Value;
		bool flag = AbilityContext.AbilityBlueprint.PsychicPower == PsychicPower.Major;
		bool flag2 = AbilityContext.Caster.Facts.Contains(PsychicPhenomenaRoot.SanctionedPsyker);
		bool flag3 = AbilityContext.Caster.Facts.Contains(PsychicPhenomenaRoot.UnsanctionedPsyker);
		if (!flag2 && !flag3)
		{
			return;
		}
		int result = Rulebook.Trigger(new RuleRollD100(AbilityContext.Caster)).Result;
		float num = PsychicPhenomenaRoot.BasePsychicPhenomenaChanceAddition + (float)AbilityContext.Caster.GetPsykerOptional().AdditionChanceOnPsychicPhenomena + PsychicPhenomenaRoot.BasePsychicPhenomenaChanceMultiplier * (float)(int)AbilityContext.Caster.GetPsykerOptional().PsyRating;
		if (value < PsychicPhenomenaRoot.CriticalVeilOnAllLocation)
		{
			if (flag)
			{
				if (flag2 && (float)result < num * 2f)
				{
					IsPsychicPhenomena = true;
				}
				if (flag3 && (float)result < num * 4f)
				{
					IsPsychicPhenomena = true;
				}
			}
			else
			{
				if (flag2 && (float)result < num / 2f)
				{
					IsPsychicPhenomena = true;
				}
				if (flag3 && (float)result < num)
				{
					IsPsychicPhenomena = true;
				}
			}
		}
		else if (flag)
		{
			float num2 = (flag2 ? ((float)(10 + AbilityContext.Caster.GetPsykerOptional().AdditionChanceOnPerilsOfWarp + (int)AbilityContext.Caster.GetPsykerOptional().PsyRating + value)) : (flag3 ? ((float)(20 + AbilityContext.Caster.GetPsykerOptional().AdditionChanceOnPerilsOfWarp + (int)AbilityContext.Caster.GetPsykerOptional().PsyRating + value)) : 0f));
			if ((float)result < num2)
			{
				IsPerilsOfTheWarp = true;
			}
			else
			{
				IsPsychicPhenomena = true;
			}
		}
		else
		{
			if (flag2 && (float)result < num * 2f)
			{
				IsPsychicPhenomena = true;
			}
			if (flag3 && (float)result < num * 4f)
			{
				IsPsychicPhenomena = true;
			}
		}
		if (IsPsychicPhenomena && PsychicPhenomenaRoot.PsychicPhenomena.Length != 0)
		{
			ResultPsychicPhenomena = PsychicPhenomenaRoot.PsychicPhenomena[PFStatefulRandom.UnitRandom.Range(0, PsychicPhenomenaRoot.PsychicPhenomena.Length)];
		}
		if (!IsPerilsOfTheWarp)
		{
			return;
		}
		if (PFStatefulRandom.UnitRandom.Range(0, 100) < 90)
		{
			if (PsychicPhenomenaRoot.PerilsOfTheWarpMinor.Length != 0)
			{
				ResultPerilsEffect = PsychicPhenomenaRoot.PerilsOfTheWarpMinor[PFStatefulRandom.UnitRandom.Range(0, PsychicPhenomenaRoot.PerilsOfTheWarpMinor.Length)];
			}
		}
		else if (PsychicPhenomenaRoot.PerilsOfTheWarpMajor.Length != 0)
		{
			ResultPerilsEffect = PsychicPhenomenaRoot.PerilsOfTheWarpMajor[PFStatefulRandom.UnitRandom.Range(0, PsychicPhenomenaRoot.PerilsOfTheWarpMajor.Length)];
		}
	}
}
