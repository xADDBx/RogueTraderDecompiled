using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;

public class SettingsEntityStatisticsOptOutConsoleView : SettingsEntityConsoleView<SettingsEntityStatisticsOptOutVM>, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler, IFunc01ClickHandler
{
	[SerializeField]
	private OwlcatMultiButton m_GoToStatisticsButton;

	[SerializeField]
	private TextMeshProUGUI m_GoToStatisticsButtonLabel;

	[SerializeField]
	private ConsoleHint m_GoToStatisticsHint;

	[SerializeField]
	private OwlcatMultiButton m_DeleteStatisticsDataButton;

	[SerializeField]
	private TextMeshProUGUI m_DeleteDataButtonLabel;

	[SerializeField]
	private ConsoleHint m_DeleteDataHint;

	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_SelectableMultiButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_GoToStatisticsButtonLabel.text = UIStrings.Instance.SettingsUI.ShowStatistics;
		AddDisposable(m_GoToStatisticsButton.OnLeftClick.AsObservable().Subscribe(base.ViewModel.OpenSettingsInBrowser));
		AddDisposable(m_GoToStatisticsHint.BindCustomAction(8, GamePad.Instance.CurrentInputLayer, m_SelectableMultiButton.OnFocusAsObservable().ToReactiveProperty()));
		m_GoToStatisticsHint.SetActive(state: false);
		m_DeleteDataButtonLabel.text = UIStrings.Instance.SettingsUI.DeleteStatisticsData;
		AddDisposable(m_DeleteStatisticsDataButton.OnLeftClick.AsObservable().Subscribe(base.ViewModel.DeleteStatisticsData));
		AddDisposable(m_DeleteDataHint.BindCustomAction(10, GamePad.Instance.CurrentInputLayer, m_SelectableMultiButton.OnFocusAsObservable().ToReactiveProperty()));
		m_DeleteDataHint.SetActive(state: false);
	}

	public void SetFocus(bool value)
	{
		m_SelectableMultiButton.SetFocus(value);
		m_SelectableMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
		if (value)
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleShowSettingsDescription(base.ViewModel.UISettingsEntity);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleHideSettingsDescription();
			});
		}
	}

	public bool IsValid()
	{
		return true;
	}

	public bool CanConfirmClick()
	{
		return true;
	}

	public void OnConfirmClick()
	{
		base.ViewModel.OpenSettingsInBrowser();
	}

	public string GetConfirmClickHint()
	{
		return UIStrings.Instance.SettingsUI.ShowStatistics.Text;
	}

	public bool CanFunc01Click()
	{
		return true;
	}

	public void OnFunc01Click()
	{
		base.ViewModel.DeleteStatisticsData();
	}

	public string GetFunc01ClickHint()
	{
		return UIStrings.Instance.SettingsUI.DeleteStatisticsData.Text;
	}

	protected override void UpdateLocalization()
	{
		base.UpdateLocalization();
		SetButtonsTexts();
	}

	private void SetButtonsTexts()
	{
		m_GoToStatisticsButtonLabel.text = UIStrings.Instance.SettingsUI.ShowStatistics;
		m_DeleteDataButtonLabel.text = UIStrings.Instance.SettingsUI.DeleteStatisticsData;
	}
}
