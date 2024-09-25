using System;
using System.Collections.Generic;
using Kingmaker.Settings;
using UnityEngine;
using UnityEngine.VFX;

namespace Kingmaker.GPUCrowd;

[ExecuteInEditMode]
public class GpuCrowd : MonoBehaviour
{
	public enum RotationSource
	{
		Locator,
		Target
	}

	public const string ObjectsCountFieldName = "ObjectsCount";

	public const string NumberOfSystemsFieldName = "NumberOfSystems";

	public const string ShadowsEnableFieldName = "Shadows Enable";

	public VisualEffect CrowdVfx;

	public List<GpuCrowdLocator> CrowdLocators = new List<GpuCrowdLocator>();

	public Texture2D PositionsTexture;

	public Texture2D RotationsTexture;

	public Texture2D ScaleTexture;

	public bool DrawGizmos = true;

	private int m_OriginalCount;

	public void Awake()
	{
		if (Application.isPlaying)
		{
			VisualEffect crowdVfx = CrowdVfx;
			if ((object)crowdVfx != null && crowdVfx.HasInt("ObjectsCount"))
			{
				m_OriginalCount = CrowdVfx.GetInt("ObjectsCount");
			}
			HandleCrowdQualityChanged(SettingsRoot.Graphics.CrowdQuality.GetValue());
			Game.Instance?.GpuCrowdController?.RegisterCrowd(this);
		}
	}

	public void OnEnable()
	{
		if (Application.isPlaying)
		{
			Game.Instance?.GpuCrowdController?.RegisterCrowd(this);
			SettingsRoot.Graphics.CrowdQuality.OnValueChanged += HandleCrowdQualityChanged;
		}
	}

	public void OnDisable()
	{
		if (Application.isPlaying)
		{
			Game.Instance?.GpuCrowdController?.UnregisterCrowd(this);
			SettingsRoot.Graphics.CrowdQuality.OnValueChanged -= HandleCrowdQualityChanged;
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			Game.Instance?.GpuCrowdController?.UnregisterCrowd(this);
		}
	}

	public void HandleCrowdQualityChanged(CrowdQualityOptions quality)
	{
		if (!(CrowdVfx == null) && m_OriginalCount > 50)
		{
			int num = quality switch
			{
				CrowdQualityOptions.Dense => 1, 
				CrowdQualityOptions.Sparse => 2, 
				CrowdQualityOptions.Minimal => 4, 
				_ => throw new NotImplementedException(), 
			};
			int @int = CrowdVfx.GetInt("ObjectsCount");
			if (m_OriginalCount / num != @int)
			{
				CrowdVfx.SetInt("ObjectsCount", m_OriginalCount / num);
				CrowdVfx.SetInt("NumberOfSystems", num);
				CrowdVfx.Reinit();
			}
		}
	}
}
