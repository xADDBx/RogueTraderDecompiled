using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

[Serializable]
[TypeId("62eaff56c12a477e97986b3780f11dd2")]
public class EnableMultiInitiative : MechanicEntityFactComponentDelegate, IHashable
{
	public int AdditionalTurns = 1;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<PartMultiInitiative>().Setup(AdditionalTurns);
		base.Owner.Initiative.Clear();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<PartMultiInitiative>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
