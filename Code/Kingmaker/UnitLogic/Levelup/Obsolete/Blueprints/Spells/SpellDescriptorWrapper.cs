using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;

[Serializable]
[JsonObject(IsReference = false)]
public struct SpellDescriptorWrapper
{
	[SerializeField]
	private long m_IntValue;

	public SpellDescriptor Value => (SpellDescriptor)m_IntValue;

	public SpellDescriptorWrapper Inverted => ~Value;

	public SpellDescriptorWrapper(SpellDescriptor descriptor)
	{
		m_IntValue = (long)descriptor;
	}

	public bool Equals(SpellDescriptorWrapper other)
	{
		return m_IntValue == other.m_IntValue;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is SpellDescriptorWrapper)
		{
			return Equals((SpellDescriptorWrapper)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_IntValue.GetHashCode();
	}

	public static bool operator ==(SpellDescriptorWrapper v1, SpellDescriptor v2)
	{
		return v1.Value == v2;
	}

	public static bool operator !=(SpellDescriptorWrapper v1, SpellDescriptor v2)
	{
		return !(v1 == v2);
	}

	public static implicit operator SpellDescriptorWrapper(SpellDescriptor descriptor)
	{
		return new SpellDescriptorWrapper(descriptor);
	}

	public static implicit operator SpellDescriptor(SpellDescriptorWrapper wrapper)
	{
		return wrapper.Value;
	}

	public static SpellDescriptor operator &(SpellDescriptorWrapper v1, SpellDescriptorWrapper v2)
	{
		return v1.Value & v2.Value;
	}

	public bool HasAnyFlag(SpellDescriptor descriptor)
	{
		return (Value & descriptor) != 0;
	}
}
