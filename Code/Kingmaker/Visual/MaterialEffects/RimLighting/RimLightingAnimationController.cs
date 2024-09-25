using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.RimLighting;

[Serializable]
public class RimLightingAnimationController
{
	private struct Entry
	{
		public RimLightingMaterial Material;

		public RimLightingMaterial.Snapshot Snapshot;
	}

	private bool m_Resetted;

	private Stack<RimLightingAnimationSettings> m_Stack = new Stack<RimLightingAnimationSettings>();

	private readonly List<Entry> m_Entries = new List<Entry>();

	public List<RimLightingAnimationSettings> Animations = new List<RimLightingAnimationSettings>();

	public void AddMaterial(RimLightingMaterial material)
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
		for (int i = 0; i < Animations.Count; i++)
		{
			RimLightingAnimationSettings rimLightingAnimationSettings = Animations[i];
			if (!rimLightingAnimationSettings.IsFinished)
			{
				UpdateAnimation(rimLightingAnimationSettings);
			}
			if (rimLightingAnimationSettings.IsFinished)
			{
				Animations.RemoveAt(i);
				i--;
			}
			else if (!rimLightingAnimationSettings.IsDelayed)
			{
				m_Stack.Push(rimLightingAnimationSettings);
			}
		}
		if (m_Stack.Count > 0)
		{
			Color rimColor = new Color(0f, 0f, 0f, 0f);
			float num = float.MinValue;
			float a = 0f;
			while (m_Stack.Count > 0)
			{
				RimLightingAnimationSettings rimLightingAnimationSettings2 = m_Stack.Pop();
				Color currentColor = rimLightingAnimationSettings2.CurrentColor;
				a = Mathf.Max(a, currentColor.a);
				rimColor += currentColor * currentColor.a * rimLightingAnimationSettings2.CurrentIntensity;
				num = Mathf.Max(num, rimLightingAnimationSettings2.CurrentPower);
			}
			rimColor.a = a;
			foreach (Entry entry in m_Entries)
			{
				Entry current = entry;
				if (current.Material.RimLighting < 1f)
				{
					current.Material.RimLighting = 1f;
				}
				current.Material.RimColor = rimColor;
				current.Material.RimPower = num;
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

	private void UpdateAnimation(RimLightingAnimationSettings animation)
	{
		float num = (animation.UnscaledTime ? Time.unscaledTime : Time.time);
		if (!animation.IsStarted)
		{
			animation.IsStarted = true;
			animation.StartTime = num + animation.Delay;
		}
		if (animation.Lifetime <= 0f || (!animation.LoopAnimation && animation.NormalizedTime > 1f))
		{
			animation.IsFinished = true;
			return;
		}
		animation.NormalizedTime = (num - animation.StartTime) / animation.Lifetime;
		if (!animation.IsDelayed)
		{
			if (animation.LoopAnimation)
			{
				animation.NormalizedTime -= Mathf.Floor(animation.NormalizedTime);
			}
			animation.CurrentColor = animation.ColorOverLifetime.Evaluate(animation.NormalizedTime);
			animation.CurrentColor.a *= animation.FadeOut;
			animation.CurrentIntensity = EvaluateNormalized(animation.IntensityOverLifetime, animation.NormalizedTime) * animation.IntensityScale;
			animation.CurrentPower = EvaluateNormalized(animation.PowerOverLifetime, animation.NormalizedTime);
		}
	}

	private float EvaluateNormalized(AnimationCurve curve, float normalizedTime)
	{
		float time = curve.keys[curve.length - 1].time;
		return curve.Evaluate(time * normalizedTime);
	}
}
