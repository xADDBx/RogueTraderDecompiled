using System.Text;
using JetBrains.Annotations;
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
[TypeId("335732aafd9506041bd15e96b782fd61")]
public class PrerequisiteObsoleteNoFeature : Prerequisite_Obsolete
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintFeatureReference m_Feature;

	public BlueprintFeature Feature => m_Feature?.Get();

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		return !unit.Facts.Contains(Feature);
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"{UIStrings.Instance.Tooltips.NoFeature}: {Feature.Name}");
		return stringBuilder.ToString();
	}
}
