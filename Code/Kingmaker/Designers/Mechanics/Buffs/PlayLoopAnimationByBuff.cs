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

	private void TrySetAction()
	{
		UnitAnimationManager unitAnimationManager = base.Owner?.View?.AnimationManager;
		if (unitAnimationManager != null)
		{
			UnitAnimationActionHandle unitAnimationActionHandle = (UnitAnimationActionHandle)unitAnimationManager.CreateHandle(BuffLoopAction);
			unitAnimationManager.Execute(unitAnimationActionHandle);
			unitAnimationManager.BuffLoopAction = unitAnimationActionHandle;
		}
	}

	private void TryResetAction()
	{
		UnitAnimationManager unitAnimationManager = base.Owner?.View?.AnimationManager;
		if (unitAnimationManager != null && unitAnimationManager.BuffLoopAction.Action is WarhammerBuffLoopAction warhammerBuffLoopAction)
		{
			warhammerBuffLoopAction.SwitchToExit(unitAnimationManager.BuffLoopAction);
		}
		else if (unitAnimationManager != null && unitAnimationManager.BuffLoopAction != null)
		{
			unitAnimationManager.BuffLoopAction.Release();
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
