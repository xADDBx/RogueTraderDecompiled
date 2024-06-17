using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public abstract class CharInfoFeatureSimpleBaseView : VirtualListElementViewBase<CharInfoFeatureVM>, IWidgetView
{
	[Header("Icon")]
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected TextMeshProUGUI m_AcronymText;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		Clear();
		Show();
		SetupIcon();
	}

	protected override void DestroyViewImplementation()
	{
		Hide();
	}

	protected virtual void Clear()
	{
		m_AcronymText.text = string.Empty;
		m_Icon.enabled = false;
		m_Icon.sprite = null;
		m_Icon.color = Color.white;
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		Clear();
	}

	public void SetupIcon()
	{
		m_Icon.enabled = true;
		if (base.ViewModel.Icon == null)
		{
			m_AcronymText.gameObject.SetActive(value: true);
			m_AcronymText.text = base.ViewModel.Acronym;
			m_Icon.color = UIUtility.GetColorByText(base.ViewModel.Acronym);
			m_Icon.sprite = UIUtility.GetIconByText(base.ViewModel.Acronym);
		}
		else
		{
			m_AcronymText.gameObject.SetActive(value: false);
			m_Icon.color = Color.white;
			m_Icon.sprite = base.ViewModel.Icon;
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as CharInfoFeatureVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is CharInfoFeatureVM;
	}
}
