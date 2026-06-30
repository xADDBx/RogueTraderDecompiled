using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Actions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Scripting;

namespace Kingmaker.Designers.Mechanics.Buffs;

[TypeId("ccdeb99837c64fb79ebc26eb36f2f47b")]
public class PlayLoopAnimationByBuff : UnitBuffComponentDelegate, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	[Preserve]
	private class LoopComponentRuntime : UnitBuffComponentRuntime, IHashable
	{
		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public WarhammerBuffLoopAction BuffLoopAction;

	[Tooltip("Анимация, которая проигрывается один раз при снятии баффа (не связана с loop-анимацией)")]
	[SerializeField]
	private UnitAnimationAction m_DeactivationAction;

	[SerializeField]
	private BlueprintBuffReference m_SuppressionBuff;

	public BlueprintBuff SuppressionBuff => m_SuppressionBuff?.Get();

	public override EntityFactComponent CreateRuntimeFactComponent()
	{
		return new LoopComponentRuntime();
	}

	protected override void OnActivateOrPostLoad()
	{
		TrySetAction(ObjectExtensions.Or(base.Owner.View, null)?.AnimationManager);
		base.OnActivateOrPostLoad();
	}

	protected override void OnDeactivate()
	{
		UnitAnimationManager animationManager = ObjectExtensions.Or(base.Owner.View, null)?.AnimationManager;
		TryResetAction(animationManager);
		TryPlayDeactivationAction(animationManager);
		base.OnDeactivate();
	}

	protected override void OnViewDidAttach()
	{
		TrySetAction(ObjectExtensions.Or(base.Owner.View, null)?.AnimationManager);
		base.OnViewDidAttach();
	}

	protected override void OnViewWillDetach()
	{
		TryResetAction(ObjectExtensions.Or(base.Owner.View, null)?.AnimationManager);
		base.OnViewDidAttach();
	}

	public void TrySetAction(UnitAnimationManager animationManager, bool skipEnter = false)
	{
		if (animationManager != null)
		{
			UnitAnimationActionHandle buffLoopAction = animationManager.BuffLoopAction;
			if (buffLoopAction != null && !buffLoopAction.IsReleased && buffLoopAction.Action is WarhammerBuffLoopAction warhammerBuffLoopAction && !warhammerBuffLoopAction.IsExiting(buffLoopAction))
			{
				PFLog.Animations.Error(base.Fact.Blueprint, $"Trying to start BuffLoopAction {BuffLoopAction} before removing the previous one. This is not properly supported!");
			}
			UnitAnimationActionHandle unitAnimationActionHandle = (UnitAnimationActionHandle)animationManager.CreateHandle(BuffLoopAction);
			unitAnimationActionHandle.SkipEnterAnimation = skipEnter;
			animationManager.Execute(unitAnimationActionHandle);
			animationManager.BuffLoopAction = unitAnimationActionHandle;
		}
	}

	public void TryRequeueAction(UnitAnimationManager animationManager, bool skipEnter = false)
	{
		BuffLoopAction.ExecutionMode = ExecutionMode.Sequenced;
		TrySetAction(animationManager, skipEnter);
	}

	public void TryResetAction(UnitAnimationManager animationManager)
	{
		if (animationManager != null && animationManager.BuffLoopAction != null)
		{
			if (animationManager.BuffLoopAction.Action is WarhammerBuffLoopAction warhammerBuffLoopAction)
			{
				warhammerBuffLoopAction.SwitchToExit(animationManager.BuffLoopAction);
			}
			else
			{
				animationManager.BuffLoopAction.Release();
			}
		}
	}

	private void TryPlayDeactivationAction(UnitAnimationManager animationManager)
	{
		if (!(animationManager == null) && !(m_DeactivationAction == null))
		{
			AnimationActionHandle handle = animationManager.CreateHandle(m_DeactivationAction);
			animationManager.Execute(handle);
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.Executor == base.Owner && SuppressionBuff != null)
		{
			BuffDuration duration = new BuffDuration(null, BuffEndCondition.TurnStartOrCombatEnd);
			base.Owner.Buffs.Add(SuppressionBuff, duration);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
