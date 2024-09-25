using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[TypeId("ccdeb99837c64fb79ebc26eb36f2f47b")]
public class PlayLoopAnimationByBuff : UnitBuffComponentDelegate, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public WarhammerBuffLoopAction BuffLoopAction;

	private UnitAnimationManager m_UnitAnimationManager;

	[SerializeField]
	private BlueprintBuffReference m_SuppressionBuff;

	public BlueprintBuff SuppressionBuff => m_SuppressionBuff?.Get();

	protected override void OnActivateOrPostLoad()
	{
		TrySetAction();
		base.OnActivateOrPostLoad();
	}

	protected override void OnDeactivate()
	{
		TryResetAction();
		base.OnDeactivate();
	}

	protected override void OnViewDidAttach()
	{
		TrySetAction();
		base.OnViewDidAttach();
	}

	protected override void OnViewWillDetach()
	{
		TryResetAction();
		base.OnViewDidAttach();
	}

	public void TrySetAction()
	{
		InitUnitAnimationManager();
		if (m_UnitAnimationManager != null)
		{
			UnitAnimationActionHandle unitAnimationActionHandle = (UnitAnimationActionHandle)m_UnitAnimationManager.CreateHandle(BuffLoopAction);
			m_UnitAnimationManager.Execute(unitAnimationActionHandle);
			m_UnitAnimationManager.BuffLoopAction = unitAnimationActionHandle;
		}
	}

	public void TryResetAction()
	{
		InitUnitAnimationManager();
		if (m_UnitAnimationManager != null && m_UnitAnimationManager.BuffLoopAction.Action is WarhammerBuffLoopAction warhammerBuffLoopAction)
		{
			warhammerBuffLoopAction.SwitchToExit(m_UnitAnimationManager.BuffLoopAction);
		}
		else if (m_UnitAnimationManager != null && m_UnitAnimationManager.BuffLoopAction != null)
		{
			m_UnitAnimationManager.BuffLoopAction.Release();
		}
	}

	private void InitUnitAnimationManager()
	{
		if (m_UnitAnimationManager == null)
		{
			m_UnitAnimationManager = base.Owner?.View?.AnimationManager;
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
