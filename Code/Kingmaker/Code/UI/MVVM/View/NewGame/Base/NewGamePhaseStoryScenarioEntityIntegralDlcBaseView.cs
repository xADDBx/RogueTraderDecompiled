using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.NewGame.Story;
using Kingmaker.DLC;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
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

	private bool m_IsSelected;

	private bool m_DlcIsOn;

	private PantographConfig PantographConfig { get; set; }

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Title.text = base.ViewModel.Title;
		m_DlcIsOn = base.ViewModel.BlueprintDlc.GetDlcSwitchOnOffState();
		m_Button.SetActiveLayer((!base.ViewModel.BlueprintDlc.IsPurchased) ? "NotAvailable" : (m_DlcIsOn ? "Available" : "Off"));
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
		List<Sprite> icons = new List<Sprite>
		{
			m_LittleCircleSprite,
			(!base.ViewModel.BlueprintDlc.IsPurchased) ? m_DlcNotAvailableSprite : (m_DlcIsOn ? m_DlcAvailableSprite : m_TransparentSprite)
		};
		PantographConfig = new PantographConfig(base.transform, base.ViewModel.Title, icons);
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
		m_Button.SetActiveLayer((!base.ViewModel.BlueprintDlc.IsPurchased) ? "NotAvailable" : (value ? "Available" : "Off"));
		if (m_IsSelected)
		{
			SetupPantographConfig();
			EventBus.RaiseEvent(delegate(IPantographHandler h)
			{
				h.Bind(PantographConfig);
			});
		}
	}
}
