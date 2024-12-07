using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.Controls.Toggles;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Common.Dropdown;

public class DropdownItemView : ViewBase<DropdownItemVM>, IConsoleEntityProxy, IConsoleEntity, IWidgetView, ISettingsFontSizeUIHandler, ISubscriber
{
	[SerializeField]
	private OwlcatToggle m_Toggle;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private float m_DefaultFontSize = 26f;

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
		SetTextFontSize();
		if (base.ViewModel.Icon != null && m_Image != null)
		{
			m_Image.sprite = base.ViewModel.Icon;
		}
		AddDisposable(EventBus.Subscribe(this));
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

	private void SetTextFontSize()
	{
		m_Text.fontSize = m_DefaultFontSize * (Game.Instance.IsControllerGamepad ? 1f : base.ViewModel.FontSizeMultiplier);
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as DropdownItemVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is DropdownItemVM;
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		m_Text.fontSize = m_DefaultFontSize * (Game.Instance.IsControllerGamepad ? 1f : size);
	}
}
