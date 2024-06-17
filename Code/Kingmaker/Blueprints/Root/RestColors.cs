using System;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class RestColors
{
	[Header("Colors")]
	public Color32 NormalDarkText;

	public Color32 NormalLightText;

	public Color32 AttentionText;

	public Color32 NormalColor;

	public Color32 AttentionColor;

	public Color32 HighlightColor;

	public Color32 DisableColor;

	public Color32 DisableLogTitle;

	public Color32 EnableLogTitle;

	public Color32 CampLogSuceeded;

	public Color32 CampLogFailed;

	public Color32 CampLogHighlited;

	public Color32 CampLogCriticalFailed;

	public Color32 NotificationMarkColor;
}
