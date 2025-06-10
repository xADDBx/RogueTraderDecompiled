using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
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

namespace Kingmaker.Designers.Mechanics.Buffs;

[TypeId("ccdeb99837c64fb79ebc26eb36f2f47b")]
public class PlayLoopAnimationByBuff : UnitBuffComponentDelegate, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public WarhammerBuffLoopAction BuffLoopAction;

	[SerializeField]
	private BlueprintBuffReference m_SuppressionBuff;

	private UnitAnimationManager m_AnimationManager;

	public BlueprintBuff SuppressionBuff => m_SuppressionBuff?.Get();

	protected override void OnActivateOrPostLoad()
	{
		BaseUnitEntity owner = base.Owner;
		m_AnimationManager = ((owner == null) ? null : ObjectExtensions.Or(owner.View, null)?.AnimationManager);
		TrySetAction();
		base.OnActivateOrPostLoad();
	}

	protected override void OnDeactivate()
	{
		m_AnimationManager = null;
		TryResetAction();
		base.OnDeactivate();
	}

	protected override void OnViewDidAttach()
	{
		BaseUnitEntity owner = base.Owner;
		m_AnimationManager = ((owner == null) ? null : ObjectExtensions.Or(owner.View, null)?.AnimationManager);
		TrySetAction();
		base.OnViewDidAttach();
	}

	protected override void OnViewWillDetach()
	{
		m_AnimationManager = null;
		TryResetAction();
		base.OnViewDidAttach();
	}

	public void TrySetAction(bool skipEnter = false)
	{
		if (m_AnimationManager != null)
		{
			UnitAnimationActionHandle buffLoopAction = m_AnimationManager.BuffLoopAction;
			if (buffLoopAction != null && !buffLoopAction.IsReleased && buffLoopAction.Action is WarhammerBuffLoopAction warhammerBuffLoopAction && !warhammerBuffLoopAction.IsExiting(buffLoopAction))
			{
				PFLog.Animations.Error(base.Fact.Blueprint, $"Trying to start BuffLoopAction {BuffLoopAction} before removing the previous one. This is not properly supported!");
			}
			UnitAnimationActionHandle unitAnimationActionHandle = (UnitAnimationActionHandle)m_AnimationManager.CreateHandle(BuffLoopAction);
			unitAnimationActionHandle.SkipEnterAnimation = skipEnter;
			m_AnimationManager.Execute(unitAnimationActionHandle);
			m_AnimationManager.BuffLoopAction = unitAnimationActionHandle;
		}
	}

	public void TryRequeueAction(bool skipEnter = false)
	{
		if (m_AnimationManager != null)
		{
			UnitAnimationActionHandle buffLoopAction = m_AnimationManager.BuffLoopAction;
			if (buffLoopAction != null && !buffLoopAction.IsReleased && buffLoopAction.Action is WarhammerBuffLoopAction warhammerBuffLoopAction && !warhammerBuffLoopAction.IsExiting(buffLoopAction))
			{
				PFLog.Animations.Error(base.Fact.Blueprint, $"Trying to start BuffLoopAction {BuffLoopAction} before removing the previous one. This is not properly supported!");
			}
			BuffLoopAction.ExecutionMode = ExecutionMode.Sequenced;
			UnitAnimationActionHandle unitAnimationActionHandle = (UnitAnimationActionHandle)m_AnimationManager.CreateHandle(BuffLoopAction);
			unitAnimationActionHandle.SkipEnterAnimation = skipEnter;
			m_AnimationManager.Execute(unitAnimationActionHandle);
			m_AnimationManager.BuffLoopAction = unitAnimationActionHandle;
		}
	}

	public void TryResetAction()
	{
		BaseUnitEntity owner = base.Owner;
		UnitAnimationManager unitAnimationManager = ((owner == null) ? null : ObjectExtensions.Or(owner.View, null)?.AnimationManager);
		if (unitAnimationManager != null && unitAnimationManager.BuffLoopAction != null)
		{
			if (unitAnimationManager.BuffLoopAction.Action is WarhammerBuffLoopAction warhammerBuffLoopAction)
			{
				warhammerBuffLoopAction.SwitchToExit(unitAnimationManager.BuffLoopAction);
			}
			else
			{
				unitAnimationManager.BuffLoopAction.Release();
			}
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.Executor == base.Owner && SuppressionBuff != null)
		{
			base.Owner.Buffs.Add(SuppressionBuff, (BuffDuration)null);
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
