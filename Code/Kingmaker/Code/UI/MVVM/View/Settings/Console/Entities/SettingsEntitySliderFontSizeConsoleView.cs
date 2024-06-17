using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.Entities;

public class SettingsEntitySliderFontSizeConsoleView : SettingsEntitySliderConsoleView
{
	[SerializeField]
	private TextMeshProUGUI m_FontSizeExample;

	[SerializeField]
	private float m_OriginalFontSize;

	private UITextSettingsUI m_UITextSettingsUI;

	public List<TMP_FontAsset> Fonts;

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
