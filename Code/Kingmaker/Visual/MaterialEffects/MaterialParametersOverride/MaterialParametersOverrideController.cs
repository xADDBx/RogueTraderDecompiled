using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.MaterialParametersOverride;

[Serializable]
public class MaterialParametersOverrideController
{
	private struct Entry
	{
		public ParametersOverrideMaterial Material;

		public ParametersOverrideMaterial.Snapshot Snapshot;
	}

	private bool m_Resetted;

	private readonly List<Entry> m_MaterialEntries = new List<Entry>();

	public List<MaterialParametersOverrideSettings> Entries = new List<MaterialParametersOverrideSettings>();

	public void AddMaterial(ParametersOverrideMaterial material)
	{
		m_MaterialEntries.Add(new Entry
		{
			Material = material,
			Snapshot = material.TakeSnapshot()
		});
	}

	public void ClearMaterials()
	{
		m_MaterialEntries.Clear();
	}

	public void UpdateMaterialProperties()
	{
		for (int i = 0; i < m_MaterialEntries.Count; i++)
		{
			Entry value = m_MaterialEntries[i];
			value.Snapshot.BaseMap = value.Material.BaseMap;
			value.Snapshot.BumpMap = value.Material.BumpMap;
			m_MaterialEntries[i] = value;
		}
	}

	internal void Update()
	{
		if (Entries.Count > 0)
		{
			int num = Entries.Count - 1;
			while (num >= 0)
			{
				MaterialParametersOverrideSettings materialParametersOverrideSettings = Entries[num];
				if (materialParametersOverrideSettings.IsDisabled)
				{
					Entries.RemoveAt(num);
					num--;
					continue;
				}
				if (!materialParametersOverrideSettings.IsActivated)
				{
					ApplyMaterialSettings(materialParametersOverrideSettings);
				}
				break;
			}
			m_Resetted = false;
		}
		else if (!m_Resetted)
		{
			m_Resetted = true;
			RevertToDefaults();
		}
	}

	private void ApplyMaterialSettings(MaterialParametersOverrideSettings settings)
	{
		foreach (Entry materialEntry in m_MaterialEntries)
		{
			Entry current = materialEntry;
			current.Material.BaseMap = settings.AlbedoMap;
			current.Material.BumpMap = settings.BumpMap;
			Vector4 baseMap_ST = ((settings.TilingType == TilingType.Albedo) ? current.Snapshot.BaseMap_ST : current.Snapshot.AdditionalAlbedoMap_ST);
			baseMap_ST.x *= settings.TilingMultiplier.x;
			baseMap_ST.y *= settings.TilingMultiplier.y;
			current.Material.BaseMap_ST = baseMap_ST;
			current.Material.NormalMapKeywordEnabled = ResolveKeywordOverride(settings.BumpOverride, current.Snapshot.NormalMapKeywordEnabled, reverse: false);
			current.Material.SpecularHighlightsOffKeywordEnabled = ResolveKeywordOverride(settings.SpecularOverride, current.Snapshot.SpecularHighlightsOffKeywordEnabled, reverse: true);
			current.Material.EmissionKeywordEnabled = ResolveKeywordOverride(settings.EmissionOverride, current.Snapshot.EmissionKeywordEnabled, reverse: false);
			if (settings.SpecularOverride == OverrideMode.On)
			{
				current.Material.Roughness = settings.Roughness;
			}
			if (settings.MetallicOverride)
			{
				current.Material.Metallic = settings.Metallic;
			}
			if (settings.EmissionOverride == OverrideMode.On)
			{
				current.Material.EmissionColorScale = settings.Emission;
			}
		}
		settings.IsActivated = true;
	}

	private static bool ResolveKeywordOverride(OverrideMode overrideMode, bool defaultValue, bool reverse)
	{
		bool result = defaultValue;
		switch (overrideMode)
		{
		case OverrideMode.Off:
			result = (reverse ? true : false);
			break;
		case OverrideMode.On:
			result = !reverse;
			break;
		case OverrideMode.DontOverride:
			result = defaultValue;
			break;
		}
		return result;
	}

	public void RevertToDefaults()
	{
		foreach (Entry materialEntry in m_MaterialEntries)
		{
			materialEntry.Material.ApplySnapshot(materialEntry.Snapshot);
		}
		foreach (MaterialParametersOverrideSettings entry in Entries)
		{
			entry.IsActivated = false;
		}
	}
}
