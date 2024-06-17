using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.FlagCountable;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class WarhammerUnitPartDisableAttack : BaseUnitPart, IHashable
{
	private readonly CountableFlag m_AttacksDisabled = new CountableFlag();

	public bool AttackDisabled => m_AttacksDisabled;

	public void RetainDisabled(EntityFact source)
	{
		m_AttacksDisabled.Retain();
	}

	public void ReleaseDisabled(EntityFact source)
	{
		m_AttacksDisabled.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
