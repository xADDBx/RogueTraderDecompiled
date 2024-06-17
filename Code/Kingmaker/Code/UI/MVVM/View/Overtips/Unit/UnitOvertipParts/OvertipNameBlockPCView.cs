using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipNameBlockPCView : ViewBase<OvertipNameBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[SerializeField]
	private FadeAnimator m_Animator;

	private bool CheckVisibleTrigger
	{
		get
		{
			if ((!base.ViewModel.UnitState.IsTBM.Value || !base.ViewModel.UnitState.IsCurrentUnitTurn.Value || Game.Instance.Player.PlayerShip.IsInCombat) && !base.ViewModel.UnitState.ForceHotKeyPressed.Value)
			{
				return base.ViewModel.UnitState.IsMouseOverUnit.Value;
			}
			return true;
		}
	}

	private bool CheckCanBeVisible
	{
		get
		{
			if (base.ViewModel.UnitState.IsVisibleForPlayer.Value && !base.ViewModel.UnitState.HasHiddenCondition.Value)
			{
				return !base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead.Value;
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
		AddDisposable(base.ViewModel.Name.Subscribe(delegate(string value)
		{
			m_CharacterName.text = value;
		}));
		AddDisposable(base.ViewModel.UnitState.IsEnemy.Subscribe(delegate(bool value)
		{
			m_MultiSelectable.SetActiveLayer(value ? "Enemy" : (base.ViewModel.UnitState.IsPlayer.Value ? "Party" : "Ally"));
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
