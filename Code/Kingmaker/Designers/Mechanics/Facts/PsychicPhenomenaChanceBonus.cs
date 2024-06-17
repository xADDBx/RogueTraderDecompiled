using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("312222be75794ea6a9526ef03fdecdcb")]
public class PsychicPhenomenaChanceBonus : UnitFactComponentDelegate, IHashable
{
	public int PhenomenaChanceBonusPercent;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
