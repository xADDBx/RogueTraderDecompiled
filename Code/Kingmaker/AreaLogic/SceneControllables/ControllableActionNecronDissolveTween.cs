using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Visual.FX;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[PlayerUpgraderAllowed(false)]
[TypeId("5246945772ff3b5448f2a7c52827d196")]
public class ControllableActionNecronDissolveTween : GameAction
{
	[Tooltip("Target NecronDissolveGroup on the scene (by ControllableComponent UniqueId)")]
	public ControllableReference IdOfObject;

	[Tooltip("Show = make visible (DissolveMoveValue → -10). Hide = dissolve away (DissolveMoveValue → +10).")]
	public NecronDissolveAction Action;

	[Tooltip("Tween duration in seconds. 0 = instant. Free input — any positive number.")]
	public float Duration = 1.5f;

	public override string GetCaption()
	{
		return $"NecronDissolve {IdOfObject?.EntityNameInEditor} -> {Action} over {Duration}s";
	}

	protected override void RunAction()
	{
		if (IdOfObject == null || string.IsNullOrEmpty(IdOfObject.UniqueId))
		{
			PFLog.TechArt.Warning("[ControllableActionNecronDissolveTween] IdOfObject is null or empty");
			return;
		}
		if (!Game.Instance.SceneControllables.TryGetControllable(IdOfObject.UniqueId, out var controllableComponent))
		{
			PFLog.TechArt.Warning("[ControllableActionNecronDissolveTween] Controllable '" + IdOfObject.EntityNameInEditor + "' (" + IdOfObject.UniqueId + ") not found in SceneControllables.");
			return;
		}
		NecronDissolveGroup necronDissolveGroup = controllableComponent as NecronDissolveGroup;
		if (necronDissolveGroup == null)
		{
			PFLog.TechArt.Warning("[ControllableActionNecronDissolveTween] Controllable '" + IdOfObject.EntityNameInEditor + "' is " + controllableComponent.GetType().Name + ", expected NecronDissolveGroup.");
		}
		else
		{
			necronDissolveGroup.AnimateTo(Action, Duration);
		}
	}
}
