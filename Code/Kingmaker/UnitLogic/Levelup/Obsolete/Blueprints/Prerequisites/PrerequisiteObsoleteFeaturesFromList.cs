using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[AllowMultipleComponents]
[TypeId("24d005ae28c58444bb8db4b94c9165cc")]
public class PrerequisiteObsoleteFeaturesFromList : Prerequisite_Obsolete
{
	[SerializeField]
	[FormerlySerializedAs("Features")]
	private BlueprintFeatureReference[] m_Features;

	public int Amount;

	public ReferenceArrayProxy<BlueprintFeature> Features
	{
		get
		{
			BlueprintReference<BlueprintFeature>[] features = m_Features;
			return features;
		}
	}

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		int num = 0;
		foreach (BlueprintFeature feature in Features)
		{
			if ((!(selectionState != null) || !selectionState.IsSelectedInChildren(feature)) && unit.Facts.Contains(feature))
			{
				num++;
				if (num >= Amount)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Amount <= 1)
		{
			stringBuilder.Append($"{UIStrings.Instance.Tooltips.OneFrom}:\n");
		}
		else
		{
			stringBuilder.Append(string.Format(UIStrings.Instance.Tooltips.FeaturesFrom, Amount));
			stringBuilder.Append(":\n");
		}
		for (int i = 0; i < Features.Length; i++)
		{
			stringBuilder.Append(Features[i].Name);
			if (i < Features.Length - 1)
			{
				stringBuilder.Append("\n");
			}
		}
		return stringBuilder.ToString();
	}
}
