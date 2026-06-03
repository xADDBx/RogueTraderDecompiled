using UnityEngine;

namespace Kingmaker.UI.DollRoom;

public class DollRoomPositionOverride : MonoBehaviour
{
	public float X;

	public float Y;

	public float Z;

	[Tooltip("Camera offset in DollRoom (inventory/character screen).")]
	public Vector3 CameraOffset = Vector3.zero;

	[Tooltip("Camera offset in Augmentations DollRoom. Independent from CameraOffset.")]
	public Vector3 AugmentationsCameraOffset = Vector3.zero;
}
