using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("2f9cfff2340b8c344ab4fd92c2eb61f2")]
public class ContextActionCastSpell : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("Spell")]
	private BlueprintAbilityReference m_Spell;

	public bool OverrideDC;

	[ShowIf("OverrideDC")]
	public ContextValue DC;

	public bool OverrideSpellLevel;

	[ShowIf("OverrideSpellLevel")]
	public ContextValue SpellLevel;

	public bool CastByTarget;

	[Tooltip("Enables animation for casting and initiates full UseAbility command instead of simply triggering cast rule")]
	public bool UseFullAbilityCastCycle;

	public BlueprintAbility Spell => m_Spell?.Get();

	public override string GetCaption()
	{
		return $"Cast spell {Spell}" + (OverrideDC ? $" DC: {DC}" : "") + (OverrideSpellLevel ? $" SL: {SpellLevel}" : "");
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = (CastByTarget ? base.Target.Entity : base.Context.MaybeCaster);
		if (mechanicEntity == null)
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		AbilityData abilityData = mechanicEntity.Facts.Get<Ability>(Spell)?.Data ?? new AbilityData(Spell, mechanicEntity);
		int? overrideDC = abilityData.OverrideDC;
		int? overrideSpellLevel = abilityData.OverrideSpellLevel;
		try
		{
			if (OverrideDC)
			{
				abilityData.OverrideDC = DC.Calculate(base.Context);
			}
			if (OverrideSpellLevel)
			{
				abilityData.OverrideSpellLevel = SpellLevel.Calculate(base.Context);
			}
			if (UseFullAbilityCastCycle)
			{
				PartUnitCommands commandsOptional = base.Caster.GetCommandsOptional();
				if (commandsOptional != null)
				{
					UnitUseAbilityParams cmdParams = new UnitUseAbilityParams(abilityData, base.Target)
					{
						IgnoreCooldown = true,
						FreeAction = true
					};
					commandsOptional.AddToQueue(cmdParams);
					return;
				}
			}
			RulePerformAbility obj = new RulePerformAbility(abilityData, base.Target)
			{
				IgnoreCooldown = true,
				ForceFreeAction = true
			};
			Rulebook.Trigger(obj);
			obj.Context.RewindActionIndex();
		}
		finally
		{
			abilityData.OverrideDC = overrideDC;
			abilityData.OverrideSpellLevel = overrideSpellLevel;
		}
	}
}
