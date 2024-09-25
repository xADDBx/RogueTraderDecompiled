using System.Collections.Generic;
using System.Linq;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartIncomingAttacksOfOpportunity : UnitPart, IHashable
{
	[JsonProperty]
	private List<AttackOfOpportunityData> m_Attacks;

	[JsonProperty(IsReference = false)]
	public Vector3 StartPosition { get; private set; }

	public IEnumerable<AttackOfOpportunityData> Attacks
	{
		get
		{
			IEnumerable<AttackOfOpportunityData> attacks = m_Attacks;
			return attacks ?? Enumerable.Empty<AttackOfOpportunityData>();
		}
	}

	public AttackOfOpportunityData? NextAttack
	{
		get
		{
			if (!m_Attacks.Empty())
			{
				return m_Attacks[0];
			}
			return null;
		}
	}

	public void SetAttacks(IEnumerable<AttackOfOpportunityData> attacks)
	{
		m_Attacks = attacks.ToList();
		StartPosition = base.Owner.Position;
	}

	public void AcceptNextAttack()
	{
		Accept(NextAttack);
	}

	private void Accept(AttackOfOpportunityData? attack)
	{
		if (attack.HasValue)
		{
			m_Attacks.Remove(attack.Value);
			if (m_Attacks.Empty())
			{
				RemoveSelf();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<AttackOfOpportunityData> attacks = m_Attacks;
		if (attacks != null)
		{
			for (int i = 0; i < attacks.Count; i++)
			{
				AttackOfOpportunityData obj = attacks[i];
				Hash128 val2 = StructHasher<AttackOfOpportunityData>.GetHash128(ref obj);
				result.Append(ref val2);
			}
		}
		Vector3 val3 = StartPosition;
		result.Append(ref val3);
		return result;
	}
}
