using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.DollRoom;
using Kingmaker.UI.MVVM.View.CharGen.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Console;

public class CharGenConsoleView : CharGenView
{
	[Header("Customization Values")]
	[SerializeField]
	private float m_RotateFactor = 1f;

	[SerializeField]
	private float m_ZoomFactor = 1f;

	[SerializeField]
	private float m_ZoomThresholdValue = 0.01f;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	[SerializeField]
	private ConsoleHint m_NextPhaseHint;

	[Space]
	[SerializeField]
	private ConsoleHint m_DeclineHint;

	[SerializeField]
	private ConsoleHint m_PreviousPhaseHint;

	[Space]
	[SerializeField]
	private ConsoleHint m_VisualSettingsHint;

	[SerializeField]
	private CharacterVisualSettingsConsoleView m_VisualSettingsConsoleView;

	[SerializeField]
	private ConsoleHint m_FullPortraitHint;

	[Space]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private readonly BoolReactiveProperty m_NextEnabled = new BoolReactiveProperty(initialValue: true);

	private readonly BoolReactiveProperty m_CanGoNextOnConfirm = new BoolReactiveProperty(initialValue: true);

	private readonly BoolReactiveProperty m_CanGoNextInMenu = new BoolReactiveProperty(initialValue: true);

	private readonly BoolReactiveProperty m_CanGoBackOnDecline = new BoolReactiveProperty(initialValue: true);

	private readonly StringReactiveProperty m_ConfirmLabel = new StringReactiveProperty(string.Empty);

	private CompositeDisposable m_PhaseCanGoSubscription;

	public static bool ShowTooltip = true;

	private InputBindStruct m_VisualSettingsBind;

	private readonly BoolReactiveProperty m_DollZoomEnabled = new BoolReactiveProperty();

	private DollRoomTargetController RoomTargetController
	{
		get
		{
			if (GetActiveDollRoomType() != CharGenDollRoomType.Character)
			{
				return m_ShipController;
			}
			return m_CharacterController;
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		m_VisualSettingsConsoleView.Initialize();
		m_VisualSettingsConsoleView.SetDollRoomController(m_CharacterController, m_RotateFactor, m_ZoomFactor, m_ZoomThresholdValue);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.VisualSettingsVM.Subscribe(m_VisualSettingsConsoleView.Bind));
		AddDisposable(m_ConfirmLabel.Subscribe(delegate(string value)
		{
			m_ConfirmHint.SetLabel(value);
		}));
		AddDisposable(m_DollZoomEnabled.Where((bool v) => !v).Subscribe(delegate
		{
			RoomTargetController.ZoomMax();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_HintsWidget.Dispose();
		m_PhaseCanGoSubscription?.Dispose();
		m_PhaseCanGoSubscription = null;
	}

	protected override void CreateInputImpl(InputLayer inputLayer, BoolReactiveProperty isMainCharacter)
	{
		InputActionEventType eventType = (base.CurrentPhaseIsLast ? InputActionEventType.ButtonJustLongPressed : InputActionEventType.ButtonJustPressed);
		UpdateConfirmLabel();
		string label = (base.CurrentPhaseIsFirst ? UIStrings.Instance.CommonTexts.CloseWindow : UIStrings.Instance.CharGen.Back);
		if (base.CurrentPhaseIsLast)
		{
			m_NextEnabled.Value = false;
			DelayedInvoker.InvokeInTime(delegate
			{
				m_NextEnabled.Value = true;
			}, 0.5f);
		}
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			NextPressed();
		}, 15, m_NextEnabled.And(m_CanGoNextInMenu).And(isMainCharacter).ToReactiveProperty(), eventType);
		AddDisposable(m_NextPhaseHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			ConfirmPressed();
		}, 8, m_CanGoNextOnConfirm.And(isMainCharacter).ToReactiveProperty(), eventType);
		AddDisposable(m_ConfirmHint.Bind(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			BackPressed();
		}, 14, CanGoBack.And(isMainCharacter).ToReactiveProperty());
		AddDisposable(m_PreviousPhaseHint.Bind(inputBindStruct3));
		AddDisposable(inputBindStruct3);
		InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
		{
			DeclinePressed();
		}, 9, m_CanGoBackOnDecline.And(isMainCharacter).ToReactiveProperty());
		AddDisposable(m_DeclineHint.Bind(inputBindStruct4));
		AddDisposable(inputBindStruct4);
		m_DeclineHint.SetLabel(label);
		AddDisposable(m_VisualSettingsBind = inputLayer.AddButton(delegate
		{
			base.ViewModel.SwitchVisualSettings();
		}, 16, base.ViewModel.ShouldShowVisualSettings.And(m_CanGoBackOnDecline).And(isMainCharacter).ToReactiveProperty()));
		m_VisualSettingsHint.SetLabel(UIStrings.Instance.CharGen.ShowVisualSettings);
		AddDisposable(base.ViewModel.VisualSettingsVM.Subscribe(delegate(CharacterVisualSettingsVM value)
		{
			if (value == null)
			{
				m_VisualSettingsHint.Bind(m_VisualSettingsBind);
			}
		}));
		AddDisposable(inputLayer.AddButton(delegate
		{
			CloseCharGen();
		}, 9, base.ViewModel.IsMainCharacter.Not().And(m_CanGoBackOnDecline).ToReactiveProperty()));
		AddDisposable(inputLayer.AddAxis(RotateDoll, 2));
		AddDisposable(inputLayer.AddAxis(ZoomDoll, 3, m_DollZoomEnabled));
		InputBindStruct inputBindStruct5 = inputLayer.AddButton(delegate
		{
			SetFullPortraitVisible(visible: true);
		}, 17, m_CanGoBackOnDecline);
		AddDisposable(inputBindStruct5);
		AddDisposable(inputLayer.AddButton(delegate
		{
			SetFullPortraitVisible(visible: false);
		}, 17, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			SetFullPortraitVisible(visible: false);
		}, 17, InputActionEventType.ButtonLongPressJustReleased));
		AddDisposable(m_FullPortraitHint.Bind(inputBindStruct5));
	}

	protected override void RefreshInput()
	{
		try
		{
			base.RefreshInput();
			m_HintsWidget.Dispose();
			SelectedDetailView?.AddInput(ref InputLayer, ref Navigation, m_HintsWidget, base.ViewModel.IsMainCharacter);
			BoolReactiveProperty dollZoomEnabled = m_DollZoomEnabled;
			ICharGenPhaseDetailedView selectedDetailView = SelectedDetailView;
			dollZoomEnabled.Value = selectedDetailView != null && !selectedDetailView.HasYScrollBind;
		}
		catch (Exception)
		{
			Debug.Log("Error!");
		}
	}

	private void RotateDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			RoomTargetController.Or(null)?.Rotate((0f - x) * m_RotateFactor);
		}
	}

	private void ZoomDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			RoomTargetController.Or(null)?.Zoom(x * m_ZoomFactor);
		}
	}

	public override void CurrentPhaseChangedImpl(CharGenPhaseBaseVM viewModel)
	{
		base.CurrentPhaseChangedImpl(viewModel);
		m_PhaseCanGoSubscription?.Dispose();
		m_PhaseCanGoSubscription = new CompositeDisposable();
		if (SelectedDetailView != null)
		{
			m_PhaseCanGoSubscription.Add(SelectedDetailView.GetCanGoNextOnConfirmProperty().Subscribe(delegate(bool value)
			{
				m_CanGoNextOnConfirm.Value = value;
			}));
			m_PhaseCanGoSubscription.Add(SelectedDetailView.GetCanGoBackOnDeclineProperty().Subscribe(delegate(bool value)
			{
				m_CanGoBackOnDecline.Value = value;
			}));
			m_PhaseCanGoSubscription.Add(SelectedDetailView.CanGoNextInMenuProperty().Subscribe(delegate(bool value)
			{
				m_CanGoNextInMenu.Value = value;
			}));
		}
	}

	private void DeclinePressed()
	{
		if (SelectedDetailView == null || SelectedDetailView.PressDeclineOnPhase())
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				GoToPrevPhaseOrClose(first: false);
			}, 1);
		}
	}

	private void ConfirmPressed()
	{
		if (SelectedDetailView.PressConfirmOnPhase())
		{
			if (!CanGoNext.Value)
			{
				m_ConfirmHint.ShowTooltip(base.ViewModel.CurrentPhaseVM.Value.NotCompletedReasonTooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.None));
			}
			else if (base.ViewModel.CurrentPhaseCanInterrupt && !base.ViewModel.CurrentPhaseIsCompleted.Value)
			{
				base.ViewModel.CurrentPhaseVM.Value?.InterruptChargen(InterruptCallback);
			}
			else
			{
				GoTeNextPhaseAfterDelay();
			}
		}
	}

	private void InterruptCallback()
	{
		if (base.ViewModel.CurrentPhaseIsCompleted.Value)
		{
			ConfirmPressed();
		}
	}

	private void UpdateConfirmLabel()
	{
		if (base.CurrentPhaseIsLast)
		{
			m_ConfirmLabel.Value = UIStrings.Instance.CharGen.Complete;
			return;
		}
		CharGenPhaseBaseVM value2 = base.ViewModel.CurrentPhaseVM.Value;
		if (value2 != null && value2.CanInterruptChargen && !value2.IsCompletedAndAvailable.Value && !string.IsNullOrEmpty(base.ViewModel.CurrentPhaseVM.Value.OverrideConfirmHintLabel.Value))
		{
			m_PhaseCanGoSubscription.Add(base.ViewModel.CurrentPhaseVM.Value.OverrideConfirmHintLabel.Subscribe(delegate(string value)
			{
				m_ConfirmLabel.Value = value;
			}));
		}
		else
		{
			m_ConfirmLabel.Value = UIStrings.Instance.CharGen.Next;
		}
	}
}
