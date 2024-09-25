using System;
using UnityEngine;

namespace Kingmaker.UI.DollRoom;

[Serializable]
public class DollRoomCameraZoomPreset
{
	public string TargetBoneName = "Head";

	public Vector3 OffsetFromHead = new Vector3(-8f, -0.15f, 0f);

	public bool CanZoom;
}
