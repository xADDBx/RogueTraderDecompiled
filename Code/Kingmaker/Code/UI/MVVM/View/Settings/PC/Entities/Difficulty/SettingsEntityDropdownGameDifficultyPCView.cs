using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities.Difficulty;

public class SettingsEntityDropdownGameDifficultyPCView : SettingsEntityWithValueView<SettingsEntityDropdownGameDifficultyVM>
{
	[SerializeField]
	private List<SettingsEntityDropdownGameDifficultyItemPCView> m_ItemViews;

	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutElementSettings;

	private int SelectedIndex => base.ViewModel.Items.IndexOf(base.ViewModel.Items.FirstOrDefault((SettingsEntityDropdownGameDifficultyItemVM item) => item.IsSelected.Value));

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutElementSettings;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		for (int i = 0; i < base.ViewModel.Items.Count; i++)
		{
			m_ItemViews[i].Bind(base.ViewModel.Items[i]);
		}
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
		SettingsEntityDropdownGameDifficultyItemPCView settingsEntityDropdownGameDifficultyItemPCView = m_ItemViews.GetRange(0, SelectedIndex).LastOrDefault((SettingsEntityDropdownGameDifficultyItemPCView item) => item.gameObject.activeSelf);
		if (settingsEntityDropdownGameDifficultyItemPCView == null)
		{
			return false;
		}
		base.ViewModel.SetValue(m_ItemViews.IndexOf(settingsEntityDropdownGameDifficultyItemPCView));
		return true;
	}

	private bool TrySelectNextValidItem()
	{
		SettingsEntityDropdownGameDifficultyItemPCView settingsEntityDropdownGameDifficultyItemPCView = m_ItemViews.GetRange(SelectedIndex + 1, m_ItemViews.Count - SelectedIndex - 1).FirstOrDefault((SettingsEntityDropdownGameDifficultyItemPCView item) => item.gameObject.activeSelf);
		if (settingsEntityDropdownGameDifficultyItemPCView == null)
		{
			return false;
		}
		base.ViewModel.SetValue(m_ItemViews.IndexOf(settingsEntityDropdownGameDifficultyItemPCView));
		return true;
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
		if (m_Selectable != null)
		{
			m_Selectable.SetActiveLayer(value ? "Selected" : "Normal");
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(ISettingsDescriptionUIHandler h)
		{
			h.HandleShowSettingsDescription(null, base.ViewModel.Items[base.ViewModel.TempIndexValue.Value].Title, base.ViewModel.Items[base.ViewModel.TempIndexValue.Value].Description);
		});
		SetupColor(isHighlighted: true);
	}
}
