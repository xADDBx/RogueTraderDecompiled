using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.ColorTint;

[Serializable]
public class ColorTintAnimationController
{
	private struct Entry
	{
		public ColorTintMaterial Material;

		public ColorTintMaterial.Snapshot Snapshot;
	}

	private bool m_Resetted;

	private readonly List<Entry> m_Entries = new List<Entry>();

	public List<ColorTintAnimationSettings> Animations = new List<ColorTintAnimationSettings>();

	public void AddMaterial(in ColorTintMaterial material)
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

	internal void Update()
	{
		ColorTintAnimationSettings colorTintAnimationSettings = null;
		for (int i = 0; i < Animations.Count; i++)
		{
			ColorTintAnimationSettings colorTintAnimationSettings2 = Animations[i];
			if (!colorTintAnimationSettings2.IsFinished)
			{
				UpdateAnimation(colorTintAnimationSettings2);
			}
			if (colorTintAnimationSettings2.IsFinished)
			{
				Animations.RemoveAt(i);
				i--;
			}
			else if (!colorTintAnimationSettings2.IsDelayed)
			{
				colorTintAnimationSettings = colorTintAnimationSettings2;
			}
		}
		if (colorTintAnimationSettings != null)
		{
			foreach (Entry entry in m_Entries)
			{
				Entry current = entry;
				Color currentColor = colorTintAnimationSettings.CurrentColor;
				currentColor.a = 1f - currentColor.a;
				current.Material.BaseColor = currentColor;
				current.Material.BaseColorBlending = 1f;
			}
			m_Resetted = false;
		}
		else if (!m_Resetted)
		{
			m_Resetted = true;
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
	}

	private void UpdateAnimation(ColorTintAnimationSettings animation)
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
			animation.CurrentColor = animation.ColorOverLifetime.Evaluate(animation.NormalizedTime) * animation.FadeOut;
		}
	}
}
