using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[TypeId("ccdeb99837c64fb79ebc26eb36f2f47b")]
public class PlayLoopAnimationByBuff : UnitBuffComponentDelegate, IHashable
{
	public WarhammerBuffLoopAction BuffLoopAction;

	private UnitAnimationManager m_UnitAnimationManager;

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

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
