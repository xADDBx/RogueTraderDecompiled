using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.UnitUpgraderOnlyActions;

[Serializable]
[TypeId("d79439d3ef3e486088c6d12fe02974da")]
public class RemoveFactFromUnit : UnitUpgraderOnlyAction
{
	[SerializeField]
	private BlueprintUnitFactReference m_Fact;

	public BlueprintUnitFact Fact => m_Fact;

	public override string GetCaption()
	{
		return $"Remove fact {Fact}";
	}

	protected override void RunActionOverride()
	{
		base.Target.Facts.Remove(Fact);
	}
}
