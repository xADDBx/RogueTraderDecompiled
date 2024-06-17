using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.Utility.FlagCountable;

namespace Kingmaker.UnitLogic;

public class FeatureCountableFlag
{
	public class BuffList
	{
		public class Element
		{
			public readonly NullifyInformation.BuffInformation BuffInformation;

			private int m_Counter;

			public int Counter => m_Counter;

			public Element(Buff buff)
			{
				BuffInformation = NullifyInformation.BuffInformation.Create(buff.Blueprint);
				m_Counter++;
			}

			public void Retain()
			{
				m_Counter++;
			}

			public void Release()
			{
				m_Counter--;
			}
		}

		private readonly Dictionary<string, Element> m_Buffs = new Dictionary<string, Element>();

		public IEnumerable<Element> Buffs => m_Buffs.Values;

		public void TryAdd(Buff buff)
		{
			if (buff != null)
			{
				if (!m_Buffs.TryGetValue(buff.UniqueId, out var value))
				{
					m_Buffs.Add(buff.UniqueId, new Element(buff));
				}
				else
				{
					value.Retain();
				}
			}
		}

		public void TryRemove(Buff buff)
		{
			if (buff != null && m_Buffs.TryGetValue(buff.UniqueId, out var value))
			{
				value.Release();
				if (value.Counter <= 0)
				{
					m_Buffs.Remove(buff.UniqueId);
				}
			}
		}

		public void Clear()
		{
			m_Buffs.Clear();
		}
	}

	private readonly CountableFlag m_Flag = new CountableFlag();

	private readonly BuffList m_AssociatedBuffs = new BuffList();

	private MechanicsFeatureType m_Type;

	private AbstractUnitEntity m_Owner;

	public int Count => m_Flag.Count;

	public bool Value => m_Flag.Value;

	public MechanicsFeatureType Type => m_Type;

	public BuffList AssociatedBuffs => m_AssociatedBuffs;

	public FeatureCountableFlag(MechanicEntity owner, MechanicsFeatureType type)
	{
		m_Type = type;
		m_Owner = owner as AbstractUnitEntity;
	}

	public void Retain(Buff associatedBuff = null)
	{
		m_AssociatedBuffs.TryAdd(associatedBuff);
		m_Flag.Retain();
		EventBus.RaiseEvent((IAbstractUnitEntity)m_Owner, (Action<IUnitFeaturesHandler>)delegate(IUnitFeaturesHandler h)
		{
			h.HandleFeatureAdded(this);
		}, isCheckRuntime: true);
	}

	public void Release(Buff associatedBuff = null)
	{
		m_AssociatedBuffs.TryRemove(associatedBuff);
		m_Flag.Release();
		EventBus.RaiseEvent((IAbstractUnitEntity)m_Owner, (Action<IUnitFeaturesHandler>)delegate(IUnitFeaturesHandler h)
		{
			h.HandleFeatureRemoved(this);
		}, isCheckRuntime: true);
	}

	public void ReleaseAll()
	{
		m_AssociatedBuffs.Clear();
		m_Flag.ReleaseAll();
		EventBus.RaiseEvent((IAbstractUnitEntity)m_Owner, (Action<IUnitFeaturesHandler>)delegate(IUnitFeaturesHandler h)
		{
			h.HandleFeatureRemoved(this);
		}, isCheckRuntime: true);
	}

	public static implicit operator bool(FeatureCountableFlag flag)
	{
		if (flag != null && flag.m_Flag != null)
		{
			return flag.m_Flag.Count > 0;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{(bool)this}({m_Flag.Count})";
	}
}
