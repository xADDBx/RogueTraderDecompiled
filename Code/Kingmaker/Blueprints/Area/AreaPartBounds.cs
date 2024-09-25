using System;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Visual.FogOfWar;
using UnityEngine;

namespace Kingmaker.Blueprints.Area;

[CreateAssetMenu(menuName = "Blueprints/Area/Area Bounds")]
public class AreaPartBounds : ScriptableObject
{
	[SerializeField]
	private Bounds m_DefaultBounds = new Bounds(default(Vector3), new Vector3(50f, 50f, 50f));

	[SerializeField]
	private bool m_OverrideMechanicBounds;

	[SerializeField]
	[ShowIf("m_OverrideMechanicBounds")]
	private Bounds m_MechanicBounds = new Bounds(default(Vector3), new Vector3(50f, 50f, 50f));

	[SerializeField]
	private bool m_OverrideFogOfWarBounds;

	[SerializeField]
	[ShowIf("m_OverrideFogOfWarBounds")]
	private Bounds m_FogOfWarBounds = new Bounds(default(Vector3), new Vector3(50f, 50f, 50f));

	[SerializeField]
	private bool m_OverrideLocalMapBounds;

	[SerializeField]
	[ShowIf("m_OverrideLocalMapBounds")]
	private Bounds m_LocalMapBounds = new Bounds(default(Vector3), new Vector3(50f, 50f, 50f));

	[SerializeField]
	private bool m_OverrideCameraBounds;

	[SerializeField]
	private bool m_UseCameraCollidersAsBounds;

	[SerializeField]
	[ShowIf("m_OverrideCameraBounds")]
	private Bounds m_CameraBounds = new Bounds(default(Vector3), new Vector3(50f, 50f, 50f));

	[SerializeField]
	private bool m_OverrideLocationWeatherDataBounds;

	[SerializeField]
	[ShowIf("m_OverrideLocationWeatherDataBounds")]
	private Bounds m_LocationWeatherDataBounds = new Bounds(default(Vector3), new Vector3(50f, 50f, 50f));

	[SerializeField]
	private float m_ShadowFalloff = 0.15f;

	[SerializeField]
	private bool m_IsBlurEnabled = true;

	[SerializeField]
	private Texture2DLink m_StaticMaskLink;

	[SerializeField]
	private BorderSettings m_FogOfWarBorderSettings = new BorderSettings();

	[SerializeField]
	[EnumFlagsAsButtons]
	private FoWStartOptions m_FogOfWarStartOptions;

	public Bounds DefaultBounds
	{
		get
		{
			return m_DefaultBounds;
		}
		set
		{
			m_DefaultBounds = value;
		}
	}

	public bool OverrideMechanicBounds
	{
		get
		{
			return m_OverrideMechanicBounds;
		}
		set
		{
			m_OverrideMechanicBounds = value;
		}
	}

	public Bounds MechanicBounds
	{
		get
		{
			if (!m_OverrideMechanicBounds)
			{
				return m_DefaultBounds;
			}
			return m_MechanicBounds;
		}
		set
		{
			m_MechanicBounds = value;
		}
	}

	public bool OverrideFogOfWarBounds
	{
		get
		{
			return m_OverrideFogOfWarBounds;
		}
		set
		{
			m_OverrideFogOfWarBounds = value;
		}
	}

	public Bounds FogOfWarBounds
	{
		get
		{
			if (!m_OverrideFogOfWarBounds)
			{
				return m_DefaultBounds;
			}
			return m_FogOfWarBounds;
		}
		set
		{
			m_FogOfWarBounds = value;
		}
	}

	public bool OverrideLocalMapBounds
	{
		get
		{
			return m_OverrideLocalMapBounds;
		}
		set
		{
			m_OverrideLocalMapBounds = value;
		}
	}

	public Bounds LocalMapBounds
	{
		get
		{
			if (!m_OverrideLocalMapBounds)
			{
				return m_DefaultBounds;
			}
			return m_LocalMapBounds;
		}
		set
		{
			m_LocalMapBounds = value;
		}
	}

	public bool OverrideCameraBounds
	{
		get
		{
			return m_OverrideCameraBounds;
		}
		set
		{
			m_OverrideCameraBounds = value;
		}
	}

	public Bounds CameraBounds
	{
		get
		{
			if (!m_OverrideCameraBounds)
			{
				return m_DefaultBounds;
			}
			return m_CameraBounds;
		}
		set
		{
			m_CameraBounds = value;
		}
	}

	public bool UseCameraCollidersAsBounds
	{
		get
		{
			return m_UseCameraCollidersAsBounds;
		}
		set
		{
			m_UseCameraCollidersAsBounds = value;
		}
	}

	public bool OverrideBakedGroundBounds
	{
		get
		{
			return m_OverrideLocationWeatherDataBounds;
		}
		set
		{
			m_OverrideLocationWeatherDataBounds = value;
		}
	}

	public Bounds BakedGroundBounds
	{
		get
		{
			if (!m_OverrideLocationWeatherDataBounds)
			{
				return m_DefaultBounds;
			}
			return m_LocationWeatherDataBounds;
		}
		set
		{
			m_LocationWeatherDataBounds = value;
		}
	}

	public float FogOfWarShadowFalloff
	{
		get
		{
			return m_ShadowFalloff;
		}
		set
		{
			m_ShadowFalloff = value;
		}
	}

	public bool FogOfWarIsBlurEnabled
	{
		get
		{
			return m_IsBlurEnabled;
		}
		set
		{
			m_IsBlurEnabled = value;
		}
	}

	public Texture2D FogOfWarStaticMask => m_StaticMaskLink.Load();

	public BorderSettings FogOfWarBorderSettings
	{
		get
		{
			return m_FogOfWarBorderSettings;
		}
		set
		{
			m_FogOfWarBorderSettings = value;
		}
	}

	public FoWStartOptions FowStartOptions
	{
		get
		{
			return m_FogOfWarStartOptions;
		}
		set
		{
			m_FogOfWarStartOptions = value;
		}
	}

	public Bounds MaxBounds
	{
		get
		{
			float num = DefaultBounds.min.x;
			float num2 = DefaultBounds.min.y;
			float num3 = DefaultBounds.min.z;
			float num4 = DefaultBounds.max.x;
			float num5 = DefaultBounds.max.y;
			float num6 = DefaultBounds.max.z;
			if (OverrideCameraBounds)
			{
				num = Math.Min(num, CameraBounds.min.x);
				num2 = Math.Min(num2, CameraBounds.min.y);
				num3 = Math.Min(num3, CameraBounds.min.z);
				num4 = Math.Max(num4, CameraBounds.max.x);
				num5 = Math.Max(num5, CameraBounds.max.y);
				num6 = Math.Max(num6, CameraBounds.max.z);
			}
			if (OverrideLocalMapBounds)
			{
				num = Math.Min(num, LocalMapBounds.min.x);
				num2 = Math.Min(num2, LocalMapBounds.min.y);
				num3 = Math.Min(num3, LocalMapBounds.min.z);
				num4 = Math.Max(num4, LocalMapBounds.max.x);
				num5 = Math.Max(num5, LocalMapBounds.max.y);
				num6 = Math.Max(num6, LocalMapBounds.max.z);
			}
			if (OverrideFogOfWarBounds)
			{
				num = Math.Min(num, FogOfWarBounds.min.x);
				num2 = Math.Min(num2, FogOfWarBounds.min.y);
				num3 = Math.Min(num3, FogOfWarBounds.min.z);
				num4 = Math.Max(num4, FogOfWarBounds.max.x);
				num5 = Math.Max(num5, FogOfWarBounds.max.y);
				num6 = Math.Max(num6, FogOfWarBounds.max.z);
			}
			if (OverrideBakedGroundBounds)
			{
				num = Math.Min(num, BakedGroundBounds.min.x);
				num2 = Math.Min(num2, BakedGroundBounds.min.y);
				num3 = Math.Min(num3, BakedGroundBounds.min.z);
				num4 = Math.Max(num4, BakedGroundBounds.max.x);
				num5 = Math.Max(num5, BakedGroundBounds.max.y);
				num6 = Math.Max(num6, BakedGroundBounds.max.z);
			}
			Vector3 size = new Vector3(num4 - num, num5 - num2, num6 - num3);
			return new Bounds(new Vector3(num + size.x / 2f, num2 + size.y / 2f, num3 + size.z / 2f), size);
		}
	}
}
