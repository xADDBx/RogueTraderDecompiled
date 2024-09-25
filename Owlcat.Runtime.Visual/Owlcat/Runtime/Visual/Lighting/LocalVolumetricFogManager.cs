using System.Collections.Generic;
using Owlcat.Runtime.Visual.Utilities;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Owlcat.Runtime.Visual.Lighting;

public class LocalVolumetricFogManager
{
	public const int kMaxLocalVolumetricFogOnScreen = 512;

	internal const int kMaxCacheSize = 2000000000;

	public static readonly GraphicsFormat m_LocalVolumetricFogAtlasFormat = GraphicsFormat.R8G8B8A8_UNorm;

	private static LocalVolumetricFogManager s_Instance;

	private LocalVolumetricFogSettings m_Settings;

	private ComputeShader m_Texture3DAtlasCS;

	private Texture3DAtlas m_VolumeAtlas;

	private List<LocalVolumetricFog> m_Volumes;

	public List<LocalVolumetricFog> Volumes => m_Volumes;

	public static LocalVolumetricFogManager Instance => s_Instance;

	public Texture3DAtlas VolumeAtlas
	{
		get
		{
			if (m_VolumeAtlas == null)
			{
				int maxTexturesInAtlas = m_Settings.MaxTexturesInAtlas;
				maxTexturesInAtlas = Mathf.Clamp(maxTexturesInAtlas, 1, m_Settings.MaxLocalVolumetricFogOnScreen);
				m_VolumeAtlas = new Texture3DAtlas(m_Texture3DAtlasCS, m_LocalVolumetricFogAtlasFormat, (int)m_Settings.MaxLocalVolumetricFogSize, maxTexturesInAtlas);
				foreach (LocalVolumetricFog volume in m_Volumes)
				{
					if (volume.Parameters.VolumeMask != null)
					{
						AddTextureIntoAtlas(volume.Parameters.VolumeMask);
					}
				}
			}
			return m_VolumeAtlas;
		}
	}

	public LocalVolumetricFogManager(LocalVolumetricFogSettings settings, ComputeShader texture3DAtlasCS)
	{
		m_Settings = settings;
		m_Texture3DAtlasCS = texture3DAtlasCS;
		m_Volumes = new List<LocalVolumetricFog>();
		s_Instance = this;
		foreach (LocalVolumetricFog item in LocalVolumetricFog.All)
		{
			if (!m_Volumes.Contains(item))
			{
				RegisterVolume(item);
			}
		}
	}

	public void RegisterVolume(LocalVolumetricFog volume)
	{
		m_Volumes.Add(volume);
		if (SystemInfo.IsFormatSupported(m_LocalVolumetricFogAtlasFormat, FormatUsage.LoadStore) && volume.Parameters.VolumeMask != null && VolumeAtlas != null && VolumeAtlas.IsTextureValid(volume.Parameters.VolumeMask))
		{
			AddTextureIntoAtlas(volume.Parameters.VolumeMask);
		}
	}

	internal void AddTextureIntoAtlas(Texture volumeTexture)
	{
		if (!VolumeAtlas.AddTexture(volumeTexture))
		{
			Debug.LogError("No more space in the Local Volumetric Fog atlas, consider increasing the max Local Volumetric Fog on screen in the HDRP asset.");
		}
	}

	public void DeRegisterVolume(LocalVolumetricFog volume)
	{
		if (m_Volumes.Contains(volume))
		{
			m_Volumes.Remove(volume);
		}
		if (SystemInfo.IsFormatSupported(m_LocalVolumetricFogAtlasFormat, FormatUsage.LoadStore) && volume.Parameters.VolumeMask != null && m_VolumeAtlas != null)
		{
			VolumeAtlas.RemoveTexture(volume.Parameters.VolumeMask);
		}
	}

	public bool ContainsVolume(LocalVolumetricFog volume)
	{
		return m_Volumes.Contains(volume);
	}

	internal void ReleaseAtlas()
	{
		if (m_VolumeAtlas != null)
		{
			VolumeAtlas.Release();
			m_VolumeAtlas = null;
		}
	}
}
