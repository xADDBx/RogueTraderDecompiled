using Kingmaker.Enums;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Stats;

public class ModifiableValueDependent<TBaseStat> : ModifiableValue, IModifiableValueDependent, IHashable where TBaseStat : ModifiableValue
{
	private Modifier m_BaseStatBonus;

	private TBaseStat m_BaseStat;

	public TBaseStat BaseStat
	{
		get
		{
			if (m_BaseStat?.Type != base.Container.GetBaseStatType(base.Type))
			{
				return UpdateBaseStat();
			}
			return m_BaseStat;
		}
	}

	ModifiableValue IModifiableValueDependent.BaseStats => BaseStat;

	public virtual int BaseStatBonus => BaseStat.ModifiedValue;

	protected override void OnInitialize()
	{
	}

	private TBaseStat UpdateBaseStat()
	{
		m_BaseStat = base.Container.RequireBaseValue<TBaseStat>(base.Type);
		m_BaseStat.AddDependentValue(this);
		base.Container.BaseStatType = m_BaseStat.Type;
		return m_BaseStat;
	}

	protected override void UpdateInternalModifiers()
	{
		if (m_BaseStatBonus?.ModValue != BaseStatBonus)
		{
			m_BaseStatBonus?.Remove();
			m_BaseStatBonus = AddInternalModifier(BaseStatBonus, BaseStat.Type, ModifierDescriptor.BaseStatBonus);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
public class ModifiableValueDependent : ModifiableValueDependent<ModifiableValue>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
