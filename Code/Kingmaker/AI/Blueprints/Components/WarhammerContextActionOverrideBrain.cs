using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace Kingmaker.AI.Blueprints.Components;

[TypeId("36ebff0e4bf0c1f439d5bccec9dd723b")]
public class WarhammerContextActionOverrideBrain : ContextAction
{
	[SerializeField]
	private BlueprintBrainBaseReference m_Brain;

	public BlueprintBrainBase Brain => m_Brain?.Get();

	public override string GetCaption()
	{
		return "Set brain to " + Brain.name;
	}

	protected override void RunAction()
	{
		base.TargetEntity?.GetOptional<PartUnitBrain>()?.SetBrain(Brain);
	}
}
