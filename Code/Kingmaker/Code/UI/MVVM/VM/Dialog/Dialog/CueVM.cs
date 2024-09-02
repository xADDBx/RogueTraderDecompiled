using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Dialog;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Settings;
using Kingmaker.TextTools;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;

public class CueVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly List<SkillCheckResult> SkillChecks;

	public readonly List<SoulMarkShift> SoulMarkShifts;

	public readonly bool IsSpecial;

	private readonly string m_Text;

	public BlueprintCue BlueprintCue { get; }

	public string RawText
	{
		get
		{
			if (!BlueprintCue)
			{
				return m_Text;
			}
			return BlueprintCue.DisplayText;
		}
	}

	public float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public CueVM(string cueText, IEnumerable<SkillCheckResult> skillChecks, IEnumerable<SoulMarkShift> soulMarkShifts, bool isSpecial = false)
	{
		m_Text = cueText;
		SkillChecks = skillChecks.ToList();
		SoulMarkShifts = soulMarkShifts.ToList();
		IsSpecial = isSpecial;
	}

	public CueVM(BlueprintCue cue, IEnumerable<SkillCheckResult> skillChecks, IEnumerable<SoulMarkShift> soulMarkShifts, bool isSpecial = false)
		: this(string.Empty, skillChecks, soulMarkShifts, isSpecial)
	{
		BlueprintCue = cue;
	}

	protected override void DisposeImplementation()
	{
	}

	public string GetCueText(DialogColors dialogColors)
	{
		using (NarratorStartTemplate.GetScope(dialogColors))
		{
			return GetCueTextInternal(dialogColors);
		}
	}

	private string GetCueTextInternal(DialogColors dialogColors)
	{
		DialogController dialogController = Game.Instance.DialogController;
		DialogType type = dialogController.Dialog.Type;
		if (type == DialogType.Common || type == DialogType.StarSystemEvent)
		{
			string text;
			if (dialogController.Dialog.IsNarratorText || dialogController.CurrentCue.IsNarratorText)
			{
				text = string.Format(DialogFormats.NarratorsTextFormat, ColorUtility.ToHtmlStringRGB(dialogColors.Narrator), dialogController.CurrentCue.DisplayText, UIUtility.SkillCheckText(SkillChecks, dialogColors));
			}
			else if (dialogController.CurrentSpeakerBlueprint == null)
			{
				text = string.Format(DialogFormats.SpeakerFormatWithoutName, dialogController.CurrentCue.DisplayText, UIUtility.SkillCheckText(SkillChecks, dialogColors));
			}
			else
			{
				Color color = (BlueprintCue.Speaker.ReplacedSpeakerWithErrorSpeaker ? Color.red : (dialogController.CurrentSpeakerBlueprint.Color * dialogColors.NameColorMultiplyer));
				text = string.Format(DialogFormats.SpeakerFormatWithColorName, UIUtility.SkillCheckText(SkillChecks, dialogColors), ColorUtility.ToHtmlStringRGB(color), BlueprintCue.Speaker.ReplacedSpeakerWithErrorSpeaker ? "Error Speaker" : dialogController.CurrentSpeakerName, dialogController.CurrentCue.DisplayText, ColorUtility.ToHtmlStringRGB(dialogColors.Narrator));
			}
			if (BuildModeUtility.IsShowDevComment && !string.IsNullOrEmpty(dialogController.CurrentCue.Comment))
			{
				text = text + "\n<color=#" + ColorUtility.ToHtmlStringRGB(new Color(0.19215687f, 0.23921569f, 0.8509804f)) + ">[DevComment]:" + dialogController.CurrentCue.Comment + "</color>";
			}
			return text;
		}
		string text2 = "";
		if (SoulMarkShifts.Count > 0)
		{
			text2 += UIUtility.SoulMarkShiftsText(SoulMarkShifts, dialogColors);
		}
		return text2 + UIUtility.SkillCheckText(SkillChecks, dialogColors) + " " + m_Text;
	}

	public string GetMechanicText(DialogColors dialogColors)
	{
		string text = "";
		if (SoulMarkShifts.Count > 0)
		{
			text += UIUtility.SoulMarkShiftsText(SoulMarkShifts, dialogColors);
		}
		return text + UIUtility.SkillCheckText(SkillChecks, dialogColors);
	}

	public string GetNarrativeText(DialogColors dialogColors)
	{
		using (NarratorStartTemplate.GetScope(dialogColors))
		{
			return RawText;
		}
	}
}
