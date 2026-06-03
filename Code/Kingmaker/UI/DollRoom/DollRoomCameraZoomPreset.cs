using System;
using UnityEngine;

namespace Kingmaker.UI.DollRoom;

[Serializable]
public class DollRoomCameraZoomPreset
{
	public string TargetBoneName = "Head";

	public Vector3 OffsetFromHead = new Vector3(-8f, -0.15f, 0f);

	public bool CanZoom;

	[Tooltip("Camera offset in DollRoom (inventory/character screen).")]
	public Vector3 CameraOffset = Vector3.zero;

	[Tooltip("Camera offset in Augmentations DollRoom. Independent from CameraOffset.")]
	public Vector3 AugmentationsCameraOffset = Vector3.zero;
}
