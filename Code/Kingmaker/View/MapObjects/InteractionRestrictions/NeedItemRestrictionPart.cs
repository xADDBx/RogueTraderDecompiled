using System;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public abstract class NeedItemRestrictionPart<T> : InteractionRestrictionPart<T>, IInteractionVariantActor, IHashable where T : NeedItemRestrictionSettings, new()
{
	public virtual bool ShowInteractFx => false;

	public int? RequiredItemsCount => 1;

	public BlueprintItem RequiredItem => base.Settings.GetItem();

	public int? InteractionDC => null;

	public virtual InteractionActorType Type
	{
		get
		{
			if (base.Settings.GetItem() == Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MeltaChargeItem)
			{
				return InteractionActorType.MeltaCharge;
			}
			if (base.Settings.GetItem() == Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MultikeyItem)
			{
				return InteractionActorType.Key;
			}
			if (base.Settings.GetItem() == Game.Instance.BlueprintRoot.SystemMechanics.Consumables.RitualSetItem)
			{
				return InteractionActorType.Ritual;
			}
			throw new NotImplementedException();
		}
	}

	public InteractionPart InteractionPart => base.ConcreteOwner.GetAll<InteractionPart>().FirstOrDefault();

	public StatType Skill => StatType.Unknown;

	protected override string GetDefaultBark(BaseUnitEntity baseUnitEntity, bool restricted)
	{
		if (!restricted)
		{
			return null;
		}
		return string.Concat(Game.Instance.BlueprintRoot.LocalizedTexts.NeedSupplyPrefix, " ", base.Settings.GetItem().Name);
	}

	public override int GetUserPriority(BaseUnitEntity user)
	{
		if (!user.Inventory.Contains(base.Settings.GetItem()))
		{
			return -1;
		}
		return 0;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		if (!IsDisabled)
		{
			return user.Inventory.Contains(base.Settings.GetItem());
		}
		return true;
	}

	public override void OnDidInteract(BaseUnitEntity user)
	{
		if (!IsDisabled)
		{
			GameHelper.GetPlayerCharacter().Inventory.Remove(base.Settings.GetItem());
		}
	}

	public override void OnFailedInteract(BaseUnitEntity user)
	{
		if (!IsDisabled)
		{
			GameHelper.GetPlayerCharacter().Inventory.Remove(base.Settings.GetItem());
		}
	}

	public virtual string GetInteractionName()
	{
		return base.Settings?.GetItem()?.Name;
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
