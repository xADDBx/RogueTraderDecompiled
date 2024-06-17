using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit;

public class LightweightUnitOvertipView : BaseOvertipView<LightweightUnitOvertipVM>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Parts Block")]
	[SerializeField]
	private OvertipLightweightUnitNameView m_NameBlockPCView;

	[Header("Bark")]
	[SerializeField]
	private OvertipBarkBlockView m_BarkBlockPCView;

	[Header("Common Block")]
	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private CanvasGroup m_InnerCanvasGroup;

	[SerializeField]
	private List<UnitOvertipVisibilitySettings> m_UnitOvertipVisibilitySettings;

	[SerializeField]
	private float m_FarDistance = 120f;

	[SerializeField]
	private float m_StandardOvertipPositionYCorrection = 30f;

	private Tweener m_FadeAnimator;

	private Tweener m_ScaleAnimator;

	private readonly ReactiveProperty<UnitOvertipVisibility> m_Visibility = new ReactiveProperty<UnitOvertipVisibility>();

	private List<Graphic> m_AdditionalRaycastBlockers = new List<Graphic>();

	private List<Graphic> m_MainRaycastBlockers = new List<Graphic>();

	private IDisposable m_HoverDelay;

	protected override bool CheckVisibility
	{
		get
		{
			if (!base.ViewModel.UnitState.IsVisibleForPlayer.Value)
			{
				return base.ViewModel.UnitState.IsDead.Value;
			}
			return true;
		}
	}

	private bool CheckVisibleTrigger
	{
		get
		{
			if ((!base.ViewModel.UnitState.IsCurrentUnitTurn.Value || base.ViewModel.UnitState.IsActing.Value) && !base.ViewModel.UnitState.IsMouseOverUnit.Value && !base.ViewModel.UnitState.IsPingUnit.Value)
			{
				return base.ViewModel.IsBarkActive.Value;
			}
			return true;
		}
	}

	private void Awake()
	{
		List<DoNotDisableRaycasts> list = new List<DoNotDisableRaycasts>();
		GetComponentsInChildren(includeInactive: true, list);
		m_MainRaycastBlockers = list.Select((DoNotDisableRaycasts c) => c.GetComponent<Graphic>()).ToList();
		GetComponentsInChildren(includeInactive: true, m_AdditionalRaycastBlockers);
		m_AdditionalRaycastBlockers = m_AdditionalRaycastBlockers.Where((Graphic g) => g.raycastTarget && g.GetComponent<DoNotDisableRaycasts>() == null).ToList();
		EnableMainRaycasts(enable: false);
		EnableAdditionalRaycasts(enable: false);
	}

	private void EnableMainRaycasts(bool enable)
	{
		foreach (Graphic mainRaycastBlocker in m_MainRaycastBlockers)
		{
			mainRaycastBlocker.raycastTarget = enable;
		}
	}

	private void EnableAdditionalRaycasts(bool enable)
	{
		foreach (Graphic additionalRaycastBlocker in m_AdditionalRaycastBlockers)
		{
			additionalRaycastBlocker.raycastTarget = enable;
		}
	}

	protected override void BindViewImplementation()
	{
		m_InnerCanvasGroup.alpha = 0f;
		base.BindViewImplementation();
		base.gameObject.name = base.ViewModel.UnitUIWrapper.Name + "_UnitOvertip";
		m_NameBlockPCView.Bind(base.ViewModel.NameBlockVM);
		m_BarkBlockPCView.Bind(base.ViewModel.BarkBlockVM);
		AddDisposable(base.ViewModel.UnitState.ForceHotKeyPressed.Subscribe(EnableAdditionalRaycasts));
		AddDisposable(base.ViewModel.UnitState.IsVisibleForPlayer.CombineLatest(base.ViewModel.IsBarkActive, base.ViewModel.UnitState.IsCurrentUnitTurn, base.ViewModel.UnitState.ForceHotKeyPressed, base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.UnitState.IsAoETarget, base.ViewModel.CameraDistance, (bool _, bool _, bool _, bool _, bool _, bool _, Vector3 _) => true).ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			UpdateVisibility();
		}));
		base.ViewModel.SetDeathDelay(0.5f);
		AddDisposable(base.ViewModel.UnitState.IsDead.Where((bool v) => v).Subscribe(delegate
		{
			DoDeath();
		}));
		AddDisposable(m_Visibility.Subscribe(DoVisibility));
		AddDisposable(base.ViewModel.UnitState.ForceHotKeyPressed.CombineLatest(m_Visibility, (bool tab, UnitOvertipVisibility vis) => new { tab, vis }).Subscribe());
		EnableMainRaycasts(enable: true);
	}

	private void DoDeath()
	{
		UISounds.Instance.Sounds.Combat.UnitDeath.Play();
	}

	private void DoVisibility(UnitOvertipVisibility unitOvertipVisibility)
	{
		UnitOvertipVisibilitySettings? unitOvertipVisibilitySettings = m_UnitOvertipVisibilitySettings.FirstOrDefault((UnitOvertipVisibilitySettings s) => s.UnitOvertipVisibility == unitOvertipVisibility);
		float alpha = unitOvertipVisibilitySettings.Value.Alpha;
		float scale = unitOvertipVisibilitySettings.Value.Scale;
		m_FadeAnimator?.Kill();
		m_FadeAnimator = m_InnerCanvasGroup.DOFade(alpha, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = m_RectTransform.DOScale(scale, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		if (unitOvertipVisibility == UnitOvertipVisibility.NotFull || unitOvertipVisibility == UnitOvertipVisibility.Full || unitOvertipVisibility == UnitOvertipVisibility.Maximized)
		{
			base.transform.SetAsLastSibling();
		}
		PositionCorrectionFromView = new Vector2(0f, 0f - m_StandardOvertipPositionYCorrection);
	}

	private void UpdateVisibility()
	{
		if (base.ViewModel.UnitState.IsDead.Value)
		{
			return;
		}
		if (!CheckVisibility || base.ViewModel.IsCutscene || base.ViewModel.IsInDialog)
		{
			m_Visibility.Value = UnitOvertipVisibility.Invisible;
			return;
		}
		bool flag = base.ViewModel.CameraDistance.Value.sqrMagnitude < m_FarDistance;
		if (base.ViewModel.UnitState.IsTBM.Value)
		{
			if (CheckVisibleTrigger)
			{
				m_Visibility.Value = (flag ? UnitOvertipVisibility.Full : UnitOvertipVisibility.Near);
			}
			else if (base.ViewModel.UnitState.IsAoETarget.Value)
			{
				m_Visibility.Value = UnitOvertipVisibility.NotFull;
			}
			else
			{
				m_Visibility.Value = ((!flag) ? UnitOvertipVisibility.Far : UnitOvertipVisibility.Near);
			}
		}
		else if (CheckVisibleTrigger)
		{
			m_Visibility.Value = (flag ? UnitOvertipVisibility.Full : UnitOvertipVisibility.Near);
		}
		else
		{
			m_Visibility.Value = ((!flag) ? UnitOvertipVisibility.Far : UnitOvertipVisibility.Near);
		}
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator?.Kill();
		m_FadeAnimator = null;
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = null;
		EnableMainRaycasts(enable: false);
		EnableAdditionalRaycasts(enable: false);
		base.DestroyViewImplementation();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (base.IsBinded && base.ViewModel.UnitState.IsInCombat.Value)
		{
			m_HoverDelay?.Dispose();
			m_HoverDelay = DelayedInvoker.InvokeInTime(delegate
			{
				m_Visibility.Value = UnitOvertipVisibility.Maximized;
			}, 0.2f);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (base.IsBinded && base.ViewModel.UnitState.IsInCombat.Value)
		{
			m_HoverDelay?.Dispose();
			m_HoverDelay = DelayedInvoker.InvokeInTime(UpdateVisibility, 1f);
		}
	}
}
