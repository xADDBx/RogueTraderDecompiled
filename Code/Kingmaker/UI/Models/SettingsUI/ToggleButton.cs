using JetBrains.Annotations;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DisposableExtension;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.Models.SettingsUI;

public class ToggleButton : MonoBehaviour
{
	public ScrollRectExtended.BoolEvent OnValueChanged = new ScrollRectExtended.BoolEvent();

	[SerializeField]
	[UsedImplicitly]
	private Toggle m_OnToggle;

	[SerializeField]
	[UsedImplicitly]
	private Image m_OnImage;

	[SerializeField]
	[UsedImplicitly]
	private Toggle m_OffToggle;

	[SerializeField]
	[UsedImplicitly]
	private Image m_OffImage;

	[SerializeField]
	[UsedImplicitly]
	private Color32 EnableTextColor;

	[SerializeField]
	[UsedImplicitly]
	private Color32 DisableTextColor;

	private bool m_IsInit;

	private bool m_Value;

	private DisposableBooleanFlag m_UpdatingGraphics = new DisposableBooleanFlag();

	public bool Value
	{
		get
		{
			return m_Value;
		}
		set
		{
			if (m_Value != value && !m_UpdatingGraphics)
			{
				m_Value = value;
				OnValueChanged?.Invoke(m_Value);
				SetupGraphics();
			}
		}
	}

	private void Init()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
			m_OnToggle.onValueChanged.AddListener(OnOnToggleChangedValue);
			m_OffToggle.onValueChanged.AddListener(OnOffToggleChangedValue);
		}
	}

	public void SetActive(bool value)
	{
		if (base.gameObject != null)
		{
			base.gameObject.SetActive(value);
			Init();
			SetupGraphics();
		}
	}

	private void OnOnToggleChangedValue(bool value)
	{
		Value = value;
	}

	private void OnOffToggleChangedValue(bool value)
	{
		Value = !value;
	}

	private void SetupGraphics()
	{
		m_OnImage.color = (m_Value ? EnableTextColor : DisableTextColor);
		m_OffImage.color = ((!m_Value) ? EnableTextColor : DisableTextColor);
		using (m_UpdatingGraphics.Retain())
		{
			m_OnToggle.isOn = m_Value;
			m_OffToggle.isOn = !m_Value;
		}
	}

	public void SetInteractable(bool dataModificationAllowed)
	{
		m_OnToggle.interactable = dataModificationAllowed;
		m_OffToggle.interactable = dataModificationAllowed;
	}
}
