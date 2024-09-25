using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.View.Party.PC;
using Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;
using Kingmaker.UI.MVVM.View.Overtips.CombatText;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject;

public abstract class OvertipDestructibleObjectView : BaseOvertipView<OvertipDestructibleObjectVM>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Parts Block")]
	[SerializeField]
	private OvertipUnitHealthBlockView m_HealthBlockView;

	[SerializeField]
	private OvertipUnitNameView m_NameBlockPCView;

	[Header("Combat Statuses")]
	[SerializeField]
	private OvertipHitChanceBlockView m_HitChanceBlockPCView;

	[SerializeField]
	private OvertipCoverBlockView m_CoverBlockPCView;

	[SerializeField]
	private OvertipTargetDefensesView m_OvertipTargetDefensesPCView;

	[SerializeField]
	private OvertipNameView m_OvertipTargetNameView;

	[SerializeField]
	private OvertipAimView m_OvertipAimView;

	[Header("Common Block")]
	[SerializeField]
	private RectTransform m_RectTransform;

	[SerializeField]
	private CanvasGroup m_InnerCanvasGroup;

	[SerializeField]
	private List<UnitOvertipVisibilitySettings> m_UnitOvertipVisibilitySettings;

	[SerializeField]
	private float m_FarDistance = 120f;

	[Header("Bark and Combat text")]
	[SerializeField]
	private OvertipCombatTextBlockView m_CombatTextBlockPCView;

	[SerializeField]
	private OvertipBarkBlockView m_BarkBlockPCView;

	[Header("Buffs")]
	[SerializeField]
	private UnitBuffPartPCView m_UnitBuffPartPCView;

	[SerializeField]
	private CanvasGroup m_UnitBuffsCanvasGroup;

	[SerializeField]
	private Vector2 m_UnitBuffUpperPosition;

	[SerializeField]
	private Vector2 m_UnitBuffLowerPosition;

	[SerializeField]
	private float m_BuffsOvertipPositionYCorrection = 30f;

	private readonly ReactiveProperty<UnitOvertipVisibility> m_Visibility = new ReactiveProperty<UnitOvertipVisibility>();

	private List<Graphic> m_AdditionalRaycastBlockers = new List<Graphic>();

	private List<Graphic> m_MainRaycastBlockers = new List<Graphic>();

	private Tweener m_FadeAnimator;

	private Tweener m_ScaleAnimator;

	private IDisposable m_HoverDelay;

	protected override bool CheckVisibility
	{
		get
		{
			if (base.ViewModel.UnitState.IsTBM.Value && base.ViewModel.IsVisibleForPlayer.Value && base.ViewModel.UnitState.IsInCombat.Value && !base.ViewModel.MapObjectEntity.Suppressed)
			{
				return !base.ViewModel.IsCutscene;
			}
			return false;
		}
	}

	private bool CheckVisibleTrigger
	{
		get
		{
			if (!base.ViewModel.UnitState.IsMouseOverUnit.Value)
			{
				return m_CombatTextBlockPCView.HasActiveCombatText.Value;
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
		base.BindViewImplementation();
		base.gameObject.name = base.ViewModel.MapObjectEntity.View.gameObject.name + "_OvertipDestructibleObject";
		m_HealthBlockView.Initialize(m_Visibility);
		m_HealthBlockView.Bind(base.ViewModel.HealthBlockVM);
		m_NameBlockPCView.Bind(base.ViewModel.HitChanceBlockVM);
		m_OvertipTargetNameView.Bind(base.ViewModel.NameBlockVM);
		m_HitChanceBlockPCView.Initialize(m_Visibility);
		m_HitChanceBlockPCView.Bind(base.ViewModel.HitChanceBlockVM);
		m_OvertipAimView.Bind(base.ViewModel.HitChanceBlockVM);
		m_CombatTextBlockPCView.Bind(base.ViewModel.CombatTextBlockVM);
		m_BarkBlockPCView.Bind(base.ViewModel.BarkBlockVM);
		m_UnitBuffPartPCView.Bind(base.ViewModel.BuffPartVM);
		m_CoverBlockPCView.Bind(base.ViewModel.CoverBlockVM);
		m_OvertipTargetDefensesPCView.Bind(base.ViewModel.HitChanceBlockVM);
		AddDisposable(base.ViewModel.UnitState.ForceHotKeyPressed.Subscribe(EnableAdditionalRaycasts));
		AddDisposable(base.ViewModel.UnitState.IsVisibleForPlayer.CombineLatest(base.ViewModel.UnitState.IsCurrentUnitTurn, base.ViewModel.UnitState.ForceHotKeyPressed, base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.UnitState.IsAoETarget, base.ViewModel.CameraDistance, (bool isVisible, bool current, bool hotkeyHighlight, bool hover, bool isAoE, Vector3 distance) => new { isVisible, current, hotkeyHighlight, hover, isAoE, distance }).ObserveLastValueOnLateUpdate().Subscribe(_ =>
		{
			UpdateVisibility();
		}));
		AddDisposable(m_CombatTextBlockPCView.HasActiveCombatText.CombineLatest(base.ViewModel.UnitState.Ability, (bool hasActiveCombatText, AbilityData ability) => new { hasActiveCombatText, ability }).ObserveLastValueOnLateUpdate().Subscribe(_ =>
		{
			UpdateVisibility();
		}));
		AddDisposable(m_Visibility.Subscribe(DoVisibility));
	}

	private void UpdateVisibility()
	{
		if (!CheckVisibility)
		{
			m_Visibility.Value = UnitOvertipVisibility.Invisible;
			return;
		}
		bool flag = base.ViewModel.CameraDistance.Value.sqrMagnitude < m_FarDistance;
		if (base.ViewModel.UnitState.IsDestructibleNotCover.Value)
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
		if (base.ViewModel.UnitState.IsCover.Value)
		{
			if (CheckVisibleTrigger)
			{
				m_Visibility.Value = (flag ? UnitOvertipVisibility.NotFull : UnitOvertipVisibility.Near);
			}
			else if (base.ViewModel.UnitState.IsAoETarget.Value)
			{
				m_Visibility.Value = UnitOvertipVisibility.NotFull;
			}
			else
			{
				m_Visibility.Value = UnitOvertipVisibility.Invisible;
			}
		}
		m_CombatTextBlockPCView.UpdateVisualForCommon();
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
		((RectTransform)m_UnitBuffPartPCView.transform).anchoredPosition = (base.ViewModel.HealthBlockVM.CanDie.Value ? m_UnitBuffLowerPosition : m_UnitBuffUpperPosition);
		m_UnitBuffsCanvasGroup.alpha = ((unitOvertipVisibility == UnitOvertipVisibility.Full || unitOvertipVisibility == UnitOvertipVisibility.Maximized) ? 1 : 0);
		PositionCorrectionFromView = (((unitOvertipVisibility == UnitOvertipVisibility.Full || unitOvertipVisibility == UnitOvertipVisibility.Maximized) && m_UnitBuffPartPCView.HasBuffs) ? Vector2.zero : new Vector2(0f, 0f - m_BuffsOvertipPositionYCorrection));
	}

	protected override void DestroyViewImplementation()
	{
		m_FadeAnimator?.Kill();
		m_FadeAnimator = null;
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = null;
		TryDestroyViewImplementation();
	}

	private void TryDestroyViewImplementation()
	{
		if (m_CombatTextBlockPCView.HasActiveCombatText.Value)
		{
			DelayedInvoker.InvokeInFrames(TryDestroyViewImplementation, 5);
			return;
		}
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
