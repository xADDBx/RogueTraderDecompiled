using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.Dialog;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;

public class DialogCuePCView : ViewBase<CueVM>, ISettingsFontSizeUIHandler, ISubscriber
{
	[SerializeField]
	internal CanvasGroup m_CueGroup;

	[SerializeField]
	private TextMeshProUGUI m_Text;

	private Action m_DestroyAction;

	[Header("Normal settings")]
	[SerializeField]
	private FontStyles m_NormalFontStyle;

	[Header("Highlight settings")]
	[SerializeField]
	private FontStyles m_HighlightFontStyle;

	[Header("Special settings")]
	[SerializeField]
	private FontStyles m_SpecialFontStyle;

	[SerializeField]
	private int m_SpecialFontSize = 17;

	[SerializeField]
	private float m_DefaultFontSize = 20f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 20f;

	private DialogColors m_DialogColors;

	private RectTransform m_CuesTooltipPlace;

	public TextMeshProUGUI Text => m_Text;

	public List<SkillCheckResult> SkillChecks => base.ViewModel.SkillChecks;

	public void Initialize(Action destroyAction, DialogColors dialogColors, RectTransform tooltipPlace = null)
	{
		m_DestroyAction = destroyAction;
		m_DialogColors = dialogColors;
		m_CuesTooltipPlace = tooltipPlace;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		base.gameObject.SetActive(value: true);
		m_CueGroup.alpha = 0f;
		string cueText = base.ViewModel.GetCueText(m_DialogColors);
		SetTextFontSize(base.ViewModel.FontSizeMultiplier);
		m_Text.text = cueText;
		m_Text.fontStyle = (base.ViewModel.IsSpecial ? m_SpecialFontStyle : m_NormalFontStyle);
		AddDisposable(m_Text.SetLinkTooltip(null, base.ViewModel.SkillChecks, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true, isEncyclopedia: false, m_CuesTooltipPlace, 0, 0, 0, new List<Vector2>
		{
			new Vector2(0.5f, 0f)
		})));
	}

	private void SetTextFontSize(float multiplier)
	{
		float fontSize = (base.ViewModel.IsSpecial ? ((float)m_SpecialFontSize) : (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize)) * multiplier;
		m_Text.fontSize = fontSize;
	}

	public void Highlight()
	{
		m_Text.fontStyle = m_HighlightFontStyle;
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		m_DestroyAction?.Invoke();
	}

	public void HandleChangeFontSizeSettings(float size)
	{
		SetTextFontSize(size);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
	}
}
