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

		[Tooltip("If available system memory is lower than this value, mip limit bias will be increased (no higher than MaxMipmapLimit).")]
		public int MipLimitBiasIncreaseThresholdMb = 500;

		[Tooltip("If available system memory is higher than this value, mip limit bias will be decreased (no lower than 0).")]
		public int MipLimitBiasDecreaseThresholdMb = 1500;

		[Tooltip("Maximum mip limit bias for each group.")]
		[Range(1f, 8f)]
		public int MaxMipmapLimit = 2;
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

		private static int MaxMipmapLimit => ControllerSettings.MaxMipmapLimit;

		private static IReadOnlyList<string> MipmapLimitGroups => ControllerSettings.MipMapGroups;

		public bool CanDecreaseTexturesQuality()
		{
			if (m_LastChangedGroup + 1 >= MipmapLimitGroups.Count)
			{
				return m_CurrentMipmapLimit < MaxMipmapLimit;
			}
			return true;
		}

		public bool CanIncreaseTexturesQuality()
		{
			return m_CurrentMipmapLimit > 0;
		}

		public void DecreaseTexturesQuality()
		{
			if (m_LastChangedGroup + 1 >= MipmapLimitGroups.Count && m_CurrentMipmapLimit >= MaxMipmapLimit)
			{
				Logger.Warning("Can't decrease textures quality, permitted mipmap limit maximum reached");
				return;
			}
			if (m_LastChangedGroup + 1 == MipmapLimitGroups.Count)
			{
				m_CurrentMipmapLimit++;
			}
			m_LastChangedGroup = (m_LastChangedGroup + 1) % MipmapLimitGroups.Count;
			SetMipmapLevelForTextureMipmapLimitGroup(MipmapLimitGroups[m_LastChangedGroup], m_CurrentMipmapLimit);
		}

		public void IncreaseTexturesQuality()
		{
			if (m_CurrentMipmapLimit == 0)
			{
				Logger.Warning("Can't increase textures quality, maximum quality reached");
				return;
			}
			SetMipmapLevelForTextureMipmapLimitGroup(MipmapLimitGroups[m_LastChangedGroup], m_CurrentMipmapLimit - 1);
			if (m_LastChangedGroup == 0)
			{
				m_CurrentMipmapLimit--;
			}
			m_LastChangedGroup = (m_LastChangedGroup + MipmapLimitGroups.Count - 1) % MipmapLimitGroups.Count;
		}

		[Cheat(Name = "set_mipmap_level", ExecutionPolicy = ExecutionPolicy.PlayMode)]
		public static void CheatSetMipMapLevel(int level, [CanBeNull] string groupName = null)
		{
			if (groupName != null && TextureMipmapLimitGroups.HasGroup(groupName))
			{
				SetMipmapLevelForTextureMipmapLimitGroup(groupName, level);
			}
			else
			{
				QualitySettings.globalTextureMipmapLimit = level;
			}
		}

		private static void SetMipmapLevelForTextureMipmapLimitGroup(string groupName, int mipmapLevel)
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

	private static bool ShouldDecreaseTexturesQuality(MemoryUsageStatus memoryUsageStatus)
	{
		return memoryUsageStatus.MemoryLeftMb < ControllerSettings.MipLimitBiasIncreaseThresholdMb;
	}

	private static bool ShouldIncreaseTexturesQuality(MemoryUsageStatus memoryUsageStatus)
	{
		return memoryUsageStatus.MemoryLeftMb > ControllerSettings.MipLimitBiasDecreaseThresholdMb;
	}

	public void Update()
	{
		if (BuildModeUtility.EnableTextureQualityLoweringToReduceMemoryUsage && BlueprintRoot.Instance != null)
		{
			MemoryUsageStatus memoryUsingStatus = GetMemoryUsingStatus();
			if (ShouldDecreaseTexturesQuality(memoryUsingStatus) && m_TexturesMipmapLevelController.CanDecreaseTexturesQuality())
			{
				Logger.Log($"Memory usage increased to {memoryUsingStatus.MemoryUsedMb} Mb/{memoryUsingStatus.TotalMemoryMb} Mb ({memoryUsingStatus.MemoryLeftMb} Mb left), decrease textures quality");
				m_TexturesMipmapLevelController.DecreaseTexturesQuality();
			}
			else if (ShouldIncreaseTexturesQuality(memoryUsingStatus) && m_TexturesMipmapLevelController.CanIncreaseTexturesQuality())
			{
				Logger.Log($"Memory usage dropped to {memoryUsingStatus.MemoryUsedMb} Mb/{memoryUsingStatus.TotalMemoryMb} Mb ({memoryUsingStatus.MemoryLeftMb} Mb left), increase textures quality");
				m_TexturesMipmapLevelController.IncreaseTexturesQuality();
			}
		}
	}

	[Cheat(Name = "enable_texture_quality_lowering_to_reduce_memory_usage")]
	public static void EnableTextureQualityLoweringToReduceMemoryUsage()
	{
		BuildModeUtility.Data.EnableTextureQualityLoweringToReduceMemoryUsage = true;
	}

	[Cheat(Name = "disable_texture_quality_lowering_to_reduce_memory_usage")]
	public static void DisableTextureQualityLoweringToReduceMemoryUsage()
	{
		BuildModeUtility.Data.EnableTextureQualityLoweringToReduceMemoryUsage = false;
		while (MonoSingleton<TexturesQualityController>.Instance.m_TexturesMipmapLevelController.CanIncreaseTexturesQuality())
		{
			MonoSingleton<TexturesQualityController>.Instance.m_TexturesMipmapLevelController.IncreaseTexturesQuality();
		}
	}
}
