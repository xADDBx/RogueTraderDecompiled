using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Visual.MaterialEffects.CustomMaterialProperty;

internal sealed class CustomMaterialPropertyAnimationController : IDisposable
{
	private struct MaterialInfo
	{
		public Material Material;

		public float OriginalValue;
	}

	private struct ClipInfo
	{
		public int Token;

		public int Priority;

		public float StartTime;

		public float FinishTime;

		public AnimationCurve Curve;

		public float CurveDuration;
	}

	private sealed class PropertyAnimator
	{
		private string m_PropertyName;

		private int m_PropertyId;

		private readonly List<MaterialInfo> m_Materials = new List<MaterialInfo>();

		private readonly List<ClipInfo> m_Clips = new List<ClipInfo>();

		private bool m_HasPropertyOverridenValue;

		private float m_PropertyOverridenValue;

		public int PropertyId => m_PropertyId;

		public void Setup(string propertyName, int propertyId)
		{
			m_PropertyName = propertyName;
			m_PropertyId = propertyId;
		}

		public void AddMaterial(Material material)
		{
			m_Materials.Add(new MaterialInfo
			{
				Material = material,
				OriginalValue = material.GetFloat(m_PropertyId)
			});
			if (m_HasPropertyOverridenValue)
			{
				material.SetFloat(m_PropertyId, m_PropertyOverridenValue);
			}
		}

		public void ClearMaterials()
		{
			if (m_HasPropertyOverridenValue)
			{
				foreach (MaterialInfo material in m_Materials)
				{
					material.Material.SetFloat(m_PropertyId, material.OriginalValue);
				}
			}
			m_Materials.Clear();
		}

		public void AddClip(int token, CustomMaterialPropertyAnimationSetup.Clip clip)
		{
			int indexToInsert = GetIndexToInsert(clip.Priority);
			float num = Time.time + clip.Delay;
			float finishTime = num + clip.Duration;
			ClipInfo clipInfo = default(ClipInfo);
			clipInfo.Token = token;
			clipInfo.Priority = clip.Priority;
			clipInfo.StartTime = num;
			clipInfo.FinishTime = finishTime;
			clipInfo.Curve = clip.Curve;
			clipInfo.CurveDuration = clip.CurveDuration;
			ClipInfo item = clipInfo;
			m_Clips.Insert(indexToInsert, item);
		}

		public void RemoveClip(int token)
		{
			for (int num = m_Clips.Count - 1; num >= 0; num--)
			{
				if (m_Clips[num].Token == token)
				{
					m_Clips.RemoveAt(num);
				}
			}
		}

		public void Clear()
		{
			ClearMaterials();
			m_Clips.Clear();
		}

		public void Update(float time)
		{
			if (TrySample(time, out var result))
			{
				m_HasPropertyOverridenValue = true;
				m_PropertyOverridenValue = result;
				{
					foreach (MaterialInfo material in m_Materials)
					{
						material.Material.SetFloat(m_PropertyId, result);
					}
					return;
				}
			}
			if (!m_HasPropertyOverridenValue)
			{
				return;
			}
			foreach (MaterialInfo material2 in m_Materials)
			{
				material2.Material.SetFloat(m_PropertyId, material2.OriginalValue);
			}
			m_HasPropertyOverridenValue = false;
		}

		public void Dump(StringBuilder builder)
		{
			builder.Append("property: ").Append(m_PropertyName).Append(" (")
				.Append(m_PropertyId)
				.Append(")")
				.AppendLine();
			if (m_HasPropertyOverridenValue)
			{
				builder.Append("value: ").Append(m_PropertyOverridenValue).AppendLine();
			}
			else
			{
				builder.Append("value: (original)").AppendLine();
			}
			builder.Append("clips: ").Append(" (").Append(m_Clips.Count)
				.Append(")")
				.AppendLine();
			for (int i = 0; i < m_Clips.Count; i++)
			{
				ClipInfo clipInfo = m_Clips[i];
				builder.Append("- priority").Append(clipInfo.Priority).Append(", start: ")
					.Append(clipInfo.StartTime)
					.Append(", finish: ")
					.Append(clipInfo.FinishTime)
					.AppendLine();
			}
		}

		private int GetIndexToInsert(int priority)
		{
			for (int i = 0; i < m_Clips.Count; i++)
			{
				if (m_Clips[i].Priority >= priority)
				{
					return i;
				}
			}
			return m_Clips.Count;
		}

		private bool TrySample(float time, out float result)
		{
			for (int num = m_Clips.Count - 1; num >= 0; num--)
			{
				ClipInfo clipInfo = m_Clips[num];
				if (!(time < clipInfo.StartTime))
				{
					if (!(time > clipInfo.FinishTime))
					{
						float time2 = (time - clipInfo.StartTime) / clipInfo.CurveDuration;
						result = clipInfo.Curve.Evaluate(time2);
						return true;
					}
					m_Clips.RemoveAt(num);
				}
			}
			result = 0f;
			return false;
		}
	}

	private static int s_NextToken;

	private static readonly ObjectPool<PropertyAnimator> s_PropertyAnimatorsPool = new ObjectPool<PropertyAnimator>(() => new PropertyAnimator(), null, delegate(PropertyAnimator animator)
	{
		animator.Clear();
	});

	private readonly List<Material> m_Materials = new List<Material>();

	private readonly List<PropertyAnimator> m_PropertyAnimators = new List<PropertyAnimator>();

	public void Dispose()
	{
		foreach (PropertyAnimator propertyAnimator in m_PropertyAnimators)
		{
			s_PropertyAnimatorsPool.Release(propertyAnimator);
		}
		m_PropertyAnimators.Clear();
		m_Materials.Clear();
	}

	public void AddMaterial(Material material)
	{
		m_Materials.Add(material);
		foreach (PropertyAnimator propertyAnimator in m_PropertyAnimators)
		{
			if (material.HasFloat(propertyAnimator.PropertyId))
			{
				propertyAnimator.AddMaterial(material);
			}
		}
	}

	public void ClearMaterials()
	{
		foreach (PropertyAnimator propertyAnimator in m_PropertyAnimators)
		{
			propertyAnimator.ClearMaterials();
		}
	}

	public int AddAnimation(CustomMaterialPropertyAnimationSetup animationSetup)
	{
		int num = s_NextToken++;
		CustomMaterialPropertyAnimationSetup.Clip[] clips = animationSetup.Clips;
		foreach (CustomMaterialPropertyAnimationSetup.Clip clip in clips)
		{
			if (!(clip.Duration <= 0f) && !(clip.CurveDuration <= 0f) && clip.Curve != null && !string.IsNullOrEmpty(clip.PropertyName) && clip.Curve != null && clip.CurveDuration > 0f && clip.Duration > 0f)
			{
				GetOrAddPropertyAnimator(clip.PropertyName).AddClip(num, clip);
			}
		}
		return num;
	}

	public void RemoveAnimation(int token)
	{
		foreach (PropertyAnimator propertyAnimator in m_PropertyAnimators)
		{
			propertyAnimator.RemoveClip(token);
		}
	}

	public void Update()
	{
		float time = Time.time;
		foreach (PropertyAnimator propertyAnimator in m_PropertyAnimators)
		{
			propertyAnimator.Update(time);
		}
	}

	public void Dump(StringBuilder builder)
	{
		foreach (PropertyAnimator propertyAnimator in m_PropertyAnimators)
		{
			propertyAnimator.Dump(builder);
		}
	}

	private bool FindPropertyAnimator(int propertyId, out PropertyAnimator result)
	{
		foreach (PropertyAnimator propertyAnimator in m_PropertyAnimators)
		{
			if (propertyAnimator.PropertyId == propertyId)
			{
				result = propertyAnimator;
				return true;
			}
		}
		result = null;
		return false;
	}

	private PropertyAnimator GetOrAddPropertyAnimator(string propertyName)
	{
		int num = Shader.PropertyToID(propertyName);
		foreach (PropertyAnimator propertyAnimator2 in m_PropertyAnimators)
		{
			if (propertyAnimator2.PropertyId == num)
			{
				return propertyAnimator2;
			}
		}
		PropertyAnimator propertyAnimator = s_PropertyAnimatorsPool.Get();
		propertyAnimator.Setup(propertyName, num);
		foreach (Material material in m_Materials)
		{
			if (material.HasFloat(num))
			{
				propertyAnimator.AddMaterial(material);
			}
		}
		m_PropertyAnimators.Add(propertyAnimator);
		return propertyAnimator;
	}
}
