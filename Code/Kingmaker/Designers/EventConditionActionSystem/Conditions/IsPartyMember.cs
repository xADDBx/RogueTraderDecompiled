using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("494b2e62fc3ee4b40956216331fc6de6")]
public class IsPartyMember : Condition
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	protected override string GetConditionCaption()
	{
		return $"{Unit} is in party";
	}

	protected override bool CheckCondition()
	{
		if (Unit.GetValue() is BaseUnitEntity item)
		{
			return Game.Instance.Player.PartyAndPets.Contains(item);
		}
		return false;
	}
}
