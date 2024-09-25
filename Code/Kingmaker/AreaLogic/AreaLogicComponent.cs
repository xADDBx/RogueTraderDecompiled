using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic;

[AllowedOn(typeof(BlueprintArea))]
[TypeId("d7e59305372c7174e95c74f47a54ff89")]
public abstract class AreaLogicComponent : EntityFactComponentDelegate, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
