using System;
using System.Collections.Generic;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Settings.Graphics;

public class TexturesQualityController : MonoSingleton<TexturesQualityController>
{
	[Serializable]
	public class Settings
	{
		[Tooltip("MipMap groups from Project Quality Settings Mipmap Limit Groups to use in automatic mip limit control. Groups will be processed in the order of the list.")]
		public List<string> MipMapGroups;

		[Tooltip("Maximum mip limit bias for each group.")]
		[Range(1f, 8f)]
		public int MaxMipmapLimit = 2;

		[Tooltip("If available system memory is lower than this value, mip limit bias will be increased (no higher than MaxMipmapLimit).")]
		public int MipLimitIncreaseThresholdMb = 500;

		[Tooltip("If available system memory is higher than this value, mip limit bias will be decreased (no lower than 0).")]
		public int MipLimitDecreaseThresholdMb = 1500;

		[Tooltip("Minimum time to wait to increase mip level after the last mip level change.")]
		public float MipLimitIncreaseTimeout = 1f;

		[Tooltip("Minimum time to wait to decrease mip level after the last mip level change.")]
		public float MipLimitDecreaseTimeout = 1f;
	}

	private readonly struct MemoryUsageStatus
	{
		public readonly int TotalMemoryMb;

		public readonly int MemoryUsedMb;

		public int MemoryLeftMb => TotalMemoryMb - MemoryUsedMb;

		public MemoryUsageStatus(long totalMemory, long memoryUsed)
		{
			TotalMemoryMb = (int)(totalMemory / 1048576);
			MemoryUsedMb = (int)(memoryUsed / 1048576);
		}
	}

	public class TexturesMipmapLevelController
	{
		private int m_CurrentMipmapLimit;

		private int m_LastChangedGroup = -1;

		private float m_LastChangeTime = -1f;

		private static int MaxMipmapLimit => ControllerSettings.MaxMipmapLimit;

		private static IReadOnlyList<string> MipmapLimitGroups => ControllerSettings.MipMapGroups;

		public bool CanIncreaseMipLimit
		{
			get
			{
				if (m_LastChangedGroup < MipmapLimitGroups.Count - 1 || m_CurrentMipmapLimit < MaxMipmapLimit)
				{
					return !IsWaitingForTimeout(ControllerSettings.MipLimitIncreaseTimeout);
				}
				return false;
			}
		}

		public bool CanDecreaseMipLimit
		{
			get
			{
				if (m_CurrentMipmapLimit > 0)
				{
					return !IsWaitingForTimeout(ControllerSettings.MipLimitDecreaseTimeout);
				}
				return false;
			}
		}

		private bool IsWaitingForTimeout(float timeout)
		{
			if (m_LastChangeTime >= 0f)
			{
				return Time.realtimeSinceStartup < m_LastChangeTime + timeout;
			}
			return false;
		}

		public void IncreaseMipLimit()
		{
			if (m_LastChangedGroup >= MipmapLimitGroups.Count - 1 && m_CurrentMipmapLimit >= MaxMipmapLimit)
			{
				Logger.Warning($"Can't increase mipmap limit, permitted maximum of {MaxMipmapLimit} reached");
				return;
			}
			if (m_LastChangedGroup < 0 || m_LastChangedGroup == MipmapLimitGroups.Count - 1)
			{
				m_CurrentMipmapLimit++;
			}
			m_LastChangedGroup = (m_LastChangedGroup + 1) % MipmapLimitGroups.Count;
			SetMipLimitBiasForGroup(MipmapLimitGroups[m_LastChangedGroup], m_CurrentMipmapLimit);
			m_LastChangeTime = Time.realtimeSinceStartup;
		}

		public void DecreaseMipLimit()
		{
			if (m_CurrentMipmapLimit == 0)
			{
				Logger.Warning("Can't decrease mipmap limit, already at 0");
				return;
			}
			SetMipLimitBiasForGroup(MipmapLimitGroups[m_LastChangedGroup], m_CurrentMipmapLimit - 1);
			if (m_LastChangedGroup == 0)
			{
				m_CurrentMipmapLimit--;
			}
			m_LastChangedGroup = (m_LastChangedGroup + MipmapLimitGroups.Count - 1) % MipmapLimitGroups.Count;
			m_LastChangeTime = Time.realtimeSinceStartup;
		}

		[Cheat(Name = "set_mipmap_level", ExecutionPolicy = ExecutionPolicy.PlayMode)]
		public static void CheatSetMipMapLevel(int level, [CanBeNull] string groupName = null)
		{
			if (groupName != null && TextureMipmapLimitGroups.HasGroup(groupName))
			{
				SetMipLimitBiasForGroup(groupName, level);
			}
			else
			{
				QualitySettings.globalTextureMipmapLimit = level;
			}
		}

		private static void SetMipLimitBiasForGroup(string groupName, int mipmapLevel)
		{
			if (string.IsNullOrEmpty(groupName) || !TextureMipmapLimitGroups.HasGroup(groupName))
			{
				Logger.Warning("Failed to set mipmap level for group \"" + (groupName ?? "<null>") + "\"");
				return;
			}
			TextureMipmapLimitSettings textureMipmapLimitSettings = QualitySettings.GetTextureMipmapLimitSettings(groupName);
			textureMipmapLimitSettings.limitBiasMode = TextureMipmapLimitBiasMode.OverrideGlobalLimit;
			textureMipmapLimitSettings.limitBias = mipmapLevel;
			QualitySettings.SetTextureMipmapLimitSettings(groupName, textureMipmapLimitSettings);
			Logger.Log($"Set mipmap level for group \"{groupName}\" to {mipmapLevel}");
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("TexturesQualityController");

	private readonly TexturesMipmapLevelController m_TexturesMipmapLevelController = new TexturesMipmapLevelController();

	private static Settings ControllerSettings => BlueprintRoot.Instance.SettingsValues.TexturesQualityControllerSettings;

	private static MemoryUsageStatus GetMemoryUsingStatus()
	{
		return new MemoryUsageStatus(MemoryUsageHelper.Stats.SystemMemoryLimit, MemoryUsageHelper.Stats.SystemMemoryUsed);
	}

	private static bool ShouldIncreaseMipLimit(MemoryUsageStatus memoryUsageStatus)
	{
		return memoryUsageStatus.MemoryLeftMb < ControllerSettings.MipLimitIncreaseThresholdMb;
	}

	private static bool ShouldDecreaseMipLimit(MemoryUsageStatus memoryUsageStatus)
	{
		return memoryUsageStatus.MemoryLeftMb > ControllerSettings.MipLimitDecreaseThresholdMb;
	}

	public static void CreateInstance()
	{
		_ = MonoSingleton<TexturesQualityController>.Instance;
	}

	public void Update()
	{
		if (BuildModeUtility.EnableTextureQualityLoweringToReduceMemoryUsage && BlueprintRoot.Instance != null)
		{
			MemoryUsageStatus memoryUsingStatus = GetMemoryUsingStatus();
			if (ShouldIncreaseMipLimit(memoryUsingStatus) && m_TexturesMipmapLevelController.CanIncreaseMipLimit)
			{
				Logger.Log($"Memory usage increased to {memoryUsingStatus.MemoryUsedMb} Mb/{memoryUsingStatus.TotalMemoryMb} Mb ({memoryUsingStatus.MemoryLeftMb} Mb left), increase mipmap limit");
				m_TexturesMipmapLevelController.IncreaseMipLimit();
			}
			else if (ShouldDecreaseMipLimit(memoryUsingStatus) && m_TexturesMipmapLevelController.CanDecreaseMipLimit)
			{
				Logger.Log($"Memory usage dropped to {memoryUsingStatus.MemoryUsedMb} Mb/{memoryUsingStatus.TotalMemoryMb} Mb ({memoryUsingStatus.MemoryLeftMb} Mb left), decrease mipmap limit");
				m_TexturesMipmapLevelController.DecreaseMipLimit();
			}
		}
	}

	[Cheat(Name = "enable_texture_quality_lowering_to_reduce_memory_usage")]
	public static void EnableTextureQualityLoweringToReduceMemoryUsage()
	{
		CreateInstance();
		BuildModeUtility.Data.EnableTextureQualityLoweringToReduceMemoryUsage = true;
	}

	[Cheat(Name = "disable_texture_quality_lowering_to_reduce_memory_usage")]
	public static void DisableTextureQualityLoweringToReduceMemoryUsage()
	{
		BuildModeUtility.Data.EnableTextureQualityLoweringToReduceMemoryUsage = false;
		while (MonoSingleton<TexturesQualityController>.Instance.m_TexturesMipmapLevelController.CanDecreaseMipLimit)
		{
			MonoSingleton<TexturesQualityController>.Instance.m_TexturesMipmapLevelController.DecreaseMipLimit();
		}
	}
}
