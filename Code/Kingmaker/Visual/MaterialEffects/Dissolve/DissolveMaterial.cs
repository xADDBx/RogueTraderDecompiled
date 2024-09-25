using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.Dissolve;

public struct DissolveMaterial
{
	public struct Snapshot
	{
		public int RenderQueue;

		public string RenderTypeTag;

		public bool IsDissolveEnabled;

		public Texture DissolveMap;

		public Vector4 DissolveMap_ST;

		public float Dissolve;

		public float DissolveWidth;

		public Color DissolveColor;

		public float DissolveColorScale;

		public float DissolveCutout;

		public float DissolveEmission;
	}

	private struct ValueCache
	{
		public int? RenderQueue;

		public string? RenderTypeTag;

		public bool? IsDissolveEnabled;

		public Texture? DissolveMap;

		public Vector4? DissolveMap_ST;

		public float? Dissolve;

		public float? DissolveWidth;

		public Color? DissolveColor;

		public float? DissolveColorScale;

		public float? DissolveCutout;

		public float? DissolveEmission;
	}

	private const string RENDER_TYPE_TAG_NAME = "RenderType";

	private const string DISSOLVE_ON = "DISSOLVE_ON";

	private static readonly int _DissolveEnabled = Shader.PropertyToID("_DissolveEnabled");

	private static readonly int _DissolveMap = Shader.PropertyToID("_DissolveMap");

	private static readonly int _DissolveMap_ST = Shader.PropertyToID("_DissolveMap_ST");

	private static readonly int _Dissolve = Shader.PropertyToID("_Dissolve");

	private static readonly int _DissolveWidth = Shader.PropertyToID("_DissolveWidth");

	private static readonly int _DissolveColor = Shader.PropertyToID("_DissolveColor");

	private static readonly int _DissolveColorScale = Shader.PropertyToID("_DissolveColorScale");

	private static readonly int _DissolveCutout = Shader.PropertyToID("_DissolveCutout");

	private static readonly int _DissolveEmission = Shader.PropertyToID("_DissolveEmission");

	private static readonly Dictionary<Shader, bool> s_ShadersCompatibilityCache = new Dictionary<Shader, bool>();

	private readonly Material m_Material;

	private ValueCache m_Cache;

	public bool IsDissolveEnabled
	{
		get
		{
			return m_Material.IsKeywordEnabled("DISSOLVE_ON");
		}
		set
		{
			if (value)
			{
				m_Material.EnableKeyword("DISSOLVE_ON");
				m_Material.SetFloat(_DissolveEnabled, 1f);
			}
			else
			{
				m_Material.DisableKeyword("DISSOLVE_ON");
				m_Material.SetFloat(_DissolveEnabled, 0f);
			}
		}
	}

	public string RenderTypeTag
	{
		get
		{
			return m_Material.GetTag("RenderType", searchFallbacks: false, null);
		}
		set
		{
			if (m_Cache.RenderTypeTag == null || m_Cache.RenderTypeTag != value)
			{
				m_Cache.RenderTypeTag = value;
				m_Material.SetOverrideTag("RenderType", value);
			}
		}
	}

	public int RenderQueue
	{
		get
		{
			return m_Material.renderQueue;
		}
		set
		{
			if (!m_Cache.RenderQueue.HasValue || m_Cache.RenderQueue != value)
			{
				m_Cache.RenderQueue = value;
				m_Material.renderQueue = value;
			}
		}
	}

	public Texture DissolveMap
	{
		get
		{
			return m_Material.GetTexture(_DissolveMap);
		}
		set
		{
			if (m_Cache.DissolveMap == null || m_Cache.DissolveMap != value)
			{
				m_Cache.DissolveMap = value;
				m_Material.SetTexture(_DissolveMap, value);
			}
		}
	}

	public Vector4 DissolveMap_ST
	{
		get
		{
			return m_Material.GetVector(_DissolveMap_ST);
		}
		set
		{
			if (!m_Cache.DissolveMap_ST.HasValue || m_Cache.DissolveMap_ST != value)
			{
				m_Cache.DissolveMap_ST = value;
				m_Material.SetVector(_DissolveMap_ST, value);
			}
		}
	}

	public float Dissolve
	{
		get
		{
			return m_Material.GetFloat(_Dissolve);
		}
		set
		{
			if (!m_Cache.Dissolve.HasValue || m_Cache.Dissolve != value)
			{
				m_Cache.Dissolve = value;
				m_Material.SetFloat(_Dissolve, value);
			}
		}
	}

	public float DissolveWidth
	{
		get
		{
			return m_Material.GetFloat(_DissolveWidth);
		}
		set
		{
			if (!m_Cache.DissolveWidth.HasValue || m_Cache.DissolveWidth != value)
			{
				m_Cache.DissolveWidth = value;
				m_Material.SetFloat(_DissolveWidth, value);
			}
		}
	}

	public Color DissolveColor
	{
		get
		{
			return m_Material.GetColor(_DissolveColor);
		}
		set
		{
			if (!m_Cache.DissolveColor.HasValue || m_Cache.DissolveColor != value)
			{
				m_Cache.DissolveColor = value;
				m_Material.SetColor(_DissolveColor, value);
			}
		}
	}

	public float DissolveColorScale
	{
		get
		{
			return m_Material.GetFloat(_DissolveColorScale);
		}
		set
		{
			if (!m_Cache.DissolveColorScale.HasValue || m_Cache.DissolveColorScale != value)
			{
				m_Cache.DissolveColorScale = value;
				m_Material.SetFloat(_DissolveColorScale, value);
			}
		}
	}

	public float DissolveCutout
	{
		get
		{
			return m_Material.GetFloat(_DissolveCutout);
		}
		set
		{
			if (!m_Cache.DissolveCutout.HasValue || m_Cache.DissolveCutout != value)
			{
				m_Cache.DissolveCutout = value;
				m_Material.SetFloat(_DissolveCutout, value);
			}
		}
	}

	public float DissolveEmission
	{
		get
		{
			return m_Material.GetFloat(_DissolveEmission);
		}
		set
		{
			if (!m_Cache.DissolveEmission.HasValue || m_Cache.DissolveEmission != value)
			{
				m_Cache.DissolveEmission = value;
				m_Material.SetFloat(_DissolveEmission, value);
			}
		}
	}

	public DissolveMaterial(Material material)
	{
		m_Material = material;
		m_Cache = default(ValueCache);
	}

	public static bool IsMaterialCompatible(Material material)
	{
		Shader shader = material.shader;
		if (!s_ShadersCompatibilityCache.TryGetValue(shader, out var value))
		{
			value = material.shader.keywordSpace.FindKeyword("DISSOLVE_ON").isValid && material.HasFloat(_DissolveEnabled) && material.HasTexture(_DissolveMap) && material.HasVector(_DissolveMap_ST) && material.HasFloat(_Dissolve) && material.HasFloat(_DissolveWidth) && material.HasColor(_DissolveColor) && material.HasFloat(_DissolveColorScale) && material.HasFloat(_DissolveCutout) && material.HasFloat(_DissolveEmission);
			s_ShadersCompatibilityCache.Add(shader, value);
		}
		return value;
	}

	public Snapshot TakeSnapshot()
	{
		Snapshot result = default(Snapshot);
		result.RenderQueue = RenderQueue;
		result.RenderTypeTag = RenderTypeTag;
		result.IsDissolveEnabled = IsDissolveEnabled;
		result.DissolveMap = DissolveMap;
		result.DissolveMap_ST = DissolveMap_ST;
		result.Dissolve = Dissolve;
		result.DissolveWidth = DissolveWidth;
		result.DissolveColor = DissolveColor;
		result.DissolveColorScale = DissolveColorScale;
		result.DissolveCutout = DissolveCutout;
		result.DissolveEmission = DissolveEmission;
		return result;
	}

	public void ApplySnapshot(in Snapshot snapshot)
	{
		RenderQueue = snapshot.RenderQueue;
		RenderTypeTag = snapshot.RenderTypeTag;
		IsDissolveEnabled = snapshot.IsDissolveEnabled;
		DissolveMap = snapshot.DissolveMap;
		DissolveMap_ST = snapshot.DissolveMap_ST;
		Dissolve = snapshot.Dissolve;
		DissolveWidth = snapshot.DissolveWidth;
		DissolveColor = snapshot.DissolveColor;
		DissolveColorScale = snapshot.DissolveColorScale;
		DissolveCutout = snapshot.DissolveCutout;
		DissolveEmission = snapshot.DissolveEmission;
	}
}
