using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("12f82af1a10a496498580ad9ada9b6cb")]
public class MasterDependentFact : UnitFactComponentDelegate, IPetInitializationHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	protected virtual void Setup()
	{
	}

	void IPetInitializationHandler.OnPetInitialized()
	{
		Setup();
	}

	protected override void OnActivate()
	{
		if (base.Owner.Master != null)
		{
			Setup();
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
