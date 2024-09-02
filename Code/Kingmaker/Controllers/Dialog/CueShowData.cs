using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using UnityEngine;

namespace Kingmaker.Controllers.Dialog;

public class CueShowData : IDialogShowData
{
	public BlueprintCue Cue;

	public List<SkillCheckResult> SkillChecks;

	public List<SoulMarkShift> SoulMarkShifts;

	public readonly string SpeakerName;

	public readonly BlueprintUnit SpeakerBlueprint;

	public CueShowData(BlueprintCue cue, IEnumerable<SkillCheckResult> skillChecks, IEnumerable<SoulMarkShift> soulMarkShifts)
	{
		Cue = cue;
		SkillChecks = skillChecks.ToList();
		SoulMarkShifts = soulMarkShifts.ToList();
	}

	public CueShowData(BlueprintCue cue, string speakerName = "", BlueprintUnit speakerBlueprint = null)
	{
		Cue = cue;
		SpeakerName = speakerName;
		SpeakerBlueprint = speakerBlueprint;
	}

	public string GetText(DialogColors colors)
	{
		if (!string.IsNullOrEmpty(SpeakerName))
		{
			string text = ColorUtility.ToHtmlStringRGB((SpeakerBlueprint?.Color ?? ((Color)colors.Narrator)) * colors.NameColorMultiplyer);
			return string.Format(DialogFormats.SpeakerFormatWithColorName, "", text, SpeakerName, Cue.DisplayText, ColorUtility.ToHtmlStringRGB(colors.Narrator));
		}
		string arg = ColorUtility.ToHtmlStringRGB(colors.Narrator);
		return string.Format(DialogFormats.NarratorsTextFormat, arg, Cue.DisplayText);
	}
}
