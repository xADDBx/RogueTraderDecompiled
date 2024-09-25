using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("d645ff5b358841ec9a57e8d4f7e73009")]
public class ForceDismembermentOnDeath : UnitFactComponentDelegate, IHashable
{
	public UnitDismemberType Dismember;

	protected override void OnActivate()
	{
		base.OnActivate();
		base.Fact.Owner.LifeState.ForceDismember = Dismember;
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		base.Fact.Owner.LifeState.ForceDismember = UnitDismemberType.None;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
