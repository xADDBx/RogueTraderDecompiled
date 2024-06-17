using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.ActionBar;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models.UnitSettings;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Kingmaker.Code.UI.MVVM.View.ActionBar;

public abstract class ActionBarBaseSlotView : ViewBase<ActionBarSlotVM>, IWidgetView, IHasVisibility
{
	[SerializeField]
	protected OwlcatMultiButton MainButton;

	[Header("Icon Block")]
	[SerializeField]
	private Image m_Icon;

	private VisibilityController m_IconVisibility;

	[SerializeField]
	private _2dxFX_GrayScale m_GrayScale;

	[Range(0f, 1f)]
	[SerializeField]
	private float m_GrayScaleAlpha = 0.8f;

	[SerializeField]
	private UICornerCut m_ForeIcon;

	private VisibilityController m_ForeIconVisibility;

	[SerializeField]
	private Image m_SelectedMark;

	[SerializeField]
	private FadeAnimator m_TargetPingEntity;

	private VisibilityController m_SelectedMarkVisibility;

	[Header("Attack Ability Group Cooldown Alert Mark")]
	[SerializeField]
	private Image m_AttackAbilityGroupCooldownAlertMark;

	[SerializeField]
	private Color m_MinAlertColor;

	[SerializeField]
	private Color m_MaxAlertColor;

	[SerializeField]
	private float m_AlertAnimationBlinkTime = 0.4f;

	[Header("Action Point Block")]
	[SerializeField]
	private GameObject m_ActionPointBlock;

	private VisibilityController m_ActionPointVisibility;

	[SerializeField]
	private Slider m_ActionPoints;

	[SerializeField]
	private int m_ActionPointsMaxValue = 6;

	[Header("Splash Block")]
	[SerializeField]
	private RectTransform m_SplashAnim;

	private VisibilityController m_SplashBlockVisibility;

	private readonly float m_SplashBasePosX = -80f;

	private readonly float m_SplashFinalPosX = 34f;

	private Tweener m_AttackAbilityGroupCooldownAlertTweener;

	private VisibilityController m_SlotVisibility;

	private IDisposable m_PingDelay;

	private bool m_IsPingInProcess;

	public MechanicActionBarSlot MechanicActionBarSlot => base.ViewModel.MechanicActionBarSlot;

	public bool IsEmpty => base.ViewModel.IsEmpty.Value;

	public MonoBehaviour MonoBehaviour => this;

	protected virtual void Awake()
	{
		m_SlotVisibility = VisibilityController.Control(base.transform);
		m_IconVisibility = VisibilityController.Control(m_Icon);
		m_ForeIconVisibility = VisibilityController.ControlParent(m_ForeIcon);
		m_SelectedMarkVisibility = VisibilityController.Control(m_SelectedMark);
		m_ActionPointVisibility = VisibilityController.Control(m_ActionPointBlock);
		m_SplashBlockVisibility = VisibilityController.ControlParent(m_SplashAnim);
	}

	public virtual void Initialize()
	{
		SetLayer();
		if (!(m_SplashAnim == null))
		{
			Vector2 anchoredPosition = m_SplashAnim.anchoredPosition;
			anchoredPosition.x = m_SplashBasePosX;
			m_SplashAnim.anchoredPosition = anchoredPosition;
			m_SplashBlockVisibility.SetVisible(visible: false);
		}
	}

	protected override void BindViewImplementation()
	{
		m_SlotVisibility.SetVisible(visible: true);
		if (m_TargetPingEntity != null)
		{
			if (m_TargetPingEntity.CanvasGroup != null)
			{
				m_TargetPingEntity.CanvasGroup.alpha = 0f;
			}
			m_TargetPingEntity.DisappearAnimation();
		}
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate
		{
			SetIcon();
		}));
		AddDisposable(base.ViewModel.ForeIcon.Subscribe(delegate
		{
			SetForeIcon();
		}));
		AddDisposable(base.ViewModel.ActionPointCost.Subscribe(delegate(int value)
		{
			if (!(m_ActionPointBlock == null))
			{
				m_ActionPointVisibility.SetVisible(value >= 0);
				m_ActionPoints.maxValue = ((value > m_ActionPointsMaxValue) ? value : m_ActionPointsMaxValue);
				m_ActionPoints.value = value;
			}
		}));
		AddDisposable(base.ViewModel.OnClickCommand.Subscribe(delegate
		{
			SplashAnimation();
		}));
		AddDisposable(base.ViewModel.IsCasting.CombineLatest(base.ViewModel.IsEmpty, (bool casting, bool empty) => new { casting, empty }).Subscribe(value =>
		{
			SetLayer();
		}));
		AddDisposable(base.ViewModel.IsPossibleActive.CombineLatest(base.ViewModel.ResourceCount, base.ViewModel.HasConvert, base.ViewModel.HasAvailableConvert, base.ViewModel.IsFake, delegate(bool active, int count, bool convert, bool convertAvailable, bool fake)
		{
			if (active && convert)
			{
				return count != 0 && convertAvailable;
			}
			return (active && !fake) || base.ViewModel.IsInCharScreen;
		}).Subscribe(delegate(bool value)
		{
			if (!(m_GrayScale == null))
			{
				m_GrayScale.EffectAmount = ((!value) ? 1 : 0);
				m_GrayScale.Alpha = (value ? 1f : m_GrayScaleAlpha);
			}
		}));
		AddDisposable(base.ViewModel.IsSelected.Subscribe(m_SelectedMarkVisibility.SetVisible));
		AddDisposable(base.ViewModel.IsAlerted.Subscribe(delegate(bool value)
		{
			if (!(m_AttackAbilityGroupCooldownAlertMark == null))
			{
				if (value)
				{
					PlayAttackAbilityGroupCooldownAlertAnimation();
				}
				else
				{
					StopAttackAbilityGroupCooldownAlertAnimation();
				}
			}
		}));
		AddDisposable(base.ViewModel.CoopPingActionBarSlot.Subscribe(PingActionBarAbility));
	}

	protected override void DestroyViewImplementation()
	{
		m_AttackAbilityGroupCooldownAlertTweener?.Kill();
		m_AttackAbilityGroupCooldownAlertTweener = null;
		m_PingDelay?.Dispose();
		m_PingDelay = null;
		m_SlotVisibility.SetVisible(visible: false);
		m_ActionPointVisibility?.SetVisible(visible: false);
	}

	public virtual void SetTooltipCustomPosition(RectTransform rectTransform, List<Vector2> pivots = null)
	{
	}

	public void SetInteractable(bool value)
	{
		MainButton.Interactable = value;
	}

	private void SetLayer()
	{
		ActionBarSlotVM viewModel = base.ViewModel;
		if (viewModel == null || viewModel.IsEmpty.Value)
		{
			MainButton.SetActiveLayer(2);
		}
		else
		{
			MainButton.SetActiveLayer(base.ViewModel.IsCasting.Value ? 1 : 0);
		}
	}

	private void SetIcon()
	{
		Sprite value = base.ViewModel.Icon.Value;
		bool visible = value != null;
		m_IconVisibility.SetVisible(visible);
		m_Icon.sprite = value;
	}

	private void SetForeIcon()
	{
		if (!(m_ForeIcon == null))
		{
			Sprite value = base.ViewModel.ForeIcon.Value;
			m_ForeIconVisibility.SetVisible(value != null);
			m_ForeIcon.sprite = value;
		}
	}

	private void SplashAnimation()
	{
		if (!(m_SplashAnim == null))
		{
			m_SplashAnim.DOKill();
			m_SplashBlockVisibility.SetVisible(visible: true);
			Vector2 anchoredPosition = m_SplashAnim.anchoredPosition;
			anchoredPosition.x = m_SplashBasePosX;
			m_SplashAnim.anchoredPosition = anchoredPosition;
			m_SplashAnim.DOAnchorPosX(m_SplashFinalPosX, 0.4f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
			{
				m_SplashBlockVisibility.SetVisible(visible: false);
			});
		}
	}

	private void StopAttackAbilityGroupCooldownAlertAnimation()
	{
		Color color = m_AttackAbilityGroupCooldownAlertMark.color;
		color.a = 0f;
		m_AttackAbilityGroupCooldownAlertMark.color = color;
		m_AttackAbilityGroupCooldownAlertTweener.Kill();
		m_AttackAbilityGroupCooldownAlertTweener = null;
	}

	private void PlayAttackAbilityGroupCooldownAlertAnimation()
	{
		m_AttackAbilityGroupCooldownAlertTweener?.Kill();
		m_AttackAbilityGroupCooldownAlertMark.color = m_MinAlertColor;
		m_AttackAbilityGroupCooldownAlertTweener = m_AttackAbilityGroupCooldownAlertMark.DOColor(m_MaxAlertColor, m_AlertAnimationBlinkTime).SetLoops(-1, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true)
			.SetAutoKill(autoKillOnCompletion: true);
		m_AttackAbilityGroupCooldownAlertTweener.Play();
	}

	public void BindWidgetVM(IViewModel viewModel)
	{
		Bind(viewModel as ActionBarSlotVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ActionBarSlotVM;
	}

	public void SetVisible(bool visible)
	{
		m_SlotVisibility.SetVisible(visible);
	}

	public void PingActionBarAbility(bool show)
	{
		if (m_TargetPingEntity == null)
		{
			return;
		}
		if (!show)
		{
			if (m_IsPingInProcess)
			{
				m_TargetPingEntity.DisappearAnimation(delegate
				{
					m_IsPingInProcess = false;
				});
			}
			return;
		}
		m_PingDelay?.Dispose();
		m_TargetPingEntity.AppearAnimation();
		m_IsPingInProcess = true;
		m_PingDelay = DelayedInvoker.InvokeInTime(delegate
		{
			m_TargetPingEntity.DisappearAnimation(delegate
			{
				m_IsPingInProcess = false;
			});
		}, 7.5f);
	}
}
