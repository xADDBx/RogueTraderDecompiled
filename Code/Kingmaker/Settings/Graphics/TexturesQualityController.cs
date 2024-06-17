using System.Collections.Generic;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Utility;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Settings.Graphics;

public class TexturesQualityController : MonoSingleton<TexturesQualityController>
{
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
		private const int MaxMipmapLimit = 2;

		private int currentMipmapLimit;

		private int lastChangedGroup = -1;

		private List<string> m_MipmapLimitGroups = new List<string> { "art_world" };

		public bool CanDecreaseTexturesQuality()
		{
			if (lastChangedGroup + 1 >= m_MipmapLimitGroups.Count)
			{
				return currentMipmapLimit < 2;
			}
			return true;
		}

		public bool CanIncreaseTexturesQuality()
		{
			return currentMipmapLimit > 0;
		}

		public void DecreaseTexturesQuality()
		{
			if (lastChangedGroup + 1 >= m_MipmapLimitGroups.Count && currentMipmapLimit >= 2)
			{
				Logger.Warning("Can't decrease textures quality, permitted mipmap limit maximum reached");
				return;
			}
			if (lastChangedGroup + 1 == m_MipmapLimitGroups.Count)
			{
				currentMipmapLimit++;
			}
			lastChangedGroup = (lastChangedGroup + 1) % m_MipmapLimitGroups.Count;
			SetMipmapLevelForTextureMipmapLimitGroup(m_MipmapLimitGroups[lastChangedGroup], currentMipmapLimit);
		}

		public void IncreaseTexturesQuality()
		{
			if (currentMipmapLimit == 0)
			{
				Logger.Warning("Can't increase textures quality, maximum quality reached");
				return;
			}
			SetMipmapLevelForTextureMipmapLimitGroup(m_MipmapLimitGroups[lastChangedGroup], currentMipmapLimit - 1);
			if (lastChangedGroup == 0)
			{
				currentMipmapLimit--;
			}
			lastChangedGroup = (lastChangedGroup + m_MipmapLimitGroups.Count - 1) % m_MipmapLimitGroups.Count;
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

	private const int DecreaseTexturesQualityThresholdMb = 500;

	private const int IncreaseTexturesQualityThresholdMb = 1500;

	private readonly TexturesMipmapLevelController m_TexturesMipmapLevelController = new TexturesMipmapLevelController();

	private static MemoryUsageStatus GetMemoryUsingStatus()
	{
		return new MemoryUsageStatus(MemoryUsageHelper.Stats.SystemMemoryLimit, MemoryUsageHelper.Stats.SystemMemoryUsed);
	}

	private static bool ShouldDecreaseTexturesQuality(MemoryUsageStatus memoryUsageStatus)
	{
		return memoryUsageStatus.MemoryLeftMb < 500;
	}

	private static bool CanIncreaseTexturesQuality(MemoryUsageStatus memoryUsageStatus)
	{
		return memoryUsageStatus.MemoryLeftMb > 1500;
	}

	public void Update()
	{
		if (BuildModeUtility.EnableTextureQualityLoweringToReduceMemoryUsage)
		{
			MemoryUsageStatus memoryUsingStatus = GetMemoryUsingStatus();
			if (ShouldDecreaseTexturesQuality(memoryUsingStatus) && m_TexturesMipmapLevelController.CanDecreaseTexturesQuality())
			{
				Logger.Log($"Memory usage increased to {memoryUsingStatus.MemoryUsedMb} Mb/{memoryUsingStatus.TotalMemoryMb} Mb ({memoryUsingStatus.MemoryLeftMb} Mb left), decrease textures quality");
				m_TexturesMipmapLevelController.DecreaseTexturesQuality();
			}
			else if (CanIncreaseTexturesQuality(memoryUsingStatus) && m_TexturesMipmapLevelController.CanIncreaseTexturesQuality())
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
