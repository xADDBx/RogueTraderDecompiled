using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Code.UnitLogic.FactLogic;

[TypeId("14bd454a75fd49a89af9346c79e40023")]
public class UseBallisticSkillToParry : UnitFactComponentDelegate, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Features.CanUseBallisticSkillToParry.Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.CanUseBallisticSkillToParry.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
