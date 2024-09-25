using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.CustomPostProcess;

[Serializable]
[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Custom Post Process/Effect", fileName = "PostProcessEffect")]
public sealed class CustomPostProcessEffect : ScriptableObject, IEquatable<CustomPostProcessEffect>, ISerializationCallbackReceiver
{
	private static readonly object s_OriginalAssetGuidToIdMapSyncObject = new object();

	private static readonly Dictionary<string, int> s_OriginalAssetGuidToIdMap = new Dictionary<string, int> { { "", 0 } };

	[SerializeField]
	private string m_DisplayName;

	[SerializeField]
	[HideInInspector]
	private string m_OriginalAssetGuid;

	[NonSerialized]
	private int m_OriginalAssetId;

	public CustomPostProcessRenderEvent Event;

	public List<CustomPostProcessEffectPass> Passes;

	public string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(m_DisplayName))
			{
				return m_DisplayName;
			}
			return base.name;
		}
	}

	private static int GetId(string guid)
	{
		if (guid == null)
		{
			return -1;
		}
		lock (s_OriginalAssetGuidToIdMapSyncObject)
		{
			if (!s_OriginalAssetGuidToIdMap.TryGetValue(guid, out var value))
			{
				value = s_OriginalAssetGuidToIdMap.Count;
				s_OriginalAssetGuidToIdMap.Add(guid, value);
			}
			return value;
		}
	}

	public bool Validate()
	{
		for (int i = 0; i < Passes.Count; i++)
		{
			CustomPostProcessEffectPass customPostProcessEffectPass = Passes[i];
			if (customPostProcessEffectPass == null || customPostProcessEffectPass.Shader == null)
			{
				return false;
			}
		}
		return true;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		m_OriginalAssetId = GetId(m_OriginalAssetGuid);
	}

	public bool Equals(CustomPostProcessEffect other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)this == other)
		{
			return true;
		}
		bool num = this;
		bool flag = other;
		if (num && flag)
		{
			return m_OriginalAssetId == other.m_OriginalAssetId;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (this != obj)
		{
			if (obj is CustomPostProcessEffect other)
			{
				return Equals(other);
			}
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return (base.GetHashCode() * 397) ^ m_OriginalAssetId;
	}

	public static bool operator ==(CustomPostProcessEffect left, CustomPostProcessEffect right)
	{
		return object.Equals(left, right);
	}

	public static bool operator !=(CustomPostProcessEffect left, CustomPostProcessEffect right)
	{
		return !object.Equals(left, right);
	}
}
