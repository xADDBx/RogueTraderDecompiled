using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Selection;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.UI.Common;

public class UIFeature : FeatureUIData
{
	public int Level;

	public int Rank;

	public bool isBead;

	public FeatureGroup Type;

	public TalentIconInfo TalentIconsInfo;

	public BlueprintFeatureSelection_Obsolete Source;

	public UIFeature(BlueprintFeature feature, FeatureParam param = null, BlueprintFeatureSelection_Obsolete source = null)
		: base(feature, param)
	{
		TalentIconsInfo = feature.TalentIconInfo;
		if (source != feature)
		{
			Source = source;
		}
	}

	public UIFeature(Feature feature, FeatureParam param = null, BlueprintFeatureSelection_Obsolete source = null)
		: this(feature.Blueprint, param, source)
	{
		Rank = feature.Rank;
	}
}
