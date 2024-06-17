using System;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class PostProcessVolumeSettings
{
	public AnimationCurve WeightOverLayerIntensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float Priority;

	public VolumeProfile Profile;

	[LayerField]
	public int VolumeLayer;
}
