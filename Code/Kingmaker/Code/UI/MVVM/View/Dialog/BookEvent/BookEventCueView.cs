using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.DebugInformation;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent;

public class BookEventCueView : ViewBase<CueVM>, IHasBlueprintInfo
{
	[Serializable]
	public class BookEventCueStyle
	{
		[SerializeField]
		private FontStyles m_FontStyle;

		[SerializeField]
		private Color m_FontColor = Color.black;

		[SerializeField]
		private float m_FontSize = 18f;

		[SerializeField]
		private float m_CharacterSpacing;

		[SerializeField]
		private float m_LineSpacing;

		[SerializeField]
		private RectOffset m_Margins;

		public void ApplyStyleTo(TMP_Text text, float fontSizeMultiplier)
		{
			text.fontStyle = m_FontStyle;
			text.color = m_FontColor;
			text.fontSize = m_FontSize * fontSizeMultiplier;
			text.characterSpacing = m_CharacterSpacing;
			text.lineSpacing = m_LineSpacing;
			text.margin = new Vector4(m_Margins.left, m_Margins.right, m_Margins.top, m_Margins.bottom);
		}
	}

	[SerializeField]
	internal CanvasGroup m_CueGroup;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	private Action m_DestroyAction;

	[SerializeField]
	private BookEventCueStyle m_NormalText;

	[SerializeField]
	private BookEventCueStyle m_HighlightText;

	[SerializeField]
	private BookEventCueStyle m_ShadeText;

	[SerializeField]
	private BookEventCueStyle m_SpecialText;

	private DialogColors m_DialogColors;

	[Header("First letter")]
	[SerializeField]
	private TMP_FontAsset m_FirstLetterFont;

	[SerializeField]
	private Material m_FirstLetterFontMaterial;

	[SerializeField]
	private Color m_FirstLetterColor = Color.black;

	[SerializeField]
	private int m_FirstLetterSize = 170;

	[SerializeField]
	private int m_FirstLetterVOffset;

	public TextMeshProUGUI Text => m_Text;

	public List<SkillCheckResult> SkillChecks => base.ViewModel?.SkillChecks;

	public BlueprintScriptableObject Blueprint => base.ViewModel?.BlueprintCue;

	public void Initialize(Action destroyAction, DialogColors dialogColors)
	{
		m_DestroyAction = destroyAction;
		m_DialogColors = dialogColors;
	}

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		m_CueGroup.alpha = 0f;
		string mechanicText = base.ViewModel.GetMechanicText(m_DialogColors);
		string text = base.ViewModel.GetNarrativeText(m_DialogColors);
		if (!base.ViewModel.IsSpecial)
		{
			text = UIUtility.GetBookFormat(text, m_FirstLetterFont, m_FirstLetterColor, m_FirstLetterSize, m_FirstLetterVOffset, m_FirstLetterFontMaterial);
		}
		SetText(mechanicText + " " + text);
		AddDisposable(m_Text.SetLinkTooltip(null, base.ViewModel.SkillChecks, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		m_DestroyAction?.Invoke();
	}

	public void SetText(string text)
	{
		m_Text.text = text;
		CueVM viewModel = base.ViewModel;
		((viewModel != null && viewModel.IsSpecial) ? m_SpecialText : m_NormalText).ApplyStyleTo(m_Text, SettingsRoot.Accessiability.FontSizeMultiplier);
	}

	public void Highlight()
	{
		m_HighlightText.ApplyStyleTo(m_Text, SettingsRoot.Accessiability.FontSizeMultiplier);
	}

	public void Shade()
	{
		m_ShadeText.ApplyStyleTo(m_Text, SettingsRoot.Accessiability.FontSizeMultiplier);
	}
}
