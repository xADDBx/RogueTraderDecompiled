using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Localization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[TypeId("eb2a8fed522277a499d2374478211954")]
public class PrerequisiteObsoleteEtude : Prerequisite_Obsolete
{
	public BlueprintEtudeReference Etude;

	public LocalizedString UIText;

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		return Game.Instance.Player.EtudesSystem.Etudes.Get((BlueprintEtude)Etude)?.IsPlaying ?? false;
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		return UIText;
	}
}
