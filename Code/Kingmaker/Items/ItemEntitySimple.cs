using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Items;

public class ItemEntitySimple : ItemEntity, IHashable
{
	public override bool OnPostLoadValidation()
	{
		return true;
	}

	public ItemEntitySimple(BlueprintItem blueprint)
		: base(blueprint)
	{
	}

	protected ItemEntitySimple(JsonConstructorMark _)
		: base(_)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
