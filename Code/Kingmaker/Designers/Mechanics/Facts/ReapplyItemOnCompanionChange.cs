using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[AllowedOn(typeof(BlueprintItem))]
[TypeId("b3c728000e984d46b1c8ad3d55f6b016")]
public class ReapplyItemOnCompanionChange : EntityFactComponentDelegate<ItemEntity>, ICompanionStateChanged, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public void HandleCompanionStateChanged()
	{
		base.Owner?.ReapplyFactsForWielder();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
