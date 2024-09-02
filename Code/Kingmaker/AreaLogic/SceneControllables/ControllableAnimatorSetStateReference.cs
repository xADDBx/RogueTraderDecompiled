using System;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[Serializable]
[HashRoot]
public class ControllableAnimatorSetStateReference : ControllableReference, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
