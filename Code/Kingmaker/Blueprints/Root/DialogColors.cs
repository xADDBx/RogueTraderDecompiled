using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class DialogColors
{
	[Header("Dialogue")]
	public Color NameColorMultiplyer;

	public Color32 Narrator;

	public Color32 SelectedAnswer;

	public Color32 FocusAnswer = new Color32(byte.MaxValue, byte.MaxValue, 132, byte.MaxValue);

	public Color32 FocusDisableAnswer = new Color32(byte.MaxValue, byte.MaxValue, 132, 123);

	public Color32 NormalAnswer;

	public Color32 DisabledAnswer;

	[Header("SoulMark Shift")]
	public Color32 SoulMarkShiftBePositive = new Color32(47, 76, 15, byte.MaxValue);

	public Color32 SoulMarkShiftBeNegative = new Color32(79, 26, 26, byte.MaxValue);

	[Header("Skillcheck")]
	public Color32 SkillCheckSuccessfulDialogue;

	public Color32 SkillCheckFailedDialogue;

	public Color32 SkillCheckSuccessfulBE;

	public Color32 SkillCheckFailedBE;
}
