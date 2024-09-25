using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[TypeId("ec87819d5ee193c489e1ceda52cfc9c1")]
public class PcFemale : Condition
{
	protected override string GetConditionCaption()
	{
		return "PC Female";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.MainCharacter.Entity.Gender == Gender.Female;
	}
}
