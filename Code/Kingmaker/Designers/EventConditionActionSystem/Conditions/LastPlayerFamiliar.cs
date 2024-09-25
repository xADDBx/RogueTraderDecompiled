using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("01abbd4b6c6b496aaf4bae4cded5194c")]
public class LastPlayerFamiliar : Condition
{
	[SerializeField]
	private BlueprintUnit.Reference m_Blueprint;

	public BlueprintUnit Unit => m_Blueprint?.Get();

	protected override string GetConditionCaption()
	{
		if (Unit != null)
		{
			return $"Player last familiar {Unit}";
		}
		return "Player did not equip any familiar";
	}

	protected override bool CheckCondition()
	{
		UnitPartFamiliarLeader optional = Game.Instance.Player.MainCharacterEntity.GetOptional<UnitPartFamiliarLeader>();
		if (optional == null)
		{
			return Unit == null;
		}
		if (optional.LastEquippedFamiliar != null)
		{
			return optional.LastEquippedFamiliar == Unit;
		}
		return false;
	}
}
