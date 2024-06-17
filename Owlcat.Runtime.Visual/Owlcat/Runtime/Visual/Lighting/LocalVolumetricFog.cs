using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Lighting;

[ExecuteAlways]
[AddComponentMenu("Rendering/Local Volumetric Fog")]
public class LocalVolumetricFog : MonoBehaviour
{
	public LocalVolumetricFogArtistParameters Parameters = new LocalVolumetricFogArtistParameters(Color.white, 10f, 0f);

	private Texture m_PreviousVolumeMask;

	private static HashSet<LocalVolumetricFog> s_All = new HashSet<LocalVolumetricFog>();

	public Action OnTextureUpdated;

	internal static HashSet<LocalVolumetricFog> All => s_All;

	internal void PrepareParameters(float time)
	{
		if (m_PreviousVolumeMask != Parameters.VolumeMask)
		{
			if (Parameters.VolumeMask != null)
			{
				LocalVolumetricFogManager.Instance.AddTextureIntoAtlas(Parameters.VolumeMask);
			}
			NotifyUpdatedTexure();
			m_PreviousVolumeMask = Parameters.VolumeMask;
		}
		Parameters.Update(time);
	}

	private void NotifyUpdatedTexure()
	{
		if (OnTextureUpdated != null)
		{
			OnTextureUpdated();
		}
	}

	private void OnEnable()
	{
		s_All.Add(this);
		if (LocalVolumetricFogManager.Instance != null)
		{
			LocalVolumetricFogManager.Instance.RegisterVolume(this);
		}
	}

	private void OnDisable()
	{
		s_All.Remove(this);
		if (LocalVolumetricFogManager.Instance != null)
		{
			LocalVolumetricFogManager.Instance.DeRegisterVolume(this);
		}
	}

	private void OnValidate()
	{
		Parameters.Constrain();
	}
}
