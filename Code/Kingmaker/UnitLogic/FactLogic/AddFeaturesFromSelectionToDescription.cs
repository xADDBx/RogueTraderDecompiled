using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("d349027bb3984883bf9d583b659c025a")]
public class AddFeaturesFromSelectionToDescription : DescriptionModifier
{
	public LocalizedString Introduction;

	[SerializeField]
	private BlueprintFeatureSelectionReference m_FeatureSelection;

	public bool OnlyIfRequiresThisFeature;

	public BlueprintFeatureSelection_Obsolete FeatureSelection => m_FeatureSelection?.Get();

	public override string Modify(string originalString)
	{
		string text = originalString + "\n" + Introduction;
		bool flag = true;
		foreach (BlueprintFeature allFeature in FeatureSelection.AllFeatures)
		{
			if (!OnlyIfRequiresThisFeature || allFeature.GetComponents<PrerequisiteObsoleteFeature>().Any((PrerequisiteObsoleteFeature p) => p.Feature == base.OwnerBlueprint))
			{
				text = text + (flag ? " " : ", ") + allFeature.Name;
				flag = false;
			}
		}
		return text;
	}
}
