using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("294fa514f9b7760448adbaa37eb1dbfa")]
public class PcRace : Condition
{
	public Race Race;

	protected override string GetConditionCaption()
	{
		return $"PC Race ({Race})";
	}

	protected override bool CheckCondition()
	{
		return Race == Game.Instance.Player.MainCharacterEntity?.ToBaseUnitEntity().Progression.Race?.RaceId;
	}
}
