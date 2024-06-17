using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View.Covers;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateCoverHitChance : RulebookOptionalTargetEvent
{
	public readonly PercentsModifiersManager ChancePercentModifiers = new PercentsModifiersManager();

	public readonly ValueModifiersManager ChanceValueModifiers = new ValueModifiersManager();

	[CanBeNull]
	public readonly AbilityData Ability;

	public readonly LosCalculations.CoverType Los;

	[CanBeNull]
	public readonly MechanicEntity Cover;

	public readonly int BaseChance;

	public int ResultChance { get; private set; }

	public RuleCalculateCoverHitChance([NotNull] MechanicEntity initiator, [CanBeNull] MechanicEntity target, [CanBeNull] AbilityData ability, LosCalculations.CoverType los, [CanBeNull] MechanicEntity cover)
		: base(initiator, target)
	{
		Ability = ability;
		Los = los;
		Cover = cover;
		BlueprintWarhammerRoot instance = BlueprintWarhammerRoot.Instance;
		BaseChance = Los switch
		{
			LosCalculations.CoverType.None => 0, 
			LosCalculations.CoverType.Half => instance.CombatRoot.HitHalfCoverChance, 
			LosCalculations.CoverType.Full => instance.CombatRoot.HitFullCoverChance, 
			LosCalculations.CoverType.Invisible => 100, 
			_ => throw new ArgumentOutOfRangeException("Los"), 
		};
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		LosCalculations.CoverType los = Los;
		if (los == LosCalculations.CoverType.Invisible || los == LosCalculations.CoverType.None)
		{
			ResultChance = BaseChance;
			return;
		}
		float num = SettingsHelper.CalculateCRModifier(SettingsRoot.Difficulty.CoverHitBonusHalfModifier);
		float num2 = SettingsHelper.CalculateCRModifier(SettingsRoot.Difficulty.CoverHitBonusFullModifier);
		if (base.Initiator.IsPlayerFaction)
		{
			int value = Los switch
			{
				LosCalculations.CoverType.Half => (int)((float)(int)SettingsRoot.Difficulty.CoverHitBonusHalfModifier * num), 
				LosCalculations.CoverType.Full => (int)((float)(int)SettingsRoot.Difficulty.CoverHitBonusFullModifier * num2), 
				_ => 0, 
			};
			ChanceValueModifiers.Add(value, this, ModifierDescriptor.Difficulty);
		}
		if ((bool)base.InitiatorUnit?.Features.IgnoreCoverEfficiency)
		{
			ChanceValueModifiers.RemoveAll((Modifier p) => p.Value > 0);
			ChancePercentModifiers.RemoveAll((Modifier p) => p.Value > 0);
		}
		int value2 = (int)((float)(BaseChance + ChanceValueModifiers.Value) * ChancePercentModifiers.Value);
		ResultChance = Math.Clamp(value2, 0, 95);
	}
}
