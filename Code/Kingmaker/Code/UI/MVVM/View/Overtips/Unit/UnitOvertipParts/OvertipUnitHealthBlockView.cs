using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.SmartSliders;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;

public class OvertipUnitHealthBlockView : ViewBase<OvertipHealthBlockVM>
{
	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private List<UnitOvertipVisibilitySettings> m_VisibilitySettings;

	[SerializeField]
	private CanvasGroup m_LargeArtBlock;

	[SerializeField]
	private CanvasGroup m_MediumArtBlock;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[Header("Health label block")]
	[SerializeField]
	private CanvasGroup m_HealthLabelBlock;

	[SerializeField]
	private TextMeshProUGUI m_HealthLabel;

	private CanvasRenderer m_HealthLabelCanvasRenderer;

	[Header("Sliders")]
	[SerializeField]
	private Slider m_HPMaxSlider;

	[SerializeField]
	private DelayedSlider m_HPLeftSlider;

	[SerializeField]
	private AdditionalSlider m_HPTempSlider;

	[SerializeField]
	private RangedSlider m_MinDamageSlider;

	[SerializeField]
	private RangedSlider m_MaxDamageSlider;

	[Header("Death")]
	[SerializeField]
	private CanvasGroup m_DeathMark;

	private ReactiveProperty<UnitOvertipVisibility> m_Visibility;

	private Tweener m_FadeAnimator;

	private Tweener m_SizeAnimator;

	private StringReactiveProperty m_HPLeftHint = new StringReactiveProperty();

	private StringReactiveProperty m_HPMaxHint = new StringReactiveProperty();

	private bool m_IsBinding;

	private bool CheckDestructible
	{
		get
		{
			if (!base.ViewModel.UnitState.IsDestructible.Value)
			{
				return true;
			}
			if (!base.ViewModel.UnitState.IsTBM.Value)
			{
				return false;
			}
			if (base.ViewModel.UnitState.IsDestructibleNotCover.Value)
			{
				return true;
			}
			return base.ViewModel.UnitState.IsMouseOverUnit.Value;
		}
	}

	private bool IsVisible
	{
		get
		{
			if (!base.ViewModel.UnitState.HasHiddenCondition.Value && base.ViewModel.UnitState.IsInCombat.Value && !base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead.Value)
			{
				return CheckDestructible;
			}
			return false;
		}
	}

	public void Initialize(ReactiveProperty<UnitOvertipVisibility> visibilityProperty)
	{
		m_Visibility = visibilityProperty;
		m_HPLeftSlider.Initialize();
		m_HPTempSlider.Initialize();
		m_MinDamageSlider.Initialize();
		m_MaxDamageSlider.Initialize();
		m_HealthLabelCanvasRenderer = m_HealthLabel.GetComponent<CanvasRenderer>();
	}

	protected override void BindViewImplementation()
	{
		m_IsBinding = true;
		AddDisposable(base.ViewModel.UnitState.IsEnemy.CombineLatest(base.ViewModel.UnitState.IsPlayer, base.ViewModel.UnitState.IsDestructibleNotCover, (bool enemy, bool player, bool destructible) => new { enemy, player, destructible }).Subscribe(value =>
		{
			int activeLayer = ((!value.player) ? ((value.enemy || value.destructible) ? 1 : 2) : 0);
			m_MultiSelectable.SetActiveLayer(activeLayer);
		}));
		AddDisposable(base.ViewModel.HitPointTotalMax.Subscribe(delegate(int value)
		{
			m_HPMaxSlider.maxValue = value;
			m_HPMaxHint.Value = GetHPMaxHint();
		}));
		AddDisposable(base.ViewModel.HitPointLeft.Subscribe(delegate(int value)
		{
			m_HPLeftSlider.SetValue(value, !m_IsBinding);
		}));
		AddDisposable(base.ViewModel.HitPointTemporary.Subscribe(delegate(int value)
		{
			m_HPTempSlider.SetAddition(value);
		}));
		AddDisposable(base.ViewModel.MinDamage.Subscribe(delegate(int value)
		{
			m_MinDamageSlider.SetRange(base.ViewModel.HitPointTotalLeft.Value - value, base.ViewModel.HitPointTotalLeft.Value, blink: true);
		}));
		AddDisposable(base.ViewModel.MaxDamage.Subscribe(delegate(int value)
		{
			m_MaxDamageSlider.SetRange(base.ViewModel.HitPointTotalLeft.Value - value, base.ViewModel.HitPointTotalLeft.Value, blink: true);
		}));
		AddDisposable(base.ViewModel.HitPointTotalLeft.Subscribe(delegate(int value)
		{
			m_HealthLabel.text = value.ToString();
		}));
		AddDisposable(base.ViewModel.CanDie.Subscribe(delegate(bool value)
		{
			m_DeathMark.alpha = (value ? 1 : 0);
			m_DeathMark.blocksRaycasts = value;
		}));
		AddDisposable(m_Visibility.CombineLatest(base.ViewModel.UnitState.IsEnemy, base.ViewModel.UnitState.IsInCombat, base.ViewModel.UnitState.HasHiddenCondition, base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead, base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.UnitState.ForceHotKeyPressed, (UnitOvertipVisibility _, bool _, bool _, bool _, bool _, bool _, bool _) => true).Subscribe(delegate
		{
			DoVisibility();
		}));
		AddDisposable(base.ViewModel.HitPointTemporary.CombineLatest(base.ViewModel.HitPointTotalLeft, base.ViewModel.HitPointTotalMax, (int _, int _, int _) => true).ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			m_HPMaxHint.Value = GetHPMaxHint();
			m_HPLeftHint.Value = GetHPLeftHint();
		}));
		AddDisposable(m_HealthLabel.SetHint(m_HPLeftHint, null, m_HealthLabelCanvasRenderer.GetColor()));
		AddDisposable(m_HPMaxSlider.SetHint(m_HPMaxHint, null, m_HealthLabelCanvasRenderer.GetColor()));
		AddDisposable(m_DeathMark.GetComponent<Image>().SetHint(UIStrings.Instance.Tooltips.PossibleToKill.Text));
		m_IsBinding = false;
	}

	private void DoVisibility()
	{
		UnitOvertipVisibilitySettings unitOvertipVisibilitySettings = m_VisibilitySettings.FirstItem((UnitOvertipVisibilitySettings s) => s.UnitOvertipVisibility == m_Visibility.Value);
		float num = (IsVisible ? 1f : 0f);
		float endValue = unitOvertipVisibilitySettings.Alpha * num;
		Vector2 size = unitOvertipVisibilitySettings.Size;
		m_FadeAnimator?.Kill();
		m_FadeAnimator = m_CanvasGroup.DOFade(endValue, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_SizeAnimator?.Kill();
		m_SizeAnimator = m_RectTransform.DOSizeDelta(size, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_CanvasGroup.blocksRaycasts = IsVisible;
		bool flag = m_Visibility.Value == UnitOvertipVisibility.Maximized;
		UnitOvertipVisibility value = m_Visibility.Value;
		bool flag2 = value == UnitOvertipVisibility.Full || value == UnitOvertipVisibility.Maximized;
		m_MediumArtBlock.alpha = (flag2 ? 1f : 0f);
		m_LargeArtBlock.alpha = (flag ? 1f : 0f);
		m_HealthLabelBlock.alpha = (flag2 ? num : 0f);
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

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator?.Kill();
		m_SizeAnimator?.Kill();
		m_CanvasGroup.blocksRaycasts = false;
		m_CanvasGroup.alpha = 0f;
	}
}
