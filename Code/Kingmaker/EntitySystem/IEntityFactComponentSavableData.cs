using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

[HashRoot]
public abstract class IEntityFactComponentSavableData : IHashable
{
	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
