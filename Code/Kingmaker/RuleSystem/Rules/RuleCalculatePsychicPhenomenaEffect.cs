using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Enums;
using Kingmaker.Designers.WarhammerSurfaceCombatPrototype;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Random;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculatePsychicPhenomenaEffect : RulebookEvent
{
	public class PhenomenaAvoidResult
	{
		public readonly ValueModifiersManager ChanceModifiers = new ValueModifiersManager();

		public RuleRollD100 D100 { get; private set; }

		public int Chance => ChanceModifiers.Value;

		public bool IsAvoided
		{
			get
			{
				if (D100 != null)
				{
					return D100.Result < ChanceModifiers.Value;
				}
				return false;
			}
		}

		public bool Roll(MechanicEntity caster)
		{
			if (Chance <= 0)
			{
				return false;
			}
			D100 = new RuleRollD100(caster);
			Rulebook.Trigger(D100);
			return IsAvoided;
		}
	}

	public readonly PhenomenaAvoidResult PsychicPhenomenaAvoid = new PhenomenaAvoidResult();

	public readonly PhenomenaAvoidResult PerilsOfTheWarpAvoid = new PhenomenaAvoidResult();

	private static BlueprintPsychicPhenomenaRoot PsychicPhenomenaRoot => BlueprintRoot.Instance.WarhammerRoot.PsychicPhenomenaRoot;

	private AbilityExecutionContext AbilityContext { get; }

	public BlueprintAbilityReference ResultPerilsEffect { get; protected set; }

	public BlueprintPsychicPhenomenaRoot.PsychicPhenomenaData ResultPsychicPhenomena { get; protected set; }

	public bool IsPsychicPhenomena { get; protected set; }

	public bool IsPerilsOfTheWarp { get; protected set; }

	[CanBeNull]
	public (MechanicEntity Entity, MechanicEntityFact Reason)? OverrideTarget { get; set; }

	public RuleCalculatePsychicPhenomenaEffect([NotNull] IMechanicEntity initiator)
		: base(initiator)
	{
	}

	public RuleCalculatePsychicPhenomenaEffect([NotNull] MechanicEntity initiator, [NotNull] AbilityExecutionContext abilityContext)
		: this(initiator)
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
		MechanicEntity caster = AbilityContext.Caster;
		if (IsPsychicPhenomena && PsychicPhenomenaAvoid.Roll(caster))
		{
			IsPsychicPhenomena = false;
		}
		if (IsPerilsOfTheWarp && PerilsOfTheWarpAvoid.Roll(caster))
		{
			IsPerilsOfTheWarp = false;
		}
		if (IsPsychicPhenomena && PsychicPhenomenaRoot.PsychicPhenomena.Length != 0)
		{
			ResultPsychicPhenomena = PsychicPhenomenaRoot.PsychicPhenomena[PFStatefulRandom.UnitRandom.Range(0, PsychicPhenomenaRoot.PsychicPhenomena.Length)];
		}
		if (!IsPerilsOfTheWarp)
		{
			return;
		}
		bool num3 = Game.Instance.Player.UnlockableFlags.IsUnlocked(PsychicPhenomenaRoot.PerilRestrictingFlag);
		BlueprintAbilityReference[] array = PsychicPhenomenaRoot.PerilsOfTheWarpMinor;
		BlueprintAbilityReference[] array2 = PsychicPhenomenaRoot.PerilsOfTheWarpMajor;
		if (!num3)
		{
			array = array.Where((BlueprintAbilityReference peril) => !PsychicPhenomenaRoot.RestrictedPerilsOfTheWarpMinor.Contains(peril)).ToArray();
			array2 = array2.Where((BlueprintAbilityReference peril) => !PsychicPhenomenaRoot.RestrictedPerilsOfTheWarpMajor.Contains(peril)).ToArray();
		}
		if (PFStatefulRandom.UnitRandom.Range(0, 100) < 90)
		{
			if (array.Length != 0)
			{
				ResultPerilsEffect = array[PFStatefulRandom.UnitRandom.Range(0, array.Length)];
			}
		}
		else if (array2.Length != 0)
		{
			ResultPerilsEffect = array2[PFStatefulRandom.UnitRandom.Range(0, array2.Length)];
		}
	}
}
