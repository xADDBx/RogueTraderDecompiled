using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Common.Dropdown;

public class DropdownItemView : ViewBase<DropdownItemVM>, IConsoleEntityProxy, IConsoleEntity, IWidgetView
{
	[SerializeField]
	private OwlcatToggle m_Toggle;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Image m_Image;

	public MonoBehaviour MonoBehaviour => this;

	public OwlcatToggle Toggle => m_Toggle;

	public IConsoleEntity ConsoleEntityProxy => m_Toggle;

	public string TextValue => base.ViewModel.Text;

	public void SetToggleGroup(OwlcatToggleGroup toggleGroup)
	{
		m_Toggle.Group = toggleGroup;
	}

	protected override void BindViewImplementation()
	{
		m_Text.text = base.ViewModel.Text;
		if (base.ViewModel.Icon != null && m_Image != null)
		{
			m_Image.sprite = base.ViewModel.Icon;
		}
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetItemHeight(float height)
	{
		RectTransform rectTransform = m_Text.transform as RectTransform;
		if (rectTransform != null)
		{
			rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, height);
		}
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as DropdownItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is DropdownItemVM;
	}
}
