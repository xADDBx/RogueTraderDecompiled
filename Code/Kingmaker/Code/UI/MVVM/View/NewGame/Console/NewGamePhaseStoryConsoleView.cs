using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Console;

public class NewGamePhaseStoryConsoleView : NewGamePhaseStoryBaseView
{
	[SerializeField]
	private NewGamePhaseStoryScenarioSelectorConsoleView m_StorySelectorConsoleView;

	[SerializeField]
	private ConsoleHint m_ScrollStoryHint;

	[SerializeField]
	private ConsoleHint m_PurchaseHint;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_StorySelectorConsoleView.Bind(base.ViewModel.SelectionGroup);
	}

	public void Scroll(InputActionEventData arg1, float x)
	{
		Scroll(x);
	}

	private void Scroll(float x)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, x * m_ScrollRect.scrollSensitivity);
		m_ScrollRect.OnSmoothlyScroll(pointerEventData);
	}

	public void CreateInputImpl(InputLayer inputLayer, ConsoleHintsWidget hintsWidget, ConsoleHint switchOnOffHint, ConsoleHint purchaseHint)
	{
		if (m_ScrollStoryHint != null)
		{
			AddDisposable(m_ScrollStoryHint.BindCustomAction(3, inputLayer, base.ViewModel.IsEnabled));
		}
		if (m_PurchaseHint != null)
		{
			AddDisposable(m_PurchaseHint.BindCustomAction(10, inputLayer, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought.Not()).ToReactiveProperty()));
		}
		AddDisposable(switchOnOffHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.SwitchDlcOn();
		}, 10, base.ViewModel.IsEnabled.And(SwitchOnButtonActive).ToReactiveProperty())));
		switchOnOffHint.SetLabel(UIStrings.Instance.CharGen.Back);
		AddDisposable(base.ViewModel.DlcIsOn.Subscribe(delegate(bool value)
		{
			switchOnOffHint.SetLabel(value ? UIStrings.Instance.SettingsUI.SettingsToggleOff : UIStrings.Instance.SettingsUI.SettingsToggleOn);
		}));
		AddDisposable(purchaseHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.ShowInStore();
		}, 10, base.ViewModel.IsEnabled.And(base.ViewModel.DlcIsBought.Not()).ToReactiveProperty())));
		purchaseHint.SetLabel(UIStrings.Instance.DlcManager.Purchase);
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		return m_StorySelectorConsoleView.GetNavigationEntities();
	}
}
