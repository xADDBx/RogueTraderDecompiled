using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public class UnlockRestrictionPart : InteractionRestrictionPart<UnlockRestrictionSettings>, IUnlockableFlagReference, IInteractionVariantActor, IHashable
{
	bool IInteractionVariantActor.ShowInteractFx => false;

	public int? RequiredItemsCount => null;

	public BlueprintItem RequiredItem => null;

	public StatType Skill => StatType.Unknown;

	public int? InteractionDC => null;

	public InteractionActorType Type => InteractionActorType.Unlock;

	public InteractionPart InteractionPart => base.ConcreteOwner.GetAll<InteractionPart>().FirstOrDefault();

	public bool CheckOnlyOnce => false;

	public bool CanUse => true;

	public override int GetUserPriority(BaseUnitEntity user)
	{
		if (!base.Settings.Flag.IsUnlocked)
		{
			return -1;
		}
		return 0;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		return base.Settings.Flag.IsUnlocked;
	}

	public UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag)
	{
		if (flag != base.Settings.Flag)
		{
			return UnlockableFlagReferenceType.None;
		}
		return UnlockableFlagReferenceType.Check;
	}

	public string GetInteractionName()
	{
		return null;
	}

	bool IInteractionVariantActor.CanInteract(BaseUnitEntity user)
	{
		return CheckRestriction(user);
	}

	void IInteractionVariantActor.OnInteract(BaseUnitEntity user)
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
