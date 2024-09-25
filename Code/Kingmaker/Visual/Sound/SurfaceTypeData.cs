using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.HitSystem;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class SurfaceTypeData : ScriptableObject, ISerializationCallbackReceiver
{
	[Serializable]
	public class Entry
	{
		public string SoundSwitch;

		public string[] MaskNames;
	}

	[Serializable]
	public class SurfaceDebugInfo
	{
		public SurfaceType SoundSwitch;

		public Color GizmoColor;
	}

	[Serializable]
	public class Setting
	{
		public BlueprintArea.SettingType Type;

		public string[] SortedMaskNames;
	}

	public Entry[] Types;

	public string[] SortedMaskNames;

	public Setting[] Settings;

	[SerializeField]
	private SurfaceDebugInfo[] m_SurfaceDebugInfos;

	public readonly Dictionary<string, byte> MaskToSurfaceType = new Dictionary<string, byte>();

	public readonly Dictionary<SurfaceType, SurfaceDebugInfo> SurfaceDebugInfos = new Dictionary<SurfaceType, SurfaceDebugInfo>();

	public void OnEnable()
	{
		MaskToSurfaceType.Clear();
		for (int i = 0; i < Types.Length; i++)
		{
			string[] array = Types[i].MaskNames.EmptyIfNull();
			foreach (string key in array)
			{
				MaskToSurfaceType[key] = (byte)i;
			}
		}
	}

	public int GetMaskIndex(BlueprintArea.SettingType setting, string maskTextureName)
	{
		int num = SettingIndex(setting);
		if (num < 0)
		{
			return -1;
		}
		Setting setting2 = Settings[num];
		for (int i = 0; i < setting2.SortedMaskNames.Length; i++)
		{
			string value = setting2.SortedMaskNames[i];
			if (maskTextureName.EndsWith(value))
			{
				return i;
			}
		}
		return -1;
	}

	public int SettingIndex(BlueprintArea.SettingType setting)
	{
		for (int i = 0; i < Settings.Length; i++)
		{
			if (Settings[i].Type == setting)
			{
				return i;
			}
		}
		return -1;
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (m_SurfaceDebugInfos != null)
		{
			SurfaceDebugInfos.Clear();
			SurfaceDebugInfo[] surfaceDebugInfos = m_SurfaceDebugInfos;
			foreach (SurfaceDebugInfo surfaceDebugInfo in surfaceDebugInfos)
			{
				SurfaceDebugInfos[surfaceDebugInfo.SoundSwitch] = surfaceDebugInfo;
			}
		}
	}
}
