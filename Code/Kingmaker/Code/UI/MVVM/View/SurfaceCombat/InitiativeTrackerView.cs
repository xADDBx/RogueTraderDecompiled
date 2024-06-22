using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.TimeSurvival;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.SurfaceCombat;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SurfaceCombat;

public abstract class InitiativeTrackerView : ViewBase<InitiativeTrackerVM>, IGameModeHandler, ISubscriber, IFullScreenUIHandler
{
	protected class TurnVirtualUnitData : ITurnVirtualItemData
	{
		public Vector2 VirtualSize { get; set; }

		public Vector2 VirtualPosition { get; set; }

		public IViewModel ViewModel { get; set; }

		public ITurnVirtualItemView BoundView { get; set; }

		public void SetBoundView(ITurnVirtualItemView view)
		{
			BoundView = view;
		}

		public void SetViewParameters(Vector2 virtualPosition, Vector2 size)
		{
			VirtualPosition = virtualPosition;
			VirtualSize = size;
		}
	}

	[Header("Tracker Container")]
	[SerializeField]
	[UsedImplicitly]
	protected RectTransform TrackerContainer;

	[Header("List items prefabs")]
	[SerializeField]
	[UsedImplicitly]
	protected RectTransform OrderContainer;

	[SerializeField]
	[UsedImplicitly]
	protected TurnVirtualListController VirtualList;

	[SerializeField]
	[UsedImplicitly]
	protected SurfaceCombatUnitOrderView CombatUnitPrefab;

	[UsedImplicitly]
	private bool m_IsInit;

	protected List<ITurnVirtualItemData> VirtualEntries = new List<ITurnVirtualItemData>();

	private bool m_WantUpdateView;

	[Header("Animator")]
	[SerializeField]
	[UsedImplicitly]
	private FadeAnimator m_OwnAnimation;

	private bool IsShowed;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_OwnAnimation.Initialize();
			base.gameObject.SetActive(value: false);
			VirtualList.Initialize(CombatUnitPrefab);
			OnInitialize();
			m_IsInit = true;
		}
	}

	protected abstract void OnInitialize();

	protected override void BindViewImplementation()
	{
		Initialize();
		base.gameObject.SetActive(value: true);
		PrepareInitiativeTracker();
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(base.ViewModel.RoundCounter.Subscribe(RoundChanged));
		AddDisposable(base.ViewModel.HoveredUnit.Subscribe(delegate
		{
			OnUnitHovered();
		}));
		AddDisposable(base.ViewModel.CurrentUnit.Subscribe(delegate(InitiativeTrackerUnitVM value)
		{
			OnCurrentUnitChanged(value?.UnitAsBaseUnitEntity);
		}));
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.UnitsUpdated.ObserveLastValueOnLateUpdate(), delegate
		{
			m_WantUpdateView = true;
		}));
		AddDisposable(UniRxExtensionMethods.Subscribe(MainThreadDispatcher.InfrequentUpdateAsObservable(), delegate
		{
			if (m_WantUpdateView && !VirtualList.IsAnimating)
			{
				m_WantUpdateView = false;
				UpdateUnits();
			}
		}));
		DelayedInvoker.InvokeInFrames(delegate
		{
			UpdateUnits();
		}, 1);
		m_WantUpdateView = true;
	}

	private void OnCurrentUnitChanged(BaseUnitEntity unit)
	{
		if (unit != null && unit.IsInPlayerParty && UINetUtility.InLobbyAndPlaying && unit.IsMyNetRole())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.TurnBasedTexts.YouTurn, addToLog: false);
			});
		}
	}

	public void RoundChanged(int round)
	{
		if (round <= 1)
		{
			return;
		}
		UISounds.Instance.Sounds.Combat.NewRound.Play();
		TimeSurvival component = Game.Instance.CurrentlyLoadedArea.GetComponent<TimeSurvival>();
		if (component != null && !component.UnlimitedTime)
		{
			string warningText = component.RoundsLeft.ToString();
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(warningText, addToLog: false, WarningNotificationFormat.Counter);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning($"{UIStrings.Instance.TurnBasedTexts.Round.Text} {round}", addToLog: false);
			});
		}
	}

	protected abstract void PrepareInitiativeTracker();

	protected abstract void OnUnitHovered();

	protected abstract void UpdateUnits();

	protected override void DestroyViewImplementation()
	{
		VirtualList.CleanList();
		m_WantUpdateView = false;
		Hide();
		Owlcat.Runtime.UI.Utility.UILog.ViewUnbinded("InitiativeTrackerView");
	}

	protected virtual void Show()
	{
		if (base.IsBinded && !IsShowed)
		{
			base.gameObject.SetActive(value: true);
			IsShowed = true;
			m_OwnAnimation.AppearAnimation();
			UISounds.Instance.Sounds.InitiativeTracker.InitiativeTrackerShow.Play();
		}
	}

	protected virtual void Hide()
	{
		if (IsShowed)
		{
			UISounds.Instance.Sounds.InitiativeTracker.InitiativeTrackerHide.Play();
			m_OwnAnimation.DisappearAnimation(delegate
			{
				base.gameObject.SetActive(value: false);
				IsShowed = false;
			});
		}
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (fullScreenUIType == FullScreenUIType.EscapeMenu)
		{
			if (!state && !Game.Instance.IsModeActive(GameModeType.Cutscene) && !Game.Instance.IsModeActive(GameModeType.Dialog))
			{
				Show();
			}
			else
			{
				Hide();
			}
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Dialog || gameMode == GameModeType.Cutscene)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
