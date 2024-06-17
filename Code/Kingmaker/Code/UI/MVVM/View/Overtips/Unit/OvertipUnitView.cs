using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.View.Overtips.Unit.UnitOvertipParts;
using Kingmaker.Code.UI.MVVM.View.Party.PC;
using Kingmaker.Code.UI.MVVM.VM.Overtips.Unit;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.MVVM.View.Overtips.CombatText;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.Unit;

public class OvertipUnitView : BaseOvertipView<OvertipEntityUnitVM>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Parts Block")]
	[SerializeField]
	private OvertipUnitHealthBlockView m_HealthBlockView;

	[SerializeField]
	private OvertipUnitNameView m_NameBlockPCView;

	[Header("Death")]
	[SerializeField]
	private FadeAnimator m_DeathAnimator;

	[SerializeField]
	private float m_DeathDelay = 0.2f;

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
	private float m_StandardOvertipPositionYCorrection = 10f;

	[SerializeField]
	private float m_BuffsOvertipPositionYCorrection = 30f;

	private Tweener m_FadeAnimator;

	private Tweener m_ScaleAnimator;

	private Tweener m_PositionAnimator;

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
			if (!base.ViewModel.UnitState.ForceHotKeyPressed.Value && !base.ViewModel.UnitState.IsPingUnit.Value && !base.ViewModel.IsBarkActive.Value && !base.ViewModel.UnitState.HoverSelfTargetAbility.Value)
			{
				if ((base.ViewModel.UnitState.IsMouseOverUnit.Value || base.ViewModel.UnitState.IsCurrentUnitTurn.Value) && !m_CombatTextBlockPCView.HasActiveCombatText.Value)
				{
					return !base.ViewModel.UnitState.IsActing.Value;
				}
				return false;
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
		m_InnerCanvasGroup.alpha = 0f;
		base.gameObject.name = base.ViewModel.UnitUIWrapper.Name + "_UnitOvertip";
		m_HealthBlockView.Initialize(m_Visibility);
		m_HealthBlockView.Bind(base.ViewModel.HealthBlockVM);
		m_NameBlockPCView.Bind(base.ViewModel.HitChanceBlockVM);
		m_OvertipTargetNameView.Bind(base.ViewModel.NameBlockVM);
		m_OvertipAimView.Bind(base.ViewModel.HitChanceBlockVM);
		m_CombatTextBlockPCView.Bind(base.ViewModel.CombatTextBlockVM);
		m_BarkBlockPCView.Bind(base.ViewModel.BarkBlockVM);
		m_UnitBuffPartPCView.Bind(base.ViewModel.BuffPartVM);
		m_UnitBuffPartPCView.gameObject.SetActive(value: true);
		AddDisposable(base.ViewModel.UnitState.ForceHotKeyPressed.Subscribe(EnableAdditionalRaycasts));
		AddDisposable(base.ViewModel.UnitState.IsDeadOrUnconsciousIsDead.Where((bool _) => _).Subscribe(delegate
		{
			if (!base.ViewModel.UnitState.IsPlayer.Value)
			{
				m_UnitBuffPartPCView.gameObject.SetActive(value: false);
			}
		}));
		AddDisposable(base.ViewModel.UnitState.IsVisibleForPlayer.CombineLatest(base.ViewModel.IsBarkActive, base.ViewModel.UnitState.IsCurrentUnitTurn, base.ViewModel.UnitState.ForceHotKeyPressed, base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.UnitState.IsAoETarget, base.ViewModel.CameraDistance, (bool _, bool _, bool _, bool _, bool _, bool _, Vector3 _) => true).ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			UpdateVisibility();
		}));
		AddDisposable(m_CombatTextBlockPCView.HasActiveCombatText.CombineLatest(base.ViewModel.UnitState.Ability, base.ViewModel.UnitState.IsActing, base.ViewModel.UnitState.IsPingUnit, (bool _, AbilityData _, bool _, bool _) => true).ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			UpdateVisibility();
		}));
		base.ViewModel.SetDeathDelay(m_DeathDelay + m_DeathAnimator.AppearAnimationTime + m_DeathAnimator.DisappearAnimationTime);
		AddDisposable((from v in base.ViewModel.UnitState.IsDead.Skip(1)
			where v
			select v).Subscribe(delegate
		{
			DoDeath();
		}));
		m_CoverBlockPCView.Or(null)?.Bind(base.ViewModel.CoverBlockVM);
		m_HitChanceBlockPCView.Or(null)?.Initialize(m_Visibility);
		m_HitChanceBlockPCView.Or(null)?.Bind(base.ViewModel.HitChanceBlockVM);
		m_OvertipTargetDefensesPCView.Bind(base.ViewModel.HitChanceBlockVM);
		AddDisposable(m_Visibility.Subscribe(DoVisibility));
		AddDisposable(base.ViewModel.UnitState.ForceHotKeyPressed.CombineLatest(m_Visibility, (bool tab, UnitOvertipVisibility vis) => new { tab, vis }).Subscribe(value =>
		{
			m_UnitBuffsCanvasGroup.blocksRaycasts = value.tab || value.vis == UnitOvertipVisibility.Maximized;
		}));
		EnableMainRaycasts(enable: true);
	}

	private void DoDeath()
	{
		UISounds.Instance.Sounds.Combat.UnitDeath.Play();
		if (base.ViewModel.IsCutscene || base.ViewModel.IsInDialog)
		{
			if (m_DeathAnimator.CanvasGroup != null)
			{
				m_DeathAnimator.CanvasGroup.alpha = 0f;
			}
			return;
		}
		m_DeathAnimator.AppearAnimation(delegate
		{
			DelayedInvoker.InvokeInTime(delegate
			{
				m_DeathAnimator.DisappearAnimation();
			}, m_DeathDelay);
		});
	}

	private void DoVisibility(UnitOvertipVisibility unitOvertipVisibility)
	{
		UnitOvertipVisibilitySettings? unitOvertipVisibilitySettings = m_UnitOvertipVisibilitySettings.FirstOrDefault((UnitOvertipVisibilitySettings s) => s.UnitOvertipVisibility == unitOvertipVisibility);
		float alpha = unitOvertipVisibilitySettings.Value.Alpha;
		float scale = unitOvertipVisibilitySettings.Value.Scale;
		float yPosition = unitOvertipVisibilitySettings.Value.YPosition;
		m_FadeAnimator?.Kill();
		m_FadeAnimator = m_InnerCanvasGroup.DOFade(alpha, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true);
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = m_RectTransform.DOScale(scale, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true)
			.OnUpdate(m_CombatTextBlockPCView.UpdateVisualForCommon)
			.OnComplete(m_CombatTextBlockPCView.UpdateVisualForCommon);
		m_PositionAnimator?.Kill();
		m_PositionAnimator = m_RectTransform.DOAnchorPosY(yPosition, 0.2f).SetUpdate(isIndependentUpdate: true).SetAutoKill(autoKillOnCompletion: true)
			.OnUpdate(m_CombatTextBlockPCView.UpdateVisualForCommon);
		if (unitOvertipVisibility == UnitOvertipVisibility.NotFull || unitOvertipVisibility == UnitOvertipVisibility.Full || unitOvertipVisibility == UnitOvertipVisibility.Maximized)
		{
			base.transform.SetAsLastSibling();
		}
		((RectTransform)m_UnitBuffPartPCView.transform).anchoredPosition = (base.ViewModel.HealthBlockVM.CanDie.Value ? m_UnitBuffLowerPosition : m_UnitBuffUpperPosition);
		bool flag = unitOvertipVisibility == UnitOvertipVisibility.Full || unitOvertipVisibility == UnitOvertipVisibility.Maximized;
		m_UnitBuffsCanvasGroup.alpha = (flag ? 1 : 0);
		PositionCorrectionFromView = ((flag && m_UnitBuffPartPCView.HasBuffs) ? new Vector2(0f, 0f - m_StandardOvertipPositionYCorrection) : new Vector2(0f, 0f - m_BuffsOvertipPositionYCorrection));
		if (unitOvertipVisibility != UnitOvertipVisibility.Maximized)
		{
			m_UnitBuffPartPCView.HideAdditionalBuffs();
		}
	}

	private void UpdateVisibility()
	{
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
				m_Visibility.Value = ((flag || base.ViewModel.UnitState.IsMouseOverUnit.Value || base.ViewModel.UnitState.HoverSelfTargetAbility.Value) ? UnitOvertipVisibility.Full : UnitOvertipVisibility.Near);
			}
			else if (base.ViewModel.UnitState.IsAoETarget.Value || (base.ViewModel.UnitState.IsCurrentUnitTurn.Value && !base.ViewModel.UnitState.IsActing.Value))
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
		m_CombatTextBlockPCView.UpdateVisualForCommon();
	}

	protected override void DestroyViewImplementation()
	{
		m_HoverDelay?.Dispose();
		m_FadeAnimator?.Kill();
		m_FadeAnimator = null;
		m_ScaleAnimator?.Kill();
		m_ScaleAnimator = null;
		m_PositionAnimator?.Kill();
		m_PositionAnimator = null;
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
