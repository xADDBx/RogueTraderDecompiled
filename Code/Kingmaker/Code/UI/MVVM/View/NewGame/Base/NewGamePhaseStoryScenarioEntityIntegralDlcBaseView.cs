using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Story;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UI.MVVM.View.Pantograph;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup.View;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.NewGame.Base;

public class NewGamePhaseStoryScenarioEntityIntegralDlcBaseView : SelectionGroupEntityView<NewGamePhaseStoryScenarioEntityIntegralDlcVM>, IWidgetView, INewGameSwitchOnOffDlcHandler, ISubscriber
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private Sprite m_TransparentSprite;

	[SerializeField]
	private Sprite m_LittleCircleSprite;

	[SerializeField]
	private Sprite m_DlcAvailableSprite;

	[SerializeField]
	private Sprite m_DlcNotAvailableSprite;

	[SerializeField]
	private Sprite m_DlcBoughtAndNotInstalled;

	[SerializeField]
	private TextMeshProUGUI m_OnText;

	[SerializeField]
	private TextMeshProUGUI m_OffText;

	private bool m_IsSelected;

	private bool m_DlcIsOn;

	private PantographConfig PantographConfig { get; set; }

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Title.text = base.ViewModel.Title;
		m_OnText.text = UIStrings.Instance.SettingsUI.SettingsToggleOn;
		m_OffText.text = UIStrings.Instance.SettingsUI.SettingsToggleOff;
		m_DlcIsOn = base.ViewModel.BlueprintDlc.GetDlcSwitchOnOffState();
		UpdateButtonState(m_DlcIsOn);
		SetupPantographConfig();
		AddDisposable(EventBus.Subscribe(this));
	}

	public override void OnChangeSelectedState(bool value)
	{
		base.OnChangeSelectedState(value);
		m_IsSelected = value;
		if (value)
		{
			SetupPantographConfig();
			EventBus.RaiseEvent(delegate(IPantographHandler h)
			{
				h.Bind(PantographConfig);
			});
			EventBus.RaiseEvent(delegate(INewGameChangeDlcHandler h)
			{
				h.HandleNewGameChangeDlc(base.ViewModel.Campaign, base.ViewModel.BlueprintDlc);
			});
		}
		OnChangeSelectedStateImpl();
	}

	protected virtual void OnChangeSelectedStateImpl()
	{
	}

	private void SetupPantographConfig()
	{
		List<Sprite> list = new List<Sprite> { m_LittleCircleSprite };
		if (!base.ViewModel.BlueprintDlc.IsPurchased)
		{
			list.Add(m_DlcNotAvailableSprite);
		}
		if (base.ViewModel.BlueprintDlc.IsPurchased && base.ViewModel.BlueprintDlc.GetDownloadState() == DownloadState.NotLoaded)
		{
			list.Add(m_DlcBoughtAndNotInstalled);
		}
		string textIcon = ((base.ViewModel.BlueprintDlc.IsPurchased && base.ViewModel.BlueprintDlc.GetDownloadState() != 0) ? ((string)(m_DlcIsOn ? UIStrings.Instance.SettingsUI.SettingsToggleOn : UIStrings.Instance.SettingsUI.SettingsToggleOff)) : string.Empty);
		PantographConfig = new PantographConfig(base.transform, base.ViewModel.Title, list, useLargeView: false, textIcon);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as NewGamePhaseStoryScenarioEntityIntegralDlcVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is NewGamePhaseStoryScenarioEntityIntegralDlcVM;
	}

	public void HandleNewGameSwitchOnOffDlc(BlueprintDlc dlc, bool value)
	{
		if (dlc != base.ViewModel.BlueprintDlc)
		{
			return;
		}
		m_DlcIsOn = value;
		UpdateButtonState(value);
		if (m_IsSelected)
		{
			SetupPantographConfig();
			EventBus.RaiseEvent(delegate(IPantographHandler h)
			{
				h.Bind(PantographConfig);
			});
		}
	}

	private void UpdateButtonState(bool dlcIsOn)
	{
		string activeLayer = "NotAvailable";
		if (base.ViewModel.BlueprintDlc.IsPurchased)
		{
			activeLayer = ((base.ViewModel.BlueprintDlc.GetDownloadState() != 0) ? (dlcIsOn ? "On" : "Off") : "NotLoaded");
		}
		m_Button.SetActiveLayer(activeLayer);
	}
}
