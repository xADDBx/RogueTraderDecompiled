using System;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Common.UnitState;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UnitLogic.Squads;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public abstract class SurfaceCombatUnitOrderView : SurfaceCombatUnitView<InitiativeTrackerUnitVM>, ITurnVirtualItemView, IUnitHighlightUIHandler, ISubscriber, IInteractionHighlightUIHandler
{
	[SerializeField]
	private ColorAnimator[] m_ColorAnimator;

	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	public Vector2 SizeWithPortrait;

	[SerializeField]
	public Vector2 SizeWithSquad;

	[SerializeField]
	public Vector2 SizeRound;

	[Header("HitChances")]
	public bool HasHitChances;

	[ConditionalShow("HasNameView")]
	[SerializeField]
	protected InitiativeTrackerUnitHitChanceView InitiativeTrackerUnitHitChanceView;

	[Header("Unit And Round")]
	[SerializeField]
	private bool m_HasRoundBlock = true;

	[ConditionalShow("m_HasRoundBlock")]
	[SerializeField]
	private InitiativeTrackerEndOfRound m_EndOfRound;

	[ConditionalShow("m_HasRoundBlock")]
	[SerializeField]
	private GameObject m_UnitContainer;

	[Header("NameZone")]
	[SerializeField]
	private FadeAnimator m_HPLabelAnimator;

	[SerializeField]
	private FadeAnimator m_NameZoneAnimator;

	private ITurnVirtualItemData m_BoundData;

	public bool WillBeDestroyedExternal;

	private bool m_IsInitialized;

	[Header("Squad")]
	[SerializeField]
	private SurfaceCombatInitiativeOrderSquadUnitView m_SurfaceCombatInitiativeOrderSquadUnitViewPrefab;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private OwlcatMultiButton m_SquadButton;

	[SerializeField]
	private GameObject m_SquadNumContainer;

	[SerializeField]
	private TextMeshProUGUI m_SquadNumber;

	public float m_AnimationTime = 0.3f;

	public float m_Scale = 1.6f;

	private bool m_DelayViewState;

	private readonly BoolReactiveProperty m_IsTargetSelected = new BoolReactiveProperty();

	private readonly BoolReactiveProperty m_IsNameZoneVisible = new BoolReactiveProperty();

	public bool HasSquad
	{
		get
		{
			if (base.ViewModel.IsSquadLeader.Value)
			{
				return base.ViewModel.HasAliveUnitsInSquad.Value;
			}
			return false;
		}
	}

	public MonoBehaviour View => this;

	public OwlcatSelectable Selectable => null;

	public RectTransform RectTransform => (RectTransform)base.transform;

	public CanvasGroup CanvasGroup => m_CanvasGroup;

	protected float CalculatedAnimationTime => m_AnimationTime / m_AnimationTimeDevider;

	private float m_AnimationTimeDevider => Game.Instance.Player.UISettings.TimeScaleAverage;

	public ITurnVirtualItemData BoundData => m_BoundData;

	public bool IsEnemy => base.ViewModel.IsEnemy.Value;

	public void TryInitialize()
	{
		if (!m_IsInitialized)
		{
			m_NameZoneAnimator.Initialize();
			m_NameZoneAnimator.DisappearAnimation();
			m_IsInitialized = true;
		}
	}

	protected override void InternalBind()
	{
		if (base.ViewModel.HasUnit)
		{
			base.InternalBind();
			TryInitialize();
			WillBeDestroyedExternal = true;
			if (m_HasRoundBlock)
			{
				m_EndOfRound.Or(null)?.gameObject.SetActive(value: false);
				m_UnitContainer.Or(null)?.gameObject.SetActive(value: true);
			}
			AddDisposable(base.ViewModel.UnitState.IsAoETarget.CombineLatest(base.ViewModel.UnitState.IsMouseOverUnit, base.ViewModel.IsTargetSelection, base.ViewModel.IsCurrent, base.ViewModel.UnitState.IsPingUnit, (bool _, bool _, bool _, bool _, bool _) => true).ObserveLastValueOnLateUpdate().Subscribe(delegate
			{
				UpdateHighlightedState();
			}));
			AddDisposable(m_IsNameZoneVisible.Delay(TimeSpan.FromSeconds(0.05)).ObserveLastValueOnLateUpdate().Subscribe(SetNameVisible));
			AddDisposable(m_IsTargetSelected.ObserveLastValueOnLateUpdate().Subscribe(Button.SetFocus));
			AddDisposable(Button.OnPointerEnterAsObservable().Subscribe(delegate
			{
				SetSelected(value: true);
				base.ViewModel.UnitAsBaseUnitEntity?.View.HandleHoverChange(isHover: true);
			}));
			AddDisposable(Button.OnPointerExitAsObservable().Subscribe(delegate
			{
				SetSelected(value: false);
				base.ViewModel.UnitAsBaseUnitEntity?.View.HandleHoverChange(isHover: false);
			}));
			AddDisposable(Button.OnPointerClickAsObservable().Subscribe(delegate
			{
				Game.Instance.CameraController?.Follower?.ScrollTo(base.ViewModel.Unit);
			}));
			AddDisposable(ObservableExtensions.Subscribe(Button.OnRightClickAsObservable(), delegate
			{
				InvokeUnitInspect();
			}));
			if (HasHitChances)
			{
				InitiativeTrackerUnitHitChanceView.Bind(base.ViewModel.OvertipHitChanceBlockVM);
			}
		}
		else if (m_HasRoundBlock)
		{
			m_UnitContainer?.gameObject.SetActive(value: false);
			m_EndOfRound?.gameObject.SetActive(value: true);
			base.transform.SetAsFirstSibling();
			if (m_EndOfRound != null)
			{
				AddDisposable(base.ViewModel.Round.Subscribe(m_EndOfRound.SetRound));
			}
		}
		m_SquadButton.gameObject.SetActive(HasSquad);
		AddDisposable(m_SquadButton.OnPointerClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.HandleShow();
		}));
		m_SquadNumContainer.SetActive(base.ViewModel.HasAliveUnitsInSquad.Value && base.ViewModel.IsSquadLeader.Value);
		m_SquadNumber.text = base.ViewModel.SquadCount.Value.ToString();
		SetupOwnSize();
		AddDisposable(EventBus.Subscribe(this));
	}

	public void InvokeUnitInspect()
	{
		EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
		{
			h.HandleUnitRightClick(base.ViewModel.UnitAsBaseUnitEntity);
		});
	}

	private void SetupOwnSize()
	{
		((RectTransform)base.transform).sizeDelta = ((!base.ViewModel.HasUnit) ? SizeRound : ((base.ViewModel.Squad != null) ? SizeWithSquad : SizeWithPortrait));
	}

	protected override void DestroyViewImplementation()
	{
		if (!WillBeDestroyedExternal)
		{
			base.ViewModel.InvokeUnitViewHighlight(state: false);
			if (m_HasRoundBlock && !base.WillBeReused)
			{
				m_EndOfRound?.gameObject.SetActive(value: false);
				m_UnitContainer?.gameObject.SetActive(value: false);
			}
			base.DestroyViewImplementation();
		}
		else
		{
			base.DestroyViewImplementation();
		}
		m_BoundData = null;
		m_IsInitialized = false;
	}

	private void OnDestroy()
	{
		EventBus.Unsubscribe(this);
	}

	public void SetSelected(bool value)
	{
		if (base.ViewModel?.Unit != null && base.ViewModel.UnitAsBaseUnitEntity != null)
		{
			if (base.ViewModel.UnitAsBaseUnitEntity.View.MouseHighlighted != value)
			{
				base.ViewModel.SetMouseHighlighted(value);
			}
			if (!value)
			{
				UnitBuffPartPCView.HideAdditionalBuffs();
			}
		}
	}

	public bool IsValid()
	{
		if (base.gameObject.activeInHierarchy)
		{
			return base.ViewModel?.Unit != null;
		}
		return false;
	}

	public Sequence GetHideAnimation(Action completeAction)
	{
		return GetHideAnimationInternal(completeAction);
	}

	protected abstract Sequence GetHideAnimationInternal(Action completeAction);

	public Sequence GetMoveAnimation(Action completeAction, Vector2 targetPosition)
	{
		return GetMoveAnimationInternal(completeAction, targetPosition);
	}

	protected abstract Sequence GetMoveAnimationInternal(Action completeAction, Vector2 targetPosition);

	public Sequence GetShowAnimation(Action completeAction, Vector2 targetPosition)
	{
		return GetShowAnimationInternal(completeAction, targetPosition);
	}

	public void SetAnchoredPosition(Vector2 position)
	{
		RectTransform.anchoredPosition = position;
	}

	protected abstract Sequence GetShowAnimationInternal(Action completeAction, Vector2 targetPosition);

	public void ViewBind(ITurnVirtualItemData data)
	{
		SetAlphaAndScale();
		m_BoundData = data;
		Bind((InitiativeTrackerUnitVM)(data?.ViewModel));
		WillBeDestroyedExternal = true;
	}

	protected virtual void SetAlphaAndScale()
	{
		CanvasGroup.alpha = 0f;
		RectTransform.localScale = new Vector3(1f, 1f, 1f);
	}

	public void DestroyViewItem()
	{
		if (m_BoundData != null)
		{
			m_BoundData.BoundView = null;
		}
		m_BoundData = null;
		DestroyViewImplementation();
	}

	public void OnPointerClick()
	{
		if (base.ViewModel?.Unit != null && !CameraRig.Instance.IsScrollingByRoutine)
		{
			CameraRig.Instance.ScrollTo(base.ViewModel.Unit.Position);
		}
	}

	public void HandleHighlightChange(AbstractUnitEntityView unit)
	{
		if (unit != base.ViewModel?.Unit?.View)
		{
			DelayedInvoker.InvokeInTime(delegate
			{
				TryHandleHighlightChangeForSquadUnit(unit);
			}, 0.1f);
		}
		else
		{
			UpdateHighlightedState();
		}
	}

	public virtual void HandleHighlightChange(bool isOn)
	{
		UpdateHighlightedState();
	}

	public void UpdateHighlightedState()
	{
		if (base.ViewModel.Unit?.View == null)
		{
			return;
		}
		m_IsTargetSelected.Value = base.ViewModel.UnitState.IsAoETarget.Value || (base.ViewModel.IsTargetSelection.Value && base.ViewModel.UnitState.IsMouseOverUnit.Value);
		BoolReactiveProperty isNameZoneVisible = m_IsNameZoneVisible;
		int value;
		if (base.ViewModel.UnitAsBaseUnitEntity == null || !base.ViewModel.UnitAsBaseUnitEntity.View.IsHighlighted)
		{
			InteractionHighlightController instance = InteractionHighlightController.Instance;
			if ((instance == null || !instance.IsHighlighting) && !base.ViewModel.UnitState.IsAoETarget.Value && !base.ViewModel.UnitState.IsMouseOverUnit.Value && !base.ViewModel.IsCurrent.Value && !UnitBuffPartPCView.IsHovered.Value)
			{
				value = (base.ViewModel.UnitState.IsPingUnit.Value ? 1 : 0);
				goto IL_0116;
			}
		}
		value = 1;
		goto IL_0116;
		IL_0116:
		isNameZoneVisible.Value = (byte)value != 0;
	}

	public void TryHandleHighlightChangeForSquadUnit(AbstractUnitEntityView unit)
	{
		if (base.ViewModel == null || !base.ViewModel.IsSquadLeader.Value || base.ViewModel.NeedToShow.Value)
		{
			return;
		}
		UnitSquad unitSquad = base.ViewModel?.Squad;
		if (unitSquad == null)
		{
			return;
		}
		foreach (UnitReference unit2 in unitSquad.Units)
		{
			BaseUnitEntity baseUnitEntity = unit2.ToBaseUnitEntity();
			if (!(baseUnitEntity.View != unit))
			{
				UpdateHighlightedStateForSquadUnit(baseUnitEntity);
				break;
			}
		}
	}

	private void UpdateHighlightedStateForSquadUnit(BaseUnitEntity unit)
	{
		UnitState orCreateUnitState = UnitStatesHolderVM.Instance.GetOrCreateUnitState(unit);
		m_IsTargetSelected.Value = orCreateUnitState.IsAoETarget.Value || (base.ViewModel.IsTargetSelection.Value && orCreateUnitState.IsMouseOverUnit.Value);
		m_IsNameZoneVisible.Value = unit.View.IsHighlighted || orCreateUnitState.IsAoETarget.Value || orCreateUnitState.IsMouseOverUnit.Value || orCreateUnitState.IsPingUnit.Value;
	}

	public void SetNameVisible(bool state)
	{
		if (base.IsBinded && m_IsInitialized)
		{
			if (state)
			{
				m_NameZoneAnimator.Or(null)?.AppearAnimation();
				m_HPLabelAnimator.Or(null)?.AppearAnimation();
			}
			else
			{
				m_NameZoneAnimator.Or(null)?.DisappearAnimation();
				m_HPLabelAnimator.Or(null)?.DisappearAnimation();
			}
		}
	}
}
