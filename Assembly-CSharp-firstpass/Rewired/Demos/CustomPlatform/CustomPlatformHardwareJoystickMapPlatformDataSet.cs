using System;
using System.Collections.Generic;
using Rewired.Platforms.Custom;
using UnityEngine;

namespace Rewired.Demos.CustomPlatform;

[Serializable]
public class CustomPlatformHardwareJoystickMapPlatformDataSet : ScriptableObject
{
	public List<HardwareJoystickMapCustomPlatformMapSO> platformMaps;
}
