using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.AdditionalAlbedo;

[Serializable]
public class AdditionalAlbedoAnimationController
{
	private struct Entry
	{
		public AdditionalAlbedoMaterial Material;

		public AdditionalAlbedoMaterial.Snapshot Snapshot;
	}

	private bool m_MaterialsModified;

	private readonly List<Entry> m_Entries = new List<Entry>();

	public List<AdditionalAlbedoSettings> Animations = new List<AdditionalAlbedoSettings>();

	public void AddMaterial(in AdditionalAlbedoMaterial material)
	{
		Entry entry = default(Entry);
		entry.Material = material;
		entry.Snapshot = material.TakeSnapshot();
		Entry item = entry;
		m_Entries.Add(item);
	}

	public void ClearMaterials()
	{
		m_Entries.Clear();
	}

	public void UpdateMaterialProperties()
	{
		for (int i = 0; i < m_Entries.Count; i++)
		{
			Entry value = m_Entries[i];
			value.Snapshot.AdditionalAlbedoMap = value.Material.AdditionalAlbedoMap;
			m_Entries[i] = value;
		}
	}

	public void Update()
	{
		AdditionalAlbedoSettings additionalAlbedoSettings = null;
		for (int i = 0; i < Animations.Count; i++)
		{
			AdditionalAlbedoSettings additionalAlbedoSettings2 = Animations[i];
			if (!additionalAlbedoSettings2.IsFinished)
			{
				UpdateAnimation(additionalAlbedoSettings2);
			}
			if (additionalAlbedoSettings2.IsFinished)
			{
				Animations.RemoveAt(i);
				i--;
			}
			else if (!additionalAlbedoSettings2.IsDelayed)
			{
				additionalAlbedoSettings = additionalAlbedoSettings2;
			}
		}
		if (additionalAlbedoSettings != null)
		{
			foreach (Entry entry in m_Entries)
			{
				Entry current = entry;
				current.Material.AdditionalAlbedoEnabled = true;
				current.Material.AdditionalAlbedoMap = additionalAlbedoSettings.Texture;
				Vector2 tilingScale = additionalAlbedoSettings.TilingScale;
				if (!additionalAlbedoSettings.TilingOverride)
				{
					tilingScale.x *= current.Snapshot.DissolveMap_ST.x;
					tilingScale.y *= current.Snapshot.DissolveMap_ST.y;
				}
				Vector2 vector = new Vector2(current.Snapshot.AdditionalAlbedoMap_ST.z, current.Snapshot.AdditionalAlbedoMap_ST.w) + additionalAlbedoSettings.CurrentTextureOffset;
				current.Material.AdditionalAlbedoMap_ST = new Vector4(tilingScale.x, tilingScale.y, vector.x, vector.y);
				current.Material.AdditionalAlbedoFactor = additionalAlbedoSettings.CurrentPetrification;
				current.Material.AdditionalAlbedoColor = additionalAlbedoSettings.Color;
				current.Material.AdditionalAlbedoColorScale = additionalAlbedoSettings.ColorScale;
				current.Material.AdditionalAlbedoAlphaScale = additionalAlbedoSettings.CurrentAlphaScale;
			}
			m_MaterialsModified = true;
		}
		else if (m_MaterialsModified)
		{
			RevertToDefaults();
		}
	}

	public void RevertToDefaults()
	{
		foreach (Entry entry in m_Entries)
		{
			Entry current = entry;
			current.Material.ApplySnapshot(in current.Snapshot);
		}
		m_MaterialsModified = false;
	}

	private void UpdateAnimation(AdditionalAlbedoSettings animation)
	{
		if (!animation.IsStarted)
		{
			animation.IsStarted = true;
			animation.StartTime = Time.time + animation.Delay;
		}
		if (animation.Lifetime <= 0f || (!animation.LoopAnimation && animation.NormalizedTime > 1f))
		{
			animation.IsFinished = true;
			return;
		}
		animation.NormalizedTime = (Time.time - animation.StartTime) / animation.Lifetime;
		if (!animation.IsDelayed)
		{
			if (animation.LoopAnimation)
			{
				animation.NormalizedTime -= Mathf.Floor(animation.NormalizedTime);
			}
			animation.CurrentPetrification = animation.FactorOverLifetime.EvaluateNormalized(animation.NormalizedTime) * animation.FadeOut;
			animation.CurrentAlphaScale = animation.AlphaScaleOverLifetime.EvaluateNormalized(animation.NormalizedTime);
			animation.CurrentTextureOffset += animation.OffsetSpeed * Time.deltaTime;
		}
	}
}
