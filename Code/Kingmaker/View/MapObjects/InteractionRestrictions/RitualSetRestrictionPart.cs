using Kingmaker.EntitySystem.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public class RitualSetRestrictionPart : NeedItemRestrictionPart<RitualSetRestrictionSettings>, IHashable
{
	public override bool ShouldCheckSourceComponent => false;

	public override bool CanUse
	{
		get
		{
			InteractionSkillCheckSettings obj = base.InteractionPart.Settings as InteractionSkillCheckSettings;
			if (obj == null)
			{
				return true;
			}
			return !obj.OnlyCheckOnce;
		}
	}

	protected override string GetDefaultBark(BaseUnitEntity user, bool restricted)
	{
		if (restricted && base.Settings.GetItem() != null && !user.Inventory.Contains(base.Settings.GetItem()))
		{
			return string.Concat(Game.Instance.BlueprintRoot.LocalizedTexts.NeedSupplyPrefix, " ", base.Settings.GetItem().Name);
		}
		return restricted ? Game.Instance.BlueprintRoot.LocalizedTexts.AccessDenied : Game.Instance.BlueprintRoot.LocalizedTexts.AccessReceived;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
