using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class InteractionButtonPart : InteractionPart<InteractionSettings>, IHashable
{
	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	protected override void OnInteract(BaseUnitEntity user)
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
