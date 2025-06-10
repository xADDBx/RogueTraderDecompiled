using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AI.Blueprints.Components;

[TypeId("acc1f164fd9b4b0685638108e52a46a8")]
public class AiCustomBehaviourForSquad : EntityFactComponentDelegate, IHashable
{
	public CustomBehaviourType BehaviourType;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
