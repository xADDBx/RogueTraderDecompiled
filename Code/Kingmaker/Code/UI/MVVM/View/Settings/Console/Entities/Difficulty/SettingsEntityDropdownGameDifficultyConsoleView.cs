using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities.Difficulty;

public class SettingsEntityDropdownGameDifficultyConsoleView : SettingsEntityWithValueConsoleView<SettingsEntityDropdownGameDifficultyVM>
{
	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutElementSettings;

	[SerializeField]
	private List<SettingsEntityDropdownGameDifficultyItemConsoleView> m_ItemViews;

	[SerializeField]
	private OwlcatMultiButton m_FocusMultiButton;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutElementSettings;

	private int SelectedIndex => base.ViewModel.Items.IndexOf(base.ViewModel.Items.FirstOrDefault((SettingsEntityDropdownGameDifficultyItemVM item) => item.IsSelected.Value));

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		for (int i = 0; i < base.ViewModel.Items.Count; i++)
		{
			m_ItemViews[i].Bind(base.ViewModel.Items[i]);
		}
	}

	public override void SetFocus(bool value)
	{
		if (value)
		{
			EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
			{
				h.HandleShowSettingsDescription(null, base.ViewModel.Items[base.ViewModel.TempIndexValue.Value].Title, base.ViewModel.Items[base.ViewModel.TempIndexValue.Value].Description);
			});
		}
		SetupColor(value);
		m_FocusMultiButton.SetFocus(value);
		m_FocusMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
	}

	public override bool HandleLeft()
	{
		return TrySelectPrevValidItem();
	}

	public override bool HandleRight()
	{
		return TrySelectNextValidItem();
	}

	private bool TrySelectPrevValidItem()
	{
		SettingsEntityDropdownGameDifficultyItemConsoleView settingsEntityDropdownGameDifficultyItemConsoleView = m_ItemViews.GetRange(0, SelectedIndex).LastOrDefault((SettingsEntityDropdownGameDifficultyItemConsoleView item) => item.gameObject.activeSelf);
		if (settingsEntityDropdownGameDifficultyItemConsoleView == null)
		{
			return false;
		}
		base.ViewModel.SetValue(m_ItemViews.IndexOf(settingsEntityDropdownGameDifficultyItemConsoleView));
		return true;
	}

	private bool TrySelectNextValidItem()
	{
		SettingsEntityDropdownGameDifficultyItemConsoleView settingsEntityDropdownGameDifficultyItemConsoleView = m_ItemViews.GetRange(SelectedIndex + 1, m_ItemViews.Count - SelectedIndex - 1).FirstOrDefault((SettingsEntityDropdownGameDifficultyItemConsoleView item) => item.gameObject.activeSelf);
		if (settingsEntityDropdownGameDifficultyItemConsoleView == null)
		{
			return false;
		}
		base.ViewModel.SetValue(m_ItemViews.IndexOf(settingsEntityDropdownGameDifficultyItemConsoleView));
		return true;
	}
}
