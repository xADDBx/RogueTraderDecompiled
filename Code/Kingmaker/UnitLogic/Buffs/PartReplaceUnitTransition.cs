using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs;

public class PartReplaceUnitTransition : MechanicEntityPart, IHashable
{
	[JsonProperty]
	private EntityRef<AbstractUnitEntity> m_FromUnit;

	[JsonProperty]
	private EntityRef<AbstractUnitEntity> m_ToUnit;

	public bool IsFinished { get; private set; }

	public AbstractUnitEntity FromUnit => m_FromUnit;

	public AbstractUnitEntity ToUnit => m_ToUnit;

	public bool IsFromOwner => base.Owner == FromUnit;

	public bool IsToOwner => base.Owner == ToUnit;

	public void Setup(AbstractUnitEntity from, AbstractUnitEntity to)
	{
		m_FromUnit = from;
		m_ToUnit = to;
	}

	protected override void OnHoldingStateChanged()
	{
		if (IsToOwner && base.Owner.HoldingState != null)
		{
			IsFinished = true;
			PartReplaceUnitTransition replaceUnitTransitionOptional = FromUnit.GetReplaceUnitTransitionOptional();
			if (replaceUnitTransitionOptional != null)
			{
				replaceUnitTransitionOptional.IsFinished = true;
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<AbstractUnitEntity> obj = m_FromUnit;
		Hash128 val2 = StructHasher<EntityRef<AbstractUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val2);
		EntityRef<AbstractUnitEntity> obj2 = m_ToUnit;
		Hash128 val3 = StructHasher<EntityRef<AbstractUnitEntity>>.GetHash128(ref obj2);
		result.Append(ref val3);
		return result;
	}
}
