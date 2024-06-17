using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[Serializable]
public struct Settings
{
	[Serializable]
	public struct DynamicTargetSettings
	{
		[Tooltip("Dynamic target mode")]
		public DynamicTargetMode mode;

		[Tooltip("Min size of the target if the target does not provide a custom values")]
		public Vector2 targetSize;

		[Tooltip("The distance at which the size of the target will be minimal")]
		public float distanceMin;

		[Tooltip("The distance at which the size of the target will be maximum")]
		public float distanceMax;
	}

	public enum DynamicTargetMode
	{
		Disabled,
		StraightDistance,
		ProjectedToAxesDistance
	}

	public static readonly Settings Default = new Settings
	{
		fadeDuration = 0.25f,
		fadeInDelay = 0.25f,
		targetInsideBoxOccluded = false,
		defaultTargetSize = new float2(3f, 4f),
		dynamicTargetSettings = new DynamicTargetSettings
		{
			mode = DynamicTargetMode.Disabled,
			targetSize = new float2(1f, 2f),
			distanceMin = 0f,
			distanceMax = 5f
		}
	};

	[Tooltip("Fade in/out animation duration")]
	public float fadeDuration;

	[Tooltip("Fade in animation delay after the occluder no longer overlaps the target")]
	public float fadeInDelay;

	[Tooltip("Occlusion test result if the target is inside the occluder")]
	public bool targetInsideBoxOccluded;

	[Tooltip("Size of the target if the target does not provide a custom values")]
	public Vector2 defaultTargetSize;

	public DynamicTargetSettings dynamicTargetSettings;
}
