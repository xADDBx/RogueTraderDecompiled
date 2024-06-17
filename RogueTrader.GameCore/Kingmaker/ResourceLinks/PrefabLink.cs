using System;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

[Serializable]
[HashRoot]
public class PrefabLink : WeakResourceLink<GameObject>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
