using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[TypeId("5436494344c2496e9f8e835d990897ab")]
public class PrerequisiteObsoleteMainCharacter : Prerequisite_Obsolete
{
	public bool Companion;

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		BaseUnitEntity baseUnitEntity = unit.CopyOf.Entity ?? unit;
		return Game.Instance.Player.MainCharacter.Entity == baseUnitEntity != Companion;
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		return "";
	}
}
