using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.View.Party.PC;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;
using Kingmaker.GameModes;
using Kingmaker.UI.MVVM.View.Overtips.CombatText;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit.OvertipSpaceShipUnit;

public class OvertipSpaceShipUnitView : BaseOvertipView<OvertipEntityUnitVM>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Parts Block")]
	[SerializeField]
	private OvertipShipHealthBlockView m_HealthBlockPCView;

	[SerializeField]
	private OvertipUnitNameView m_NameBlockPCView;

	[SerializeField]
	private OvertipCombatTextBlockView m_CombatTextBlockPCView;

	[SerializeField]
	private OvertipBarkBlockView m_BarkBlockPCView;

	[SerializeField]
	private OvertipVoidshipHealthView m_VoidshipHealthPCView;

	[Header("Combat Statuses")]
	[SerializeField]
	private OvertipHitChanceBlockView m_HitChanceBlockPCView;

	[SerializeField]
	private OvertipSpaceShipEvasionBlockView m_OvertipSpaceShipEvasionBlockView;

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

	[Header("Buffs")]
	[SerializeField]
	private UnitBuffPartPCView m_UnitBuffPartPCView;

	[SerializeField]
	private CanvasGroup m_UnitBuffsCanvasGroup;

	[SerializeField]
	private Vector2 m_UnitBuffUpperPosition;

	[SerializeField]
	private Vector2 m_UnitBuffLowerPosition;

	private Tweener m_FadeAnimator;

	private Tweener m_ScaleAnimator;

	private IDisposable m_HoverDelay;

	private ReactiveProperty<UnitOvertipVisibility> m_Visibility = new ReactiveProperty<UnitOvertipVisibility>();

	private List<Graphic> m_AdditionalRaycastBlockers = new List<Graphic>();

	private List<Graphic> m_MainRaycastBlockers = new List<Graphic>();

	protected override bool CheckVisibility => base.ViewModel.UnitState.IsVisibleForPlayer.Value;

	private bool CheckVisibleTrigger
	{
		get
		{
			if (!base.ViewModel.UnitState.IsCurrentUnitTurn.Value && !base.ViewModel.UnitState.ForceHotKeyPressed.Value && !base.ViewModel.UnitState.IsMouseOverUnit.Value && !base.ViewModel.IsBarkActive.Value)
			{
				return m_CombatTextBlockPCView.HasActiveCombatText.Value;
			}
			return true;
		}
	}

	private bool CheckCanBeVisible
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
		base.gameObject.name = base.ViewModel.UnitUIWrapper.Name + "_SpaceShipUnitOvertip";
		m_HealthBlockPCView.Initialize(m_Visibility);
		m_HealthBlockPCView.Bind(base.ViewModel.HealthBlockVM);
		m_NameBlockPCView.Bind(base.ViewModel.HitChanceBlockVM);
		m_OvertipTargetNameView.Bind(base.ViewModel.NameBlockVM);
		m_OvertipAimView.Bind(base.ViewModel.HitChanceBlockVM);
		m_CombatTextBlockPCView.Bind(base.ViewModel.CombatTextBlockVM);
		m_BarkBlockPCView.Bind(base.ViewModel.BarkBlockVM);
		m_UnitBuffPartPCView.Bind(base.ViewModel.BuffPartVM);
		AddDisposable(base.ViewModel.UnitState.ForceHotKeyPressed.Subscribe(EnableAdditionalRaycasts));
		if (!base.ViewModel.UnitState.IsEnemy.Value && base.ViewModel.UnitState.IsPlayer.Value && Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			m_VoidshipHealthPCView.Bind(base.ViewModel.VoidshipHealthVM);
		}
		AddDisposable(base.ViewModel.UnitState.IsVisibleForPlayer.CombineLatest(base.ViewModel.IsBarkActive, base.ViewModel.UnitState.IsCurrentUnitTurn, base.ViewModel.UnitState.ForceHotKeyPressed, base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.UnitState.IsAoETarget, base.ViewModel.CameraDistance, (bool isVisible, bool bark, bool current, bool hotkeyHighlight, bool hover, bool isAoE, Vector3 distance) => new { isVisible, bark, current, hotkeyHighlight, hover, isAoE, distance }).ObserveLastValueOnLateUpdate().Subscribe(_ =>
		{
			UpdateVisibility();
		}));
		AddDisposable(m_CombatTextBlockPCView.HasActiveCombatText.ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			UpdateVisibility();
		}));
		m_HitChanceBlockPCView.Or(null)?.Initialize(m_Visibility);
		m_HitChanceBlockPCView.Or(null)?.Bind(base.ViewModel.HitChanceBlockVM);
		m_OvertipSpaceShipEvasionBlockView.Bind(base.ViewModel.HitChanceBlockVM);
		AddDisposable(m_Visibility.Subscribe(DoVisibility));
		EnableMainRaycasts(enable: true);
	}

	private void DoVisibility(UnitOvertipVisibility unitOvertipVisibility)
	{
		UnitOvertipVisibilitySettings? unitOvertipVisibilitySettings = m_UnitOvertipVisibilitySettings.FirstOrDefault((UnitOvertipVisibilitySettings s) => s.UnitOvertipVisibility == unitOvertipVisibility);
		float alpha = unitOvertipVisibilitySettings.Value.Alpha;
		float scale = unitOvertipVisibilitySettings.Value.Scale;
		m_FadeAnimator?.Kill();
		m_FadeAnimator = m_InnerCanvasGroup.DOFade(alpha, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_ScaleAnimator?.Kill();
		m_FadeAnimator = m_RectTransform.DOScale(scale, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		if (unitOvertipVisibility == UnitOvertipVisibility.NotFull || unitOvertipVisibility == UnitOvertipVisibility.Full || unitOvertipVisibility == UnitOvertipVisibility.Maximized)
		{
			base.transform.SetAsLastSibling();
		}
		((RectTransform)m_UnitBuffPartPCView.transform).anchoredPosition = (base.ViewModel.HealthBlockVM.CanDie.Value ? m_UnitBuffLowerPosition : m_UnitBuffUpperPosition);
		m_UnitBuffsCanvasGroup.alpha = ((unitOvertipVisibility == UnitOvertipVisibility.Full || unitOvertipVisibility == UnitOvertipVisibility.Maximized) ? 1 : 0);
	}

	private void UpdateVisibility()
	{
		if (base.ViewModel.UnitState.IsDead.Value)
		{
			return;
		}
		if (!CheckCanBeVisible || base.ViewModel.IsCutscene)
		{
			m_Visibility.Value = UnitOvertipVisibility.Invisible;
			return;
		}
		bool flag = base.ViewModel.CameraDistance.Value.sqrMagnitude < m_FarDistance;
		if (CheckVisibleTrigger)
		{
			m_Visibility.Value = (flag ? UnitOvertipVisibility.Full : UnitOvertipVisibility.Near);
		}
		else
		{
			m_Visibility.Value = ((!flag) ? UnitOvertipVisibility.Far : UnitOvertipVisibility.Near);
		}
		m_CombatTextBlockPCView.UpdateVisualForCommon();
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
		if (base.ViewModel.UnitState.IsInCombat.Value)
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
		if (base.ViewModel.UnitState.IsInCombat.Value)
		{
			m_HoverDelay?.Dispose();
			m_HoverDelay = DelayedInvoker.InvokeInTime(UpdateVisibility, 1f);
		}
	}
}
