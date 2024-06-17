using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("6957e888c702e8041b67aed4c0231fb4")]
public class RemoveBuffOnLoad : UnitBuffComponentDelegate, IHashable
{
	public bool OnlyFromParty;

	protected override void OnActivateOrPostLoad()
	{
		if (!OnlyFromParty || Game.Instance.Player.Party.Contains(base.Owner))
		{
			base.Buff.MarkExpired();
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
