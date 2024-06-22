using Kingmaker.UI.Models.SettingsUI;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public class ItemsFilterSearchPCView : ItemsFilterSearchBaseView
{
	[Header("Dropdown Part")]
	[SerializeField]
	private OwlcatButton m_DropdownButton;

	[SerializeField]
	private TMP_Dropdown m_Dropdown;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_InputField.text = string.Empty;
		AddDisposable(m_InputField.ObserveEveryValueChanged((TMP_InputField f) => f.text).Skip(1).Subscribe(base.OnSearchStringEdit));
		if ((bool)m_DropdownButton && (bool)m_Dropdown)
		{
			AddDisposable(m_DropdownButton.OnLeftClickAsObservable().Subscribe(delegate
			{
				ShowDropdown();
			}));
			AddDisposable(m_Dropdown.onValueChanged.AsObservable().Subscribe(SetValueFromDropdown));
			SetupDropdown();
		}
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.OpenSearchInventory.name, ActivateDeactivateInputField));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void ActivateDeactivateInputField()
	{
		if (m_InputField.isFocused)
		{
			m_InputField.DeactivateInputField();
		}
		else
		{
			m_InputField.ActivateInputField();
		}
	}

	private void SetValueFromDropdown(int value)
	{
		m_InputField.text = DropdownValues[value];
	}

	public void ShowDropdown()
	{
		if (m_Dropdown.IsExpanded)
		{
			m_Dropdown.Hide();
		}
		else
		{
			m_Dropdown.Show();
		}
	}

	private void SetupDropdown()
	{
		m_Dropdown.ClearOptions();
		m_Dropdown.AddOptions(DropdownValues);
	}

	public override void SetActive(bool value)
	{
		base.gameObject.SetActive(value);
		m_Dropdown.Hide();
		m_InputField.text = string.Empty;
		if (value)
		{
			m_InputField.Select();
		}
	}
}
