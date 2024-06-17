using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipShipLevelBlockView : ViewBase<OvertipShipLevelBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ShipLevel;

	[SerializeField]
	private FadeAnimator m_Animator;

	private bool CheckVisibleTrigger
	{
		get
		{
			if ((!base.ViewModel.UnitState.IsTBM.Value || !base.ViewModel.UnitState.IsCurrentUnitTurn.Value) && !base.ViewModel.UnitState.ForceHotKeyPressed.Value)
			{
				if (base.ViewModel.UnitState.IsMouseOverUnit.Value)
				{
					return base.ViewModel.UnitState.IsPlayer.Value;
				}
				return false;
			}
			return true;
		}
	}

	private bool CheckCanBeVisible
	{
		get
		{
			if (base.ViewModel.UnitState.IsVisibleForPlayer.Value && !base.ViewModel.UnitState.HasHiddenCondition.Value && !base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead.Value)
			{
				return base.ViewModel.UnitState.IsPlayer.Value;
			}
			return false;
		}
	}

	private bool CheckVisibility
	{
		get
		{
			if (CheckCanBeVisible)
			{
				return CheckVisibleTrigger;
			}
			return false;
		}
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Level.Subscribe(delegate(string value)
		{
			m_ShipLevel.text = value;
		}));
		AddDisposable(base.ViewModel.UnitState.IsCurrentUnitTurn.CombineLatest(base.ViewModel.UnitState.ForceHotKeyPressed, base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.UnitState.IsVisibleForPlayer, base.ViewModel.UnitState.HasHiddenCondition, base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead, (bool isCurrentUnitTurn, bool forceHotKeyPressed, bool isMouseOverUnit, bool isVisibleForPlayer, bool hasHiddenCondition, bool isDead) => new { isCurrentUnitTurn, forceHotKeyPressed, isMouseOverUnit, isVisibleForPlayer, hasHiddenCondition, isDead }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			m_Animator.PlayAnimation(CheckVisibility);
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
