using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.ActivatableAbilities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("10463761e631499a8b8146db3dcf8660")]
public class SwitchActivatableAbility : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[SerializeField]
	private BlueprintActivatableAbilityReference m_Ability;

	public bool IsOn;

	public BlueprintActivatableAbility Ability => m_Ability;

	public override string GetCaption()
	{
		if (!IsOn)
		{
			return $"Turn off activatable ability {Ability} for unit {Unit}";
		}
		return $"Turn on activatable ability {Ability} for unit {Unit}";
	}

	public override void RunAction()
	{
		Unit.GetValue().Facts.Get<ActivatableAbility>(Ability).IsOn = IsOn;
	}
}
