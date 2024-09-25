using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("95ab8455743b4254580bb0adfaf3be54")]
public class IsInCombat : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public bool Player = true;

	protected override string GetConditionCaption()
	{
		if (Player)
		{
			return "Player is in combat";
		}
		return $"{Unit} in combat";
	}

	protected override bool CheckCondition()
	{
		if (Player)
		{
			return Game.Instance.Player.IsInCombat;
		}
		return Unit.GetValue().IsInCombat;
	}
}
