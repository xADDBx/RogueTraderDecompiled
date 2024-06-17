using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Camera;

[TypeId("6c261c9ba23740c58dcd66c6353ac59d")]
public class BlueprintCameraSettings : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<BlueprintCameraSettings>
	{
	}

	[Header("Field of view and zoom")]
	[SerializeField]
	public float FovMin = 17.5f;

	[SerializeField]
	public float FovMax = 30f;

	[SerializeField]
	public float FovDefault = 30f;

	[SerializeField]
	public float ZoomLength = 1f;

	[SerializeField]
	public float ZoomSmoothness = 5f;

	[SerializeField]
	public bool EnablePhysicalZoom;

	[SerializeField]
	public float PhysicalZoomMin;

	[SerializeField]
	public float PhysicalZoomMax = 10f;

	[Header("Movement")]
	[SerializeField]
	public float ScrollRubberBand = 9f;

	[SerializeField]
	public float ScrollScreenThreshold = 4f;

	[SerializeField]
	public float ScrollSpeed = 10f;

	[Header("Rotation")]
	[SerializeField]
	public float RotationRubberBand = 30f;

	[SerializeField]
	public float RotationSpeed = 0.25f;

	[SerializeField]
	public float RotationTime = 0.075f;

	[SerializeField]
	public float RotationRatio = 7f;

	[SerializeField]
	public bool OverrideStartRotation;

	[SerializeField]
	public Vector3 StartRotation = Vector3.zero;

	[Header("Ground Positioning")]
	[SerializeField]
	public float TimeToGround = 1f;

	[SerializeField]
	public float DistanceFromHighGround = 5f;

	[Header("Position Overrides")]
	[SerializeField]
	public bool OverrideStartPosition;

	[SerializeField]
	public Vector3 StartPosition = Vector3.zero;

	[SerializeField]
	public bool HardBindPositionEnabled;

	[SerializeField]
	public Vector3 HardBindPosition = Vector3.zero;

	[Header("Graphics")]
	[SerializeField]
	public float ShadowDistance = 100f;
}
