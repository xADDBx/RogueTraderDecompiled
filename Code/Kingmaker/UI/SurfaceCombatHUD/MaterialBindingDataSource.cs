using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

public sealed class MaterialBindingDataSource
{
	private readonly Dictionary<IconOverrideSource, Texture2D> m_IconOverrideMap = new Dictionary<IconOverrideSource, Texture2D>();

	private readonly Dictionary<HighlightDataSource, HighlightData> m_HighlightOverrideMap = new Dictionary<HighlightDataSource, HighlightData>();

	private CombatHudMaterialRemapAsset m_MaterialRemapAsset;

	public void SetIcon(IconOverrideSource source, Texture2D value)
	{
		m_IconOverrideMap[source] = value;
	}

	public void SetHighlight(HighlightDataSource source, HighlightData value)
	{
		m_HighlightOverrideMap[source] = value;
	}

	public void SetMaterialRemap(CombatHudMaterialRemapAsset value)
	{
		m_MaterialRemapAsset = value;
	}

	public void Clear()
	{
		m_IconOverrideMap.Clear();
		m_HighlightOverrideMap.Clear();
		m_MaterialRemapAsset = null;
	}

	public Material RemapMaterial(Material material, int tag)
	{
		if (m_MaterialRemapAsset != null && m_MaterialRemapAsset.RemapMaterial(tag, out var result))
		{
			return result;
		}
		return material;
	}

	public void RemapMaterials(Material material, Material[] additionalMaterials, int tag, List<Material> results)
	{
		if (!(m_MaterialRemapAsset != null) || !m_MaterialRemapAsset.RemapMaterials(tag, results))
		{
			results.Add(material);
			if (additionalMaterials != null)
			{
				results.AddRange(additionalMaterials);
			}
		}
	}

	public MaterialOverrides GetOverrides(IconOverrideSource iconOverrideSource, HighlightDataSource highlightDataSource)
	{
		m_IconOverrideMap.TryGetValue(iconOverrideSource, out var value);
		if (!m_HighlightOverrideMap.TryGetValue(highlightDataSource, out var value2))
		{
			value2 = new HighlightData(-1f, 1f);
		}
		return new MaterialOverrides(value, value2);
	}
}
