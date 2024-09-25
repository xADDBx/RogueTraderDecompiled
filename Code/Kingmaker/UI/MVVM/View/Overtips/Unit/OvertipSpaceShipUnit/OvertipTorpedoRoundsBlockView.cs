using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit;
using Kingmaker.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Overtips.Unit.OvertipSpaceShipUnit;

public class OvertipTorpedoRoundsBlockView : ViewBase<OvertipTorpedoVM>
{
	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_RoundsLeftLabel;

	[SerializeField]
	private List<UnitOvertipVisibilitySettings> m_UnitOvertipVisibilitySettings;

	private ReactiveProperty<UnitOvertipVisibility> m_Visibility = new ReactiveProperty<UnitOvertipVisibility>();

	private Tweener m_FadeAnimator;

	private Tweener m_SizeAnimator;

	private bool IsVisible
	{
		get
		{
			if (!base.ViewModel.UnitState.HasHiddenCondition.Value && (base.ViewModel.UnitState.IsEnemy.Value || base.ViewModel.UnitState.IsInCombat.Value))
			{
				return !base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead.Value;
			}
			return false;
		}
	}

	public void Initialize(ReactiveProperty<UnitOvertipVisibility> visibilityProperty)
	{
		m_Visibility = visibilityProperty;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.RoundsLeft.Subscribe(delegate(int value)
		{
			m_RoundsLeftLabel.text = value.ToString();
		}));
		AddDisposable(base.ViewModel.UnitState.IsEnemy.CombineLatest(base.ViewModel.UnitState.IsInCombat, base.ViewModel.UnitState.HasHiddenCondition, base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead, (bool enemy, bool combat, bool shouldBeHidden, bool isDead) => new { enemy, combat, shouldBeHidden, isDead }).Subscribe(value =>
		{
			bool active = !value.shouldBeHidden && (value.enemy || value.combat) && !value.isDead;
			base.gameObject.SetActive(active);
		}));
		AddDisposable(m_Visibility.CombineLatest(base.ViewModel.UnitState.IsEnemy, base.ViewModel.UnitState.IsInCombat, base.ViewModel.UnitState.HasHiddenCondition, base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead, (UnitOvertipVisibility _, bool _, bool _, bool _, bool _) => true).Subscribe(delegate
		{
			DoVisibility();
		}));
	}

	private void DoVisibility()
	{
		UnitOvertipVisibilitySettings unitOvertipVisibilitySettings = m_UnitOvertipVisibilitySettings.FirstItem((UnitOvertipVisibilitySettings s) => s.UnitOvertipVisibility == m_Visibility.Value);
		float num = (IsVisible ? 1f : 0f);
		float endValue = unitOvertipVisibilitySettings.Alpha * num;
		Vector2 size = unitOvertipVisibilitySettings.Size;
		m_FadeAnimator?.Kill();
		m_FadeAnimator = m_CanvasGroup.DOFade(endValue, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_SizeAnimator?.Kill();
		m_SizeAnimator = m_RectTransform.DOSizeDelta(size, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_CanvasGroup.blocksRaycasts = IsVisible;
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator?.Kill();
		m_SizeAnimator?.Kill();
	}
}
