using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.AreaLogic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateVeilCount : RulebookEvent
{
	public readonly ValueModifiersManager StartVeilModifiers = new ValueModifiersManager();

	public AbilityData Ability { get; }

	public bool IsTurnBaseSwitched { get; }

	public bool IsNewRoundStart { get; }

	public int AdditionValue { get; }

	public int Result { get; private set; }

	private RuleCalculateVeilCount([NotNull] MechanicEntity initiator)
		: base(initiator)
	{
		StartVeilModifiers.Add(Game.Instance.LoadedAreaState.Blueprint.StartVailValueForLocation, this, ModifierDescriptor.UntypedUnstackable);
	}

	public RuleCalculateVeilCount([NotNull] MechanicEntity initiator, [NotNull] AbilityData ability)
		: this(initiator)
	{
		Ability = ability;
	}

	public RuleCalculateVeilCount([NotNull] MechanicEntity initiator, bool isTurnBaseSwitched, bool isNewRoundStart)
		: this(initiator)
	{
		IsTurnBaseSwitched = isTurnBaseSwitched;
		IsNewRoundStart = isNewRoundStart;
	}

	public RuleCalculateVeilCount([NotNull] MechanicEntity initiator, int additionValue)
		: this(initiator)
	{
		AdditionValue = additionValue;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		Result = Game.Instance.TurnController.VeilThicknessCounter.Value;
		if (IsTurnBaseSwitched && !IsNewRoundStart)
		{
			AreaVailValuePart areaVailPart = Game.Instance.LoadedAreaState.AreaVailPart;
			int minimalVailForCurrentArea = (Result = StartVeilModifiers.Value);
			areaVailPart.MinimalVailForCurrentArea = minimalVailForCurrentArea;
		}
		if (IsNewRoundStart && !IsTurnBaseSwitched && Result > Game.Instance.LoadedAreaState.AreaVailPart.MinimalVailForCurrentArea)
		{
			Result--;
		}
		if (Ability != null && !IsTurnBaseSwitched && !IsNewRoundStart)
		{
			Result += Ability.GetVeilThicknessPointsToAdd();
		}
		if (AdditionValue != 0)
		{
			Result += AdditionValue;
		}
		Result = Math.Clamp(Result, Game.Instance.LoadedAreaState.AreaVailPart.MinimalVailForCurrentArea, BlueprintRoot.Instance.WarhammerRoot.PsychicPhenomenaRoot.MaximumVeilOnAllLocation);
	}
}
