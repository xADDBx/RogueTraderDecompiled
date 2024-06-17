using System.Text;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Prerequisites;

[AllowMultipleComponents]
[TypeId("9e1ba60ad05361f4d9c4f98c0dfd501e")]
public class PrerequisiteObsoletePlayerHasFeature : Prerequisite_Obsolete
{
	[NotNull]
	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintFeatureReference m_Feature;

	public BlueprintFeature Feature => m_Feature?.Get();

	public override bool Check(FeatureSelectionState selectionState, BaseUnitEntity unit, LevelUpState state)
	{
		if (selectionState != null && selectionState.IsSelectedInChildren(Feature))
		{
			return false;
		}
		return GameHelper.GetPlayerCharacter().Progression.Features.Contains(Feature);
	}

	public override string GetUIText(BaseUnitEntity unit)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Feature == null)
		{
			PFLog.Default.Error("Empty Feature fild in prerequisite component: " + name);
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
