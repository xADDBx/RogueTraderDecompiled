using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.DollRoom;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.VM.MessageBox;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.Models;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases;
using Kingmaker.UI.MVVM.View.CharGen.Common.Portrait;
using Kingmaker.UI.MVVM.View.Pantograph;
using Kingmaker.UI.MVVM.VM.CharGen;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common;

public class CharGenView : ViewBase<CharGenVM>, IFullScreenUIHandler, ISubscriber, ICharGenChangePhaseHandler, IInitializable
{
	public static readonly string InputLayerContextName = "Chargen";

	[SerializeField]
	protected CanvasGroup m_CanvasGroup;

	[SerializeField]
	protected CharGenRoadmapMenuView RoadmapMenuView;

	[SerializeField]
	protected CharGenPhaseDetailedViewsFactory DetailedViewsFactory;

	[Header("Portrait")]
	[SerializeField]
	private CharGenPortraitView m_PortraitView;

	[SerializeField]
	private CharGenPortraitView m_PortraitFullView;

	[Header("Pantograph")]
	[SerializeField]
	private PantographView m_PantographView;

	[SerializeField]
	private RectTransform m_PantographPosition;

	[Header("Doll")]
	[SerializeField]
	protected DollRoomTargetController m_CharacterController;

	[SerializeField]
	private FadeAnimator m_CharacterDollTexture;

	[SerializeField]
	protected DollRoomTargetController m_ShipController;

	[SerializeField]
	private FadeAnimator m_ShipDollTexture;

	[SerializeField]
	private RectTransform m_DollTransform;

	[SerializeField]
	private DollPosition[] m_DollPositions;

	[SerializeField]
	private PaperHints m_PaperHints;

	protected InputLayer InputLayer;

	protected GridConsoleNavigationBehaviour Navigation;

	private DollRoomBase m_ActiveRoom;

	protected ICharGenPhaseDetailedView SelectedDetailView;

	protected readonly ReactiveProperty<bool> CanGoBack = new ReactiveProperty<bool>();

	protected readonly ReactiveProperty<bool> CanGoNext = new ReactiveProperty<bool>();

	private bool m_DeviceBackWasOpenedBefore;

	private CharGenDollRoom CharacterRoom => UIDollRooms.Instance.Or(null)?.CharGenDollRoom;

	private CharGenShipDollRoom ShipRoom => UIDollRooms.Instance.Or(null).CharGenShipDollRoom;

	protected bool CurrentPhaseIsFirst => base.ViewModel.CurrentPhaseVM.Value == base.ViewModel.PhasesCollection.First();

	protected bool CurrentPhaseIsLast => base.ViewModel?.CurrentPhaseVM?.Value == base.ViewModel?.PhasesCollection?.Last();

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
		RoadmapMenuView.Initialize();
		DetailedViewsFactory.Initialize();
		DetailedViewsFactory.SetPaperHints(m_PaperHints);
		m_PortraitView.Initialize(HandleSmallPortraitHover);
		m_PortraitFullView.Initialize();
		m_CharacterDollTexture.Initialize();
		m_ShipDollTexture.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		CharacterRoom.Initialize(m_CharacterController);
		ShipRoom.Initialize(m_ShipController);
		RoadmapMenuView.Bind(base.ViewModel.PhasesSelectionGroupRadioVM);
		AddDisposable(base.ViewModel.CurrentPhaseVM.Subscribe(CurrentPhaseChanged));
		AddDisposable(base.ViewModel.PortraitVM.Subscribe(m_PortraitView.Bind));
		AddDisposable(base.ViewModel.CharGenContext.CurrentUnit.Subscribe(delegate
		{
			SetDoll();
		}));
		AddDisposable(base.ViewModel.CurrentPhaseIsCompleted.Subscribe(delegate(bool isCompleted)
		{
			CanGoNext.Value = isCompleted || base.ViewModel.CurrentPhaseCanInterrupt;
		}));
		AddDisposable(base.ViewModel.CharGenContext.IsCustomCharacter.Subscribe(RoadmapMenuView.SetBackgroundFrameState));
		AddDisposable(m_PantographView);
		if (m_PantographPosition != null)
		{
			m_PantographView.transform.position = m_PantographPosition.position;
		}
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Chargen);
		});
		AddDisposable(EventBus.Subscribe(this));
		CreateNavigation();
		AddDisposable(base.ViewModel.CheckCoopControls.Subscribe(delegate
		{
			DetailedViewsFactory.SetPaperHints(m_PaperHints);
		}));
	}

	protected override void DestroyViewImplementation()
	{
		RoadmapMenuView.KillSelectorTween();
		RoadmapMenuView.ShutUpSelector();
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
		m_DeviceBackWasOpenedBefore = false;
		base.gameObject.SetActive(value: false);
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
		HideRooms();
		DestroyInputLayer();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Chargen);
		});
	}

	private void HandleSmallPortraitHover(bool hovered)
	{
		SetFullPortraitVisible(hovered);
	}

	protected void SetFullPortraitVisible(bool visible)
	{
		if (visible)
		{
			m_PortraitFullView.Bind(base.ViewModel.PortraitVM.Value);
		}
		else
		{
			m_PortraitFullView.Unbind();
		}
	}

	public void CurrentPhaseChanged(CharGenPhaseBaseVM viewModel)
	{
		if (viewModel != null)
		{
			Game.Instance.GameCommandQueue.CharGenChangePhase(viewModel.PhaseType);
		}
	}

	void ICharGenChangePhaseHandler.HandlePhaseChange(CharGenPhaseType phaseType)
	{
		CharGenPhaseBaseVM charGenPhaseBaseVM = base.ViewModel.PhasesCollection.FirstOrDefault((CharGenPhaseBaseVM phase) => phase.PhaseType == phaseType);
		if (!UINetUtility.IsControlMainCharacter())
		{
			base.ViewModel.CurrentPhaseVM.Value = charGenPhaseBaseVM;
		}
		CurrentPhaseChangedImpl(charGenPhaseBaseVM);
	}

	public virtual void CurrentPhaseChangedImpl(CharGenPhaseBaseVM viewModel)
	{
		if (viewModel != null)
		{
			if (viewModel.HasPantograph)
			{
				m_PantographView.Show();
			}
			else
			{
				m_PantographView.Hide();
			}
			m_PortraitView.SetVisibility(viewModel.HasPortrait);
			if (!m_DeviceBackWasOpenedBefore)
			{
				UISounds.Instance.Sounds.Inventory.InventoryOpen.Play();
				m_DeviceBackWasOpenedBefore = true;
			}
			else
			{
				UISounds.Instance.Sounds.Inventory.InventoryClose.Play();
				m_DeviceBackWasOpenedBefore = false;
			}
			base.ViewModel.LastPhase?.EndDetailedView();
			SelectedDetailView?.Unbind();
			base.ViewModel.SetLastPhase(viewModel);
			UpdateDollRooms(viewModel);
			viewModel.BeginDetailedView();
			SelectedDetailView = DetailedViewsFactory.GetDetailedPhaseView(viewModel);
			CanGoBack.Value = true;
			DelayedUpdate();
			TooltipHelper.HideInfo();
			base.ViewModel.HideVisualSettings();
		}
	}

	private void DelayedUpdate()
	{
		DelayedInvoker.InvokeInFrames(RefreshInput, 3);
	}

	private void CreateNavigation()
	{
		AddDisposable(Navigation = new GridConsoleNavigationBehaviour());
		CreateInputLayer();
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = Navigation.GetInputLayer(new InputLayer
		{
			ContextName = InputLayerContextName
		});
		CreateInputImpl(inputLayer, base.ViewModel?.IsMainCharacter ?? new BoolReactiveProperty(UINetUtility.IsControlMainCharacter()));
		return inputLayer;
	}

	protected virtual void CreateInputImpl(InputLayer inputLayer, BoolReactiveProperty isMainCharacter)
	{
	}

	private void CreateInputLayer()
	{
		InputLayer = GetInputLayer();
		GamePad.Instance.PushLayer(InputLayer);
	}

	private void DestroyInputLayer()
	{
		if (InputLayer != null)
		{
			GamePad.Instance.PopLayer(InputLayer);
			InputLayer = null;
		}
	}

	protected virtual void RefreshInput()
	{
		Navigation.Clear();
		DestroyInputLayer();
		CreateNavigation();
	}

	private void HideRooms()
	{
		m_ActiveRoom.Or(null)?.Hide();
	}

	private void UpdateDollRooms(CharGenPhaseBaseVM viewModel)
	{
		if (viewModel != null)
		{
			bool flag = viewModel.DollRoomType == CharGenDollRoomType.Character;
			if (flag)
			{
				SetDoll();
			}
			if (GetActiveDollRoomType() != viewModel.DollRoomType)
			{
				m_ActiveRoom.Or(null)?.Hide();
				CharacterRoom.SetVisibility(flag);
				ShipRoom.SetVisibility(!flag);
				m_ActiveRoom = (flag ? ((DollRoomBase)CharacterRoom) : ((DollRoomBase)ShipRoom));
			}
			m_CharacterDollTexture.PlayAnimation(flag);
			m_ShipDollTexture.PlayAnimation(!flag);
			DollPosition dollPosition = m_DollPositions.First((DollPosition i) => i.Position == viewModel.DollPosition);
			m_DollTransform.transform.position = dollPosition.Transform.position;
		}
	}

	protected CharGenDollRoomType? GetActiveDollRoomType()
	{
		if (CharacterRoom.IsVisible)
		{
			return CharGenDollRoomType.Character;
		}
		if (ShipRoom.IsVisible)
		{
			return CharGenDollRoomType.Ship;
		}
		return null;
	}

	private void SetDoll()
	{
		CharacterRoom.BindDollState(base.ViewModel.CharGenContext.Doll);
	}

	protected virtual void CloseCharGen()
	{
		bool isMainCharacter = UINetUtility.IsControlMainCharacter();
		UIUtility.ShowMessageBox(isMainCharacter ? UIStrings.Instance.CharGen.SureWantClose : UIStrings.Instance.CharGen.CloseCoopChargenNotRt, DialogMessageBoxBase.BoxType.Dialog, delegate(DialogMessageBoxBase.BoxButton button)
		{
			if (base.ViewModel != null && button == DialogMessageBoxBase.BoxButton.Yes)
			{
				if (isMainCharacter)
				{
					base.ViewModel.Close();
				}
				else
				{
					Game.Instance.ResetToMainMenu();
				}
			}
		});
	}

	protected virtual void NextPressed()
	{
		if (CanGoNext.Value)
		{
			if (base.ViewModel.CurrentPhaseCanInterrupt && !base.ViewModel.CurrentPhaseIsCompleted.Value)
			{
				base.ViewModel.CurrentPhaseVM.Value?.InterruptChargen(InterruptCallback);
				return;
			}
			TooltipHelper.HideInfo();
			GoTeNextPhaseAfterDelay();
		}
	}

	private void InterruptCallback()
	{
		if (base.ViewModel.CurrentPhaseIsCompleted.Value)
		{
			NextPressed();
		}
	}

	protected virtual void BackPressed()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			GoToPrevPhaseOrClose(first: false);
		}, 1);
	}

	protected void GoTeNextPhaseAfterDelay()
	{
		if (CanGoNext.Value)
		{
			Navigation.Clear();
		}
		DelayedInvoker.InvokeInFrames(delegate
		{
			GoToNextPhaseOrComplete(lastValid: false);
		}, 1);
	}

	protected void GoToNextPhaseOrComplete(bool lastValid)
	{
		if (CurrentPhaseIsLast)
		{
			RoadmapMenuView.ShutUpSelector();
			base.ViewModel.Complete();
		}
		else if (lastValid)
		{
			RoadmapMenuView.SelectLastValidPhase();
		}
		else
		{
			RoadmapMenuView.SelectNextPhase();
		}
	}

	protected void GoToPrevPhaseOrClose(bool first)
	{
		if (CurrentPhaseIsFirst)
		{
			CloseCharGen();
		}
		else if (first)
		{
			RoadmapMenuView.SelectFirstValidPhase();
		}
		else
		{
			RoadmapMenuView.SelectPrevPhase();
		}
	}

	public void HandleFullScreenUiChanged(bool windowShowed, FullScreenUIType fullScreenUIType)
	{
		if (fullScreenUIType == FullScreenUIType.Encyclopedia)
		{
			m_CanvasGroup.alpha = (windowShowed ? 0f : 1f);
			m_CanvasGroup.interactable = !windowShowed;
			m_CanvasGroup.blocksRaycasts = !windowShowed;
			if (windowShowed)
			{
				m_PantographView.Hide();
			}
			else
			{
				m_PantographView.Show();
			}
		}
	}
}
