using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent;

public class BookEventSkillPCView : ViewBase<CharInfoStatVM>, IWidgetView
{
	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private TextMeshProUGUI m_HighlightedValue;

	private bool m_IsInit;

	public MonoBehaviour MonoBehaviour => this;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		OnChangedValue();
	}

	public void Highlight()
	{
		m_Value.gameObject.SetActive(value: false);
		m_HighlightedValue.gameObject.SetActive(value: true);
		m_HighlightedValue.text = UIUtility.AddSign(base.ViewModel.StatValue.Value);
	}

	private void OnChangedValue()
	{
		if (!(m_Value.gameObject == null))
		{
			m_Value.gameObject.SetActive(value: true);
			m_HighlightedValue.gameObject.SetActive(value: false);
			m_Value.text = UIUtility.AddSign(base.ViewModel?.StatValue.Value);
		}
	}

	public void SetSelected(bool state)
	{
		m_Background.gameObject.SetActive(state);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		if (vm == null)
		{
			OnChangedValue();
		}
		else
		{
			Bind((CharInfoStatVM)vm);
		}
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharInfoStatVM;
	}
}
