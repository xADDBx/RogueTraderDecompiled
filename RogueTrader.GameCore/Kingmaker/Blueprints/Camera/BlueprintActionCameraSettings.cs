using System;
using Cinemachine;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Camera;

[TypeId("303f32c8f2d7514428b142628c35df0f")]
public class BlueprintActionCameraSettings : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<BlueprintActionCameraSettings>
	{
	}

	[Header("BrainBlending")]
	public CinemachineBlenderSettings BrainCustomBlends;

	[Header("Aim")]
	[Header("Cinemachine Group Composer")]
	[Tooltip("The bounding box of the targets should occupy this amount of the screen space.  1 means fill the whole screen.  0.5 means fill half the screen, etc.")]
	public float GroupFramingSize = 0.5f;

	public float FrameDamping = 1f;

	public CinemachineGroupComposer.AdjustmentMode AdjustmentMode = CinemachineGroupComposer.AdjustmentMode.DollyOnly;

	public float MinimumDistance = 10f;

	public float MaximumDistance = 1000f;

	[Tooltip("If adjusting FOV, will not set the FOV lower than this.")]
	[Range(1f, 179f)]
	public float MinimumFOW = 35f;

	[Tooltip("If adjusting FOV, will not set the FOV higher than this.")]
	[Range(1f, 179f)]
	public float MaximumFOV = 40f;

	[Header("Cinemachine Noise")]
	public NoiseSettings NoiseSettings;

	public float FrequencyGain = 0.5f;

	public float AmplitudeGain = 0.5f;
}
