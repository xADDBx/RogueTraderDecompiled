using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class SpaceDialogColors
{
	[Header("Dialogue")]
	public Color32 Narrator;

	public Color32 SelectedAnswer;

	public Color32 NormalAnswer;

	public Color32 DisabledAnswer;

	[Header("Dialogue Tooltip Colors")]
	public Color32 GlossaryGlossary;

	public Color32 GlossaryDecisions;

	public Color32 GlossaryMechanics;

	public Color32 GlossaryDefault;

	[Header("Dialogue Notifications")]
	public Color32 AlignmentShiftGood;

	public Color32 AlignmentShiftNeutral;

	public Color32 AlignmentShiftEvil;

	[Header("Skillcheck")]
	public Color32 SkillCheckSuccessfulDialogue;

	public Color32 SkillCheckFailedDialogue;

	public Color32 SkillCheckSuccessfulBE;

	public Color32 SkillCheckFailedBE;
}
