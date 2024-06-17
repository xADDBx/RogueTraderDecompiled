using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Features;

[TypeId("7f9d25856f2935b43ab312cdc2db3a24")]
public abstract class BlueprintFeatureBase : BlueprintUnitFact
{
	[Tooltip("It will not be showed in any UI screens")]
	public bool HideInUI;

	[Tooltip("It will not be showed on page Total in LevelUp/Charscreen and Character Sheet > Abilities")]
	public bool HideInCharacterSheetAndLevelUp;

	[Tooltip("For BlueprintFeature: NotAvailible will not be showed in LevelUp/Charscreen selecors. For BlueprintFeatureSelection: all NotAvailible child features will not be showed.")]
	public bool HideNotAvailibleInUI;
}
