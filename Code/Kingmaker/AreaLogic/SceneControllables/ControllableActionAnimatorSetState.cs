using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[PlayerUpgraderAllowed(false)]
[TypeId("069b9553e9bf4011ad8e4a10e67e805c")]
public class ControllableActionAnimatorSetState : GameAction
{
	public int State;

	[Tooltip("Прямая ссылка на ControllableAnimator")]
	public ControllableAnimatorSetStateReference IdOfObject;

	[Tooltip("Unit, в children которого искать ControllableAnimator")]
	[SerializeReference]
	public AbstractUnitEvaluator TargetUnit;

	public override string GetCaption()
	{
		if (TargetUnit != null && TargetUnit.TryGetValue(out var value))
		{
			return $"Set {value.CharacterName} ControllableAnimator State to {State}";
		}
		return $"Set {IdOfObject?.EntityNameInEditor} Animator State to {State}";
	}

	protected override void RunAction()
	{
		PFLog.Default.Log($"[ControllableActionAnimatorSetState] RunAction called with State={State}");
		ControllableState state = new ControllableState
		{
			State = State
		};
		string controllableId = GetControllableId();
		if (!string.IsNullOrEmpty(controllableId))
		{
			PFLog.Default.Log($"[ControllableActionAnimatorSetState] Setting state {State} for controllable {controllableId}");
			Game.Instance.SceneControllables.SetState(controllableId, state);
		}
		else
		{
			PFLog.Default.Warning("[ControllableActionAnimatorSetState] No controllable ID found, action not executed");
		}
	}

	private string GetControllableId()
	{
		PFLog.Default.Log($"[ControllableActionAnimatorSetState] GetControllableId called. TargetUnit={TargetUnit}, IdOfObject={IdOfObject?.EntityNameInEditor}");
		if (TargetUnit != null && TargetUnit.TryGetValue(out var value))
		{
			PFLog.Default.Log("[ControllableActionAnimatorSetState] Found unit: " + value.CharacterName);
			AbstractUnitEntityView view = value.View;
			if (view != null)
			{
				PFLog.Default.Log("[ControllableActionAnimatorSetState] Unit has view, searching for ControllableAnimator in children");
				ControllableAnimator componentInChildren = view.ViewTransform.GetComponentInChildren<ControllableAnimator>();
				if (componentInChildren != null)
				{
					componentInChildren.Setup();
					PFLog.Default.Log("[ControllableActionAnimatorSetState] Found ControllableAnimator with ID: " + componentInChildren.UniqueId);
					return componentInChildren.UniqueId;
				}
				PFLog.TechArt.Warning("[ControllableActionAnimatorSetState] ControllableAnimator not found in children of unit " + value.CharacterName);
			}
			else
			{
				PFLog.TechArt.Warning("[ControllableActionAnimatorSetState] Unit " + value.CharacterName + " has no View");
			}
		}
		else
		{
			if (IdOfObject != null)
			{
				PFLog.Default.Log("[ControllableActionAnimatorSetState] Using direct reference: " + IdOfObject.UniqueId);
				return IdOfObject.UniqueId;
			}
			PFLog.TechArt.Warning("[ControllableActionAnimatorSetState] Neither TargetUnit nor IdOfObject is specified");
		}
		return null;
	}
}
