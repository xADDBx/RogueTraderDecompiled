using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class WeatherEffect
{
	public enum PositionType
	{
		Grid,
		Single
	}

	public string Name;

	public AnimationCurve EffectIntensityOverLayerIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public PositionType PositioningMode;

	public float DistanceFromCamera = 26f;

	public bool SnapToGround;

	public LayerMask SnapRaycastMask = -1;

	public bool UseBakedLocationData;

	public GameObject VisualEffectPrefab;
}
