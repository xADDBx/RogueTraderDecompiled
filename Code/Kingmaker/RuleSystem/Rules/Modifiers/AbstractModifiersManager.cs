using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.RuleSystem.Rules.Modifiers;

public abstract class AbstractModifiersManager
{
	private static readonly ReadonlyList<Modifier> EmptyList = new ReadonlyList<Modifier>(null);

	[CanBeNull]
	private List<Modifier> m_List;

	public ReadonlyList<Modifier> List
	{
		get
		{
			if (m_List == null)
			{
				return EmptyList;
			}
			return new ReadonlyList<Modifier>(m_List);
		}
	}

	public bool Any
	{
		get
		{
			List<Modifier> list = m_List;
			if (list != null)
			{
				return list.Count > 0;
			}
			return false;
		}
	}

	public bool Empty => !Any;

	protected void GetValues(out int valAdd, out float pctAdd, out float pctMul, out int valAddExtra, out float pctMulExtra, Predicate<Modifier> filter = null)
	{
		valAdd = 0;
		pctAdd = 0f;
		pctMul = 1f;
		valAddExtra = 0;
		pctMulExtra = 1f;
		if (m_List == null)
		{
			return;
		}
		foreach (Modifier item in m_List)
		{
			if (filter == null || filter(item))
			{
				switch (item.Type)
				{
				case ModifierType.ValAdd:
					valAdd += item.Value;
					break;
				case ModifierType.PctAdd:
					pctAdd += (float)item.Value / 100f;
					break;
				case ModifierType.PctMul:
					pctMul *= (float)item.Value / 100f;
					break;
				case ModifierType.ValAdd_Extra:
					valAddExtra += item.Value;
					break;
				case ModifierType.PctMul_Extra:
					pctMulExtra *= (float)item.Value / 100f;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
	}

	public IEnumerable<Modifier> GetList(ModifierType type)
	{
		return m_List?.Where((Modifier i) => i.Type == type) ?? Enumerable.Empty<Modifier>();
	}

	protected int GetSum([CanBeNull] Predicate<Modifier> filter = null)
	{
		if (m_List == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < m_List.Count; i++)
		{
			if (filter == null || filter(m_List[i]))
			{
				num += m_List[i].Value;
			}
		}
		return num;
	}

	public bool HasModifier(Func<Modifier, bool> pred)
	{
		return m_List.HasItem(pred);
	}

	protected void Add(Modifier modifier)
	{
		if (modifier.Descriptor.IsStackable())
		{
			if (m_List == null)
			{
				m_List = new List<Modifier>();
			}
			m_List.Add(modifier);
			return;
		}
		for (int i = 0; i < List.Count; i++)
		{
			Modifier modifier2 = List[i];
			if (modifier2.Descriptor != modifier.Descriptor || modifier2.Type == modifier.Type)
			{
				continue;
			}
			if (modifier2.Value < modifier.Value)
			{
				if (m_List == null)
				{
					m_List = new List<Modifier>();
				}
				m_List[i] = modifier;
			}
			return;
		}
		if (m_List == null)
		{
			m_List = new List<Modifier>();
		}
		m_List.Add(modifier);
	}

	public void Add(ModifierType type, int value, [NotNull] EntityFact source, ModifierDescriptor descriptor = ModifierDescriptor.None)
	{
		Add(new Modifier(type, value, source, descriptor));
	}

	public void Add(ModifierType type, int value, [NotNull] UnitCommand source, StatType stat)
	{
		Add(new Modifier(type, value, stat));
	}

	public void Add(ModifierType type, int value, RulebookEvent source, ModifierDescriptor descriptor)
	{
		Add(new Modifier(type, value, descriptor));
	}

	public void Add(ModifierType type, int value, RulebookEvent source, StatType stat)
	{
		Add(new Modifier(type, value, stat));
	}

	public void Add(ModifierType type, int value, ItemEntity source, ModifierDescriptor descriptor)
	{
		Add(new Modifier(type, value, source, descriptor));
	}

	public void RemoveAll(Predicate<Modifier> match)
	{
		if (m_List != null && !m_List.Empty())
		{
			m_List.RemoveAll(match);
		}
	}

	protected void CopyFrom([CanBeNull] AbstractModifiersManager other, Predicate<Modifier> pred = null)
	{
		if (other?.m_List == null || other.m_List.Count < 1)
		{
			return;
		}
		foreach (Modifier item in other.m_List)
		{
			if (pred == null || pred(item))
			{
				Add(item);
			}
		}
	}
}
