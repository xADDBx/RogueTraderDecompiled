using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.ChangeAppearance.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.UI.MVVM.VM.ServiceWindows.Inventory;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ChangeAppearance.Console;

public class ChangeAppearanceConsoleView : ChangeAppearanceView
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
	private ConsoleHint m_NextHint;

	[SerializeField]
	private ConsoleHint m_PrevHint;

	[Space]
	[SerializeField]
	private ConsoleHint m_DeclineHint;

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

	private readonly BoolReactiveProperty m_CanGoNextOnConfirm = new BoolReactiveProperty(initialValue: true);

	private readonly BoolReactiveProperty m_CanGoBackOnDecline = new BoolReactiveProperty(initialValue: true);

	public static bool ShowTooltip = true;

	private InputBindStruct m_VisualSettingsBind;

	private readonly BoolReactiveProperty m_DollZoomEnabled = new BoolReactiveProperty();

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
		AddDisposable(m_DollZoomEnabled.Where((bool v) => !v).Subscribe(delegate
		{
			m_CharacterController.ZoomMax();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_HintsWidget.Dispose();
	}

	protected override void CreateInputImpl(InputLayer inputLayer, BoolReactiveProperty isMainCharacter)
	{
		IReadOnlyReactiveProperty<bool> readOnlyReactiveProperty = m_AppearancePhaseDetailedView.GetCanGoNextOnConfirmProperty().And(m_AppearancePhaseDetailedView.CurrentPhaseIsLast.Not()).And(isMainCharacter)
			.ToReactiveProperty();
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			m_AppearancePhaseDetailedView.PressConfirmOnPhase();
		}, 8, readOnlyReactiveProperty, InputActionEventType.ButtonShortPressJustReleased);
		AddDisposable(m_NextHint.Bind(inputBindStruct));
		AddDisposable(inputBindStruct);
		m_NextHint.SetLabel(UIStrings.Instance.CharGen.Next);
		IReadOnlyReactiveProperty<bool> readOnlyReactiveProperty2 = m_AppearancePhaseDetailedView.GetCanGoBackOnDeclineProperty().And(m_AppearancePhaseDetailedView.CurrentPageIsFirst.Not()).And(isMainCharacter)
			.ToReactiveProperty();
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			m_AppearancePhaseDetailedView.PressDeclineOnPhase();
		}, 9, readOnlyReactiveProperty2, InputActionEventType.ButtonShortPressJustReleased);
		AddDisposable(m_PrevHint.Bind(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		m_PrevHint.SetLabel(UIStrings.Instance.CharGen.Back);
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			OnConfirm();
		}, 8, m_CanGoNextOnConfirm.And(isMainCharacter).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed);
		AddDisposable(m_ConfirmHint.Bind(inputBindStruct3));
		AddDisposable(inputBindStruct3);
		m_ConfirmHint.SetLabel(UIStrings.Instance.CharGen.Complete);
		InputBindStruct inputBindStruct4 = inputLayer.AddButton(delegate
		{
			OnClose();
		}, 9, m_CanGoBackOnDecline.And(isMainCharacter).ToReactiveProperty(), InputActionEventType.ButtonJustLongPressed);
		AddDisposable(m_DeclineHint.Bind(inputBindStruct4));
		AddDisposable(inputBindStruct4);
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
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
		m_AppearancePhaseDetailedView.AddInput(ref inputLayer, ref Navigation, m_HintsWidget, base.ViewModel.IsMainCharacter);
	}

	private void RotateDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_CharacterController.Rotate((0f - x) * m_RotateFactor);
		}
	}

	private void ZoomDoll(InputActionEventData obj, float x)
	{
		if (!(Mathf.Abs(x) < m_ZoomThresholdValue))
		{
			m_CharacterController.Zoom(x * m_ZoomFactor);
		}
	}
}
