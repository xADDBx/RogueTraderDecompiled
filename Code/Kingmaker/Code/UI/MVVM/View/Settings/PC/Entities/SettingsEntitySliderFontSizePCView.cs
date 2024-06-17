using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.PC.Entities;

public class SettingsEntitySliderFontSizePCView : SettingsEntitySliderPCView
{
	[SerializeField]
	private TextMeshProUGUI m_FontSizeExample;

	[SerializeField]
	private float m_OriginalFontSize;

	private UITextSettingsUI m_UITextSettingsUI;

	public List<TMP_FontAsset> Fonts;

	[SerializeField]
	private VirtualListLayoutElementSettings m_LayoutSettings;

	public override VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_UITextSettingsUI = UIStrings.Instance.SettingsUI;
		m_FontSizeExample.text = m_UITextSettingsUI.AccessiabilityExampleFontSize;
		AddDisposable(base.ViewModel.TempFloatValue.Subscribe(SetFontsSize));
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
	}

	private void SetFontsSize(float fontSize)
	{
		m_FontSizeExample.fontSize = m_OriginalFontSize * fontSize;
	}
}
