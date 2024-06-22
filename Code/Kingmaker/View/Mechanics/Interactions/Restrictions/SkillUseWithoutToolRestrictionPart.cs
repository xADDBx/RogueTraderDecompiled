using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Mechanics.Interactions.Restrictions;

internal class SkillUseWithoutToolRestrictionPart : InteractionRestrictionPart<SkillUseWithoutToolRestrictionSettings>, IInteractionVariantActor, IHashable
{
	public override bool ShouldCheckSourceComponent => false;

	public int? InteractionDC => InteractionPart?.Settings?.GetDC();

	bool IInteractionVariantActor.ShowInteractFx => false;

	public int? RequiredItemsCount => null;

	public BlueprintItem RequiredItem => base.Settings?.Type switch
	{
		InteractionActorType.Logic => base.Owner.ToEntity().GetOptional<RitualSetRestrictionPart>()?.Settings?.GetItem(), 
		InteractionActorType.LoreXenos => base.Owner.ToEntity().GetOptional<MultikeyRestrictionPart>()?.Settings?.GetItem(), 
		_ => null, 
	};

	public StatType Skill => InteractionPart.GetSkill();

	public bool CheckOnlyOnce => InteractionPart.Settings.OnlyCheckOnce;

	public bool CanUse => true;

	public InteractionActorType Type => base.Settings.Type;

	InteractionPart IInteractionVariantActor.InteractionPart => InteractionPart;

	private InteractionSkillCheckPart InteractionPart => base.ConcreteOwner.GetRequired<InteractionSkillCheckPart>();

	protected override void OnAttach()
	{
		base.OnAttach();
		base.Settings.Type = InteractionPart.GetSkill().ToInteractionActorType();
	}

	public string GetInteractionName()
	{
		return LocalizedTexts.Instance.Stats.GetText(InteractionPart.GetSkill());
	}

	bool IInteractionVariantActor.CanInteract(BaseUnitEntity user)
	{
		return CheckRestriction(user);
	}

	public override int GetUserPriority(BaseUnitEntity user)
	{
		StatType skill = InteractionPart.GetSkill();
		return user.Stats.GetStatOptional(skill)?.ModifiedValue ?? 0;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		if (InteractionPart.IsFailed)
		{
			return !InteractionPart.Settings.InteractOnlyWithToolAfterFail;
		}
		return true;
	}

	void IInteractionVariantActor.OnInteract(BaseUnitEntity user)
	{
	}

	protected override string GetDefaultBark(BaseUnitEntity baseUnitEntity, bool restricted)
	{
		if (!restricted)
		{
			return null;
		}
		BlueprintItem item = base.Settings.Type switch
		{
			InteractionActorType.Logic => base.ConcreteOwner.GetOptional<RitualSetRestrictionPart>()?.Settings?.GetItem(), 
			InteractionActorType.LoreXenos => base.ConcreteOwner.GetOptional<MultikeyRestrictionPart>()?.Settings?.GetItem(), 
			_ => null, 
		};
		if (item != null)
		{
			return Game.Instance.BlueprintRoot.LocalizedTexts.InteractOnlyWithTool.ToString(delegate
			{
				GameLogContext.Text = item.Name;
			});
		}
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
