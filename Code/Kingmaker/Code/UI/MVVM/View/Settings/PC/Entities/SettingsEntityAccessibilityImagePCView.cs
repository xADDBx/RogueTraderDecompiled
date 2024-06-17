using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;

public class SettingsEntityAccessibilityImagePCView : SettingsEntityView<SettingsEntityAccessibilityImageVM>
{
	[SerializeField]
	private Image m_AccessibilityImage;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;
}
