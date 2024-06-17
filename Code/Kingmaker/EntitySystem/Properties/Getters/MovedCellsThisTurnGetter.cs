using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("86b13f19aa70461bb1bda4305b261ffd")]
public class MovedCellsThisTurnGetter : UnitPropertyGetter
{
	protected override int GetBaseValue()
	{
		return (int)base.CurrentEntity.CombatState.MovedCellsThisTurn;
	}

	protected override string GetInnerCaption()
	{
		return "Moved cells this turn";
	}
}
