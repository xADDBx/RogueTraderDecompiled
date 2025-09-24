using System;
using System.Collections.Generic;
using Rewired.Data.Mapping;
using Rewired.Platforms.Custom;

namespace Rewired.Demos.CustomPlatform;

[Serializable]
public class CustomPlatformHardwareJoystickMapProvider : IHardwareJoystickMapCustomPlatformMapProvider
{
	[Serializable]
	public class PlatformDataSet
	{
		public CustomPlatformType platformType;

		public CustomPlatformHardwareJoystickMapPlatformDataSet dataSet;
	}

	public List<PlatformDataSet> platformJoystickDataSets;

	public HardwareJoystickMap.Platform GetPlatformMap(int customPlatformId, Guid hardwareTypeGuid)
	{
		CustomPlatformHardwareJoystickMapPlatformDataSet platformDataSet = GetPlatformDataSet(customPlatformId);
		if (platformDataSet == null)
		{
			return null;
		}
		return GetPlatformMap(platformDataSet, hardwareTypeGuid);
	}

	private CustomPlatformHardwareJoystickMapPlatformDataSet GetPlatformDataSet(int customPlatformId)
	{
		int count = platformJoystickDataSets.Count;
		for (int i = 0; i < count; i++)
		{
			if (platformJoystickDataSets[i] != null && platformJoystickDataSets[i].platformType == (CustomPlatformType)customPlatformId)
			{
				return platformJoystickDataSets[i].dataSet;
			}
		}
		return null;
	}

	private static HardwareJoystickMap.Platform GetPlatformMap(CustomPlatformHardwareJoystickMapPlatformDataSet platformDataSet, Guid hardwareTypeGuid)
	{
		if (platformDataSet == null || platformDataSet.platformMaps == null)
		{
			return null;
		}
		int count = platformDataSet.platformMaps.Count;
		for (int i = 0; i < count; i++)
		{
			if (platformDataSet.platformMaps[i] != null && platformDataSet.platformMaps[i].Matches(hardwareTypeGuid))
			{
				return platformDataSet.platformMaps[i].GetPlatformMap();
			}
		}
		return null;
	}
}
