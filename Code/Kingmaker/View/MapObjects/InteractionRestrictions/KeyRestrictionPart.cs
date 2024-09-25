using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public class KeyRestrictionPart : InteractionRestrictionPart<KeyRestrictionSettings>, IInteractionVariantActor, IHashable
{
	bool IInteractionVariantActor.ShowInteractFx => false;

	public int? RequiredItemsCount => 1;

	public BlueprintItem RequiredItem => base.Settings?.Key;

	public int? InteractionDC => null;

	public InteractionActorType Type => InteractionActorType.Key;

	public InteractionPart InteractionPart => base.ConcreteOwner.GetAll<InteractionPart>().FirstOrDefault();

	public StatType Skill => StatType.Unknown;

	public bool CheckOnlyOnce => false;

	public bool CanUse => true;

	protected override string GetDefaultBark(BaseUnitEntity baseUnitEntity, bool restricted)
	{
		return restricted ? Game.Instance.BlueprintRoot.LocalizedTexts.LockedwithKey : Game.Instance.BlueprintRoot.LocalizedTexts.UnlockedWithKey;
	}

	public override int GetUserPriority(BaseUnitEntity user)
	{
		if (!user.Inventory.Contains(base.Settings.Key))
		{
			return -1;
		}
		return 0;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		if (!IsDisabled)
		{
			return user.Inventory.Contains(base.Settings.Key);
		}
		return true;
	}

	public override void OnDidInteract(BaseUnitEntity user)
	{
		if (!base.Settings.DontRemoveKey && !IsDisabled)
		{
			GameHelper.GetPlayerCharacter().Inventory.Remove(base.Settings.Key);
		}
	}

	public string GetInteractionName()
	{
		return base.Settings?.Key?.Name;
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
