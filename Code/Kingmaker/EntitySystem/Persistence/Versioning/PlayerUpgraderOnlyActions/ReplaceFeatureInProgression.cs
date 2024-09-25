using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[Serializable]
[TypeId("5d4dce41cd4a491da09cb418b1bcd060")]
public class ReplaceFeatureInProgression : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private BlueprintFeatureReference m_Remove;

	[SerializeField]
	private BlueprintFeatureReference m_Add;

	public BlueprintFeature Remove => m_Remove;

	public BlueprintFeature Add => m_Add;

	public override string GetCaption()
	{
		return $"Replace {Remove} with {Add} in progression of {m_Unit}";
	}

	protected override void RunActionOverride()
	{
		m_Unit.GetValue().GetProgressionOptional()?.ReplaceFeature(Remove, Add);
	}
}
