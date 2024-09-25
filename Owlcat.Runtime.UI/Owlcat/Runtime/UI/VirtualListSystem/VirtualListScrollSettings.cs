using System;
using UnityEngine;

namespace Owlcat.Runtime.UI.VirtualListSystem;

[Serializable]
public class VirtualListScrollSettings
{
	[Header("Scroll Wheel")]
	public bool UseScrollWheel;

	public float ScrollWheelSpeed = 50f;

	public float ConsoleNavigationScrollSpeed = 50f;

	[Header("Other")]
	[Tooltip("This value reduces zone in witch element is considered visible when scrolling to it")]
	public float ScrollZoneReduction;
}
