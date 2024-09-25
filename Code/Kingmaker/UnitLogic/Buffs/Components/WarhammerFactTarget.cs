using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[TypeId("41cb587881aa4b145bd090532a08dac1")]
public class WarhammerFactTarget : UnitFactComponentDelegate, IHashable
{
	public enum TargetType
	{
		SkillChecker,
		Party,
		Starship,
		MainCharacter
	}

	public TargetType Target;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
