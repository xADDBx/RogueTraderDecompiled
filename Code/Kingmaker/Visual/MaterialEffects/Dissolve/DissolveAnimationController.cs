using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.Dissolve;

[Serializable]
public class DissolveAnimationController
{
	private class Entry
	{
		public DissolveMaterial Material;

		public DissolveMaterial.Snapshot Snapshot;
	}

	private bool m_Resetted;

	private DissolveSettings m_LastActiveAnimation;

	private readonly List<Entry> m_Entries = new List<Entry>();

	public List<DissolveSettings> Animations = new List<DissolveSettings>();

	internal void InvalidateCache()
	{
		m_LastActiveAnimation = null;
	}

	public void AddMaterial(DissolveMaterial material)
	{
		m_Entries.Add(new Entry
		{
			Material = material,
			Snapshot = material.TakeSnapshot()
		});
	}

	public void ClearMaterials()
	{
		m_Entries.Clear();
	}

	public void UpdateMaterialProperties()
	{
		for (int i = 0; i < m_Entries.Count; i++)
		{
			Entry entry = m_Entries[i];
			entry.Snapshot.DissolveMap = entry.Material.DissolveMap;
			m_Entries[i] = entry;
		}
	}

	internal void Update()
	{
		DissolveSettings dissolveSettings = null;
		int num = Animations.Count;
		for (int i = 0; i < num; i++)
		{
			DissolveSettings dissolveSettings2 = Animations[i];
			if (!dissolveSettings2.IsFinished)
			{
				UpdateAnimation(dissolveSettings2);
			}
			if (dissolveSettings2.IsFinished)
			{
				int num2 = num - 1;
				if (num2 != i)
				{
					List<DissolveSettings> animations = Animations;
					int index = num2;
					List<DissolveSettings> animations2 = Animations;
					int index2 = i;
					DissolveSettings dissolveSettings3 = Animations[i];
					DissolveSettings dissolveSettings4 = Animations[num2];
					DissolveSettings dissolveSettings6 = (animations[index] = dissolveSettings3);
					dissolveSettings6 = (animations2[index2] = dissolveSettings4);
				}
				num--;
				i--;
			}
			else if (!dissolveSettings2.IsDelayed && (dissolveSettings == null || dissolveSettings2.Layer >= dissolveSettings.Layer))
			{
				dissolveSettings = dissolveSettings2;
			}
		}
		Animations.RemoveRange(num, Animations.Count - num);
		if (dissolveSettings != null)
		{
			if (dissolveSettings != m_LastActiveAnimation)
			{
				foreach (Entry entry in m_Entries)
				{
					entry.Material.IsDissolveEnabled = true;
					if (dissolveSettings.DissolveCutout)
					{
						entry.Material.RenderTypeTag = "TransparentCutout";
						entry.Material.RenderQueue = 2450;
					}
					else
					{
						entry.Material.RenderTypeTag = entry.Snapshot.RenderTypeTag;
						entry.Material.RenderQueue = entry.Snapshot.RenderQueue;
					}
					entry.Material.DissolveMap = dissolveSettings.Texture;
					entry.Material.DissolveColorScale = dissolveSettings.HdrColorScale;
					entry.Material.DissolveCutout = (dissolveSettings.DissolveCutout ? 1 : 0);
					entry.Material.DissolveEmission = (dissolveSettings.DissolveEmission ? 1 : 0);
				}
				m_Resetted = false;
			}
			foreach (Entry entry2 in m_Entries)
			{
				Vector2 b = dissolveSettings.TilingScale;
				if (!dissolveSettings.TilingOverride)
				{
					b = Vector2.Scale(new Vector2(entry2.Snapshot.DissolveMap_ST.x, entry2.Snapshot.DissolveMap_ST.y), b);
				}
				Vector2 vector = new Vector2(entry2.Snapshot.DissolveMap_ST.z, entry2.Snapshot.DissolveMap_ST.w) + dissolveSettings.CurrentTextureOffset;
				entry2.Material.DissolveMap_ST = new Vector4(b.x, b.y, vector.x, vector.y);
				entry2.Material.Dissolve = dissolveSettings.CurrentDissolve;
				entry2.Material.DissolveWidth = dissolveSettings.CurrentDissolveWidth;
				entry2.Material.DissolveColor = dissolveSettings.CurrentColor;
			}
		}
		else if (!m_Resetted)
		{
			m_Resetted = true;
			RevertToDefaults();
		}
		m_LastActiveAnimation = dissolveSettings;
	}

	public void RevertToDefaults()
	{
		foreach (Entry entry in m_Entries)
		{
			entry.Material.ApplySnapshot(in entry.Snapshot);
		}
	}

	private void UpdateAnimation(DissolveSettings animation)
	{
		float num = (animation.UseUnscaledTime ? Time.unscaledTime : Time.time);
		float num2 = (animation.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
		if (!animation.IsStarted)
		{
			animation.IsStarted = true;
			animation.StartTime = num;
		}
		if (animation.Lifetime <= 0f || (!animation.LoopAnimation && animation.NormalizedTime > 1f))
		{
			animation.IsFinished = true;
			return;
		}
		animation.NormalizedTime = (num - animation.StartTime - animation.Delay) / animation.Lifetime;
		if (!animation.IsDelayed)
		{
			if (animation.LoopAnimation)
			{
				animation.NormalizedTime -= Mathf.Floor(animation.NormalizedTime);
			}
			float num3 = (animation.PlayBackwards ? (1f - animation.NormalizedTime) : animation.NormalizedTime);
			animation.CurrentColor = animation.ColorOverLifetime.Evaluate(num3);
			animation.CurrentDissolve = animation.DissolveOverLifetime.EvaluateNormalized(num3) * animation.FadeOut;
			animation.CurrentDissolveWidth = animation.DissolveWidthOverLifetime.EvaluateNormalized(num3) * animation.DissolveWidthScale;
			animation.CurrentTextureOffset += animation.OffsetSpeed * num2 * ((!animation.PlayBackwards) ? 1 : (-1));
		}
	}
}
