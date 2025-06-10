using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("f96938c3f3494e7abe435945919d1170")]
public class ContextActionRemoveTemporaryWounds : ContextAction
{
	private enum ActionTargets
	{
		Target,
		Caster,
		All
	}

	[SerializeField]
	[Tooltip("The action target to remove temporary wounds")]
	private ActionTargets m_ActionTarget;

	public override string GetCaption()
	{
		return m_ActionTarget switch
		{
			ActionTargets.Target => "Remove all Temporary Wounds from Target", 
			ActionTargets.Caster => "Remove all Temporary Wounds from Caster", 
			_ => "Remove all Temporary Wounds from Target and Caster", 
		};
	}

	protected override void RunAction()
	{
		switch (m_ActionTarget)
		{
		case ActionTargets.Target:
			base.Target?.Entity?.GetOptional<PartHealth>()?.CleanupTemporaryHitPoints();
			break;
		case ActionTargets.Caster:
			base.Context.MaybeCaster?.GetOptional<PartHealth>()?.CleanupTemporaryHitPoints();
			break;
		default:
			base.Target?.Entity?.GetOptional<PartHealth>()?.CleanupTemporaryHitPoints();
			base.Context.MaybeCaster?.GetOptional<PartHealth>()?.CleanupTemporaryHitPoints();
			break;
		}
	}
}
