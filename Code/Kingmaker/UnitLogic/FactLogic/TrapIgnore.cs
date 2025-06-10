using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("7d44af74bf694575a7216bc8d3c02fb0")]
public class TrapIgnore : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private bool m_AlsoIgnoreTrapsInCombat;

	public bool ShouldInteractWithTrap()
	{
		if (Game.Instance.Player.IsInCombat)
		{
			return !m_AlsoIgnoreTrapsInCombat;
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
