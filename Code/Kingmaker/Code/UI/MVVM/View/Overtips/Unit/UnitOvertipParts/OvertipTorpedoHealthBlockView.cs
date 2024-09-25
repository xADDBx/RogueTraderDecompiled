using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipTorpedoHealthBlockView : ViewBase<OvertipHealthBlockVM>
{
	[Serializable]
	private struct OvertipTorpedoHealthPoint
	{
		public GameObject GameObject;

		public Image Image;

		public Image DamageImage;
	}

	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private OvertipTorpedoHealthPoint[] m_HealthPoints;

	[SerializeField]
	private Color[] m_HealthPointColors;

	[SerializeField]
	private CanvasGroup m_DamageHealthCanvasGroup;

	[SerializeField]
	private float m_DamageHealthBlinkAlpha = 0.5f;

	[SerializeField]
	private List<UnitOvertipVisibilitySettings> m_UnitOvertipVisibilitySettings;

	private float m_CurrentHealth;

	private ReactiveProperty<UnitOvertipVisibility> m_Visibility = new ReactiveProperty<UnitOvertipVisibility>();

	private Tweener m_FadeAnimator;

	private Tweener m_SizeAnimator;

	private StringReactiveProperty m_HPLeftHint = new StringReactiveProperty();

	private StringReactiveProperty m_HPMaxHint = new StringReactiveProperty();

	private Tweener m_DamageHelperTweener;

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
		AddDisposable(base.ViewModel.UnitState.IsEnemy.CombineLatest(base.ViewModel.UnitState.IsPlayer, (bool enemy, bool player) => new { enemy, player }).Subscribe(value =>
		{
			int num2 = ((!value.player) ? (value.enemy ? 1 : 2) : 0);
			for (int m = 0; m < m_HealthPoints.Length; m++)
			{
				m_HealthPoints[m].Image.color = m_HealthPointColors[num2];
			}
		}));
		AddDisposable(base.ViewModel.HitPointTotalMax.Subscribe(delegate(int value)
		{
			for (int l = 0; l < m_HealthPoints.Length; l++)
			{
				m_HealthPoints[l].GameObject.SetActive(l < value);
				m_HealthPoints[l].DamageImage.gameObject.SetActive(l < value);
			}
			m_HPMaxHint.Value = GetHPMaxHint();
		}));
		AddDisposable(base.ViewModel.HitPointLeft.Subscribe(delegate(int value)
		{
			for (int k = 0; k < m_HealthPoints.Length; k++)
			{
				m_HealthPoints[k].Image.enabled = k < value;
				m_HealthPoints[k].DamageImage.enabled = false;
			}
		}));
		AddDisposable(base.ViewModel.HitPointLeft.CombineLatest(base.ViewModel.HitPointMax, base.ViewModel.MinDamage, base.ViewModel.MaxDamage, base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.UnitState.IsAoETarget, (int left, int max, int minDamage, int maxDamage, bool hover, bool isAoE) => new { left, max, minDamage, maxDamage, hover, isAoE }).ObserveLastValueOnLateUpdate().Subscribe(value =>
		{
			bool flag = (value.hover || value.isAoE) && value.minDamage > 0 && base.ViewModel.Shield.Value <= 0;
			int minDamage2 = value.minDamage;
			int num = value.left - 1;
			for (int i = 0; i < minDamage2; i++)
			{
				m_HealthPoints[num].Image.enabled = false;
				m_HealthPoints[num].DamageImage.enabled = true;
				num--;
				if (num < 0)
				{
					break;
				}
			}
			m_DamageHelperTweener?.Kill();
			if (flag)
			{
				m_DamageHealthCanvasGroup.alpha = 1f;
				m_DamageHelperTweener = m_DamageHealthCanvasGroup.DOFade(m_DamageHealthBlinkAlpha, 0.4f).SetLoops(-1, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true);
				m_DamageHelperTweener.Play();
			}
			else
			{
				m_DamageHealthCanvasGroup.alpha = 0f;
				for (int j = 0; j < m_HealthPoints.Length; j++)
				{
					m_HealthPoints[j].Image.enabled = j < value.left;
					m_HealthPoints[j].DamageImage.enabled = false;
				}
			}
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

	private string GetHPLeftHint()
	{
		if (base.ViewModel.HitPointTemporary.Value == 0)
		{
			return $"{base.ViewModel.HitPointTotalLeft.Value} {UIStrings.Instance.Tooltips.HPLeft.Text}";
		}
		return $"{base.ViewModel.HitPointTotalLeft.Value} {UIStrings.Instance.Tooltips.HPTotalLeft.Text}\r\n<separator>\r\n{base.ViewModel.HitPointLeft.Value} {UIStrings.Instance.Tooltips.HPLeft.Text}\r\n{base.ViewModel.HitPointTemporary.Value} {UIStrings.Instance.Tooltips.HPTemporary.Text}";
	}

	private string GetHPMaxHint()
	{
		if (base.ViewModel.HitPointTemporary.Value == 0)
		{
			return $"{base.ViewModel.HitPointTotalMax.Value} {UIStrings.Instance.Tooltips.HPMax.Text}";
		}
		return $"{base.ViewModel.HitPointTotalLeft.Value} {UIStrings.Instance.Tooltips.HPTotalMax.Text}\r\n<separator>\r\n{base.ViewModel.HitPointLeft.Value} {UIStrings.Instance.Tooltips.HPMax.Text}\r\n{base.ViewModel.HitPointTemporary.Value} {UIStrings.Instance.Tooltips.HPTemporary.Text}";
	}
}
