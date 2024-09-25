using System;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark.Parts;

public class ShipDecalConfig : MonoBehaviour
{
	[Serializable]
	public class ShieldDecalColorSet
	{
		public Color32 ShieldColor;
	}

	[Header("Shields")]
	public ShieldDecalColorSet DefaultShieldColor;

	public ShieldDecalColorSet HighlightHitShieldColor;

	public Color32 ReinforcedShieldColor;

	[Range(0f, 1f)]
	public float MediumShieldCapacityThreshold;

	[Range(0f, 1f)]
	public float LowShieldCapacityThreshold;

	[Range(0f, 1f)]
	public float HitHighlightBlinkAlpha = 0.5f;
}
