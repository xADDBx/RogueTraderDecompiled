using Kingmaker.Blueprints.Root.Strings;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.Common;

public class EditableTextInput : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	protected TMP_InputField m_TextInput;

	[SerializeField]
	protected string m_Text = "";

	[SerializeField]
	protected string m_Placeholder = "";

	protected bool m_IsEdit;

	public TMP_InputField TextInput
	{
		get
		{
			if (!m_TextInput)
			{
				return m_TextInput = GetComponent<TMP_InputField>();
			}
			return m_TextInput;
		}
	}

	public string Text
	{
		get
		{
			return m_Text;
		}
		set
		{
			m_Text = value;
			TextInput.text = (m_IsEdit ? m_Text : (m_Text + " " + UIStrings.Instance.SaveLoadTexts.SaveClickToEdit));
		}
	}

	public string Placeholder
	{
		get
		{
			return m_Placeholder;
		}
		set
		{
			m_Placeholder = value;
			if ((bool)m_TextInput.placeholder && m_TextInput.placeholder is TextMeshProUGUI)
			{
				(m_TextInput.placeholder as TextMeshProUGUI).text = m_Placeholder;
			}
		}
	}

	protected void Awake()
	{
		m_TextInput = GetComponent<TMP_InputField>();
		m_TextInput.interactable = false;
		Text = m_Text;
		Placeholder = m_Placeholder;
	}

	protected void BeingEdit()
	{
		m_TextInput.interactable = true;
		m_TextInput.Select();
		m_TextInput.ActivateInputField();
		m_TextInput.onEndEdit.RemoveAllListeners();
		m_TextInput.onEndEdit.AddListener(EndEdit);
		m_IsEdit = true;
		Text = m_Text;
	}

	protected void EndEdit(string text)
	{
		m_IsEdit = false;
		m_TextInput.interactable = false;
		Text = text;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!m_IsEdit)
		{
			BeingEdit();
		}
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}
}
