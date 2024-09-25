using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("7c34a1bbda3ce6c4680181ce3eb2581b")]
public class RemoveDeathDoor : GameAction
{
	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	public override string GetDescription()
	{
		return $"Снимает DeathDoor с указанного юнита {Unit}";
	}

	public override string GetCaption()
	{
		return $"Remove DeathDoor condition from {Unit}";
	}

	protected override void RunAction()
	{
		Unit.GetValue().Remove<UnitPartDeathDoor>();
	}
}
