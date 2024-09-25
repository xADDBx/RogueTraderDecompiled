using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;

public class SettingsEntityDisplayImagesPCView : SettingsEntityView<SettingsEntityDisplayImagesVM>
{
	[SerializeField]
	private Image m_DisplayImage_1;

	[SerializeField]
	private Image m_DisplayImage_2;

	[SerializeField]
	private Image m_DisplayImage_3;

	[SerializeField]
	private TextMeshProUGUI m_DisplayImageText_1;

	[SerializeField]
	private TextMeshProUGUI m_DisplayImageText_2;

	[SerializeField]
	private TextMeshProUGUI m_DisplayImageText_3;

	protected UITextSettingsUI m_UITextSettingsUI;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		m_UITextSettingsUI = UIStrings.Instance.SettingsUI;
		m_DisplayImageText_1.text = m_UITextSettingsUI.DisplayImageShadows;
		m_DisplayImageText_2.text = m_UITextSettingsUI.DisplayImageMidtones;
		m_DisplayImageText_3.text = m_UITextSettingsUI.DisplayImageBrights;
	}
}
