using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[AllowedOn(typeof(BlueprintFeatureSelection_Obsolete))]
[AllowMultipleComponents]
[TypeId("77f0d7f2ab009f44198dbe4256d3d80b")]
public class PrerequisiteObsoleteSelectionPossible : Prerequisite_Obsolete
{
	[SerializeField]
	[FormerlySerializedAs("ThisFeature")]
	private BlueprintFeatureSelectionReference m_ThisFeature;

	public BlueprintFeatureSelection_Obsolete ThisFeature => m_ThisFeature?.Get();

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		if (selectionState != null)
		{
			return ThisFeature.AllFeatures.Any((BlueprintFeature p) => !unit.Facts.Contains(p) && (ThisFeature.IgnorePrerequisites || p.MeetsPrerequisites(unit, fromProgression: false, selectionState, state)));
		}
		return false;
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		return UIStrings.Instance.Tooltips.NoItemsAvailableToSelect;
	}
}
