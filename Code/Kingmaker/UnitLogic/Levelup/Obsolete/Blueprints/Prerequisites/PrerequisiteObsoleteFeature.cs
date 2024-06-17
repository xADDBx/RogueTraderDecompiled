using System;
using System.Text;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[Obsolete]
[AllowMultipleComponents]
[TypeId("17030ee91bfc7f24e93d3b6fa583dd09")]
public class PrerequisiteObsoleteFeature : Prerequisite_Obsolete
{
	[SerializeField]
	[ValidateNotNull]
	[FormerlySerializedAs("Feature")]
	private BlueprintFeatureReference m_Feature;

	public BlueprintFeature Feature => m_Feature?.Get();

	public bool IsRace => Feature is BlueprintRace;

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		if (IsRace && state != null && state.IsFirstCharacterLevel)
		{
			return true;
		}
		if (selectionState != null && selectionState.IsSelectedInChildren(Feature))
		{
			return false;
		}
		return unit.Progression.Features.Contains(Feature);
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Feature == null)
		{
			PFLog.Default.Error("Empty Feature field in prerequisite component: " + name);
		}
		else
		{
			if (string.IsNullOrEmpty(Feature.Name))
			{
				PFLog.Default.Error(Feature.name + " has no Display Name");
			}
			stringBuilder.Append(Feature.Name);
		}
		return stringBuilder.ToString();
	}
}
