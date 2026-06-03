using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[TypeId("bb865bfaa8723e84b94630feaa347406")]
public class EnsureHasForgeWorldAugment : PlayerUpgraderOnlyAction
{
	[SerializeField]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private BlueprintFeatureReference m_Feature;

	[SerializeField]
	private BlueprintFeatureReference m_CorrespondingAugment;

	public override string GetCaption()
	{
		return $"Ensure {m_Unit} has corresponding Forge World augment";
	}

	protected override void RunActionOverride()
	{
		AbstractUnitEntity value = m_Unit.GetValue();
		if (value.Facts.Contains((BlueprintFeature)m_Feature) && !value.Facts.Contains((BlueprintFeature)m_CorrespondingAugment))
		{
			value.AddFact((BlueprintFeature)m_CorrespondingAugment);
		}
	}
}
