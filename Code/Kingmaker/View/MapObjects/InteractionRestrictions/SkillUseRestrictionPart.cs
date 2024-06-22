using System;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public abstract class SkillUseRestrictionPart<T> : InteractionRestrictionPart<T>, IInteractionVariantActor, IHashable where T : SkillUseRestrictionSettings, new()
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int DCOverride;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool Unlocked;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool Failed;

	public abstract StatType Skill { get; }

	public abstract InteractionActorType Type { get; }

	public virtual bool ShowInteractFx => false;

	public int? RequiredItemsCount
	{
		get
		{
			if (base.Settings?.GetItem() == null)
			{
				return null;
			}
			return 1;
		}
	}

	public BlueprintItem RequiredItem => base.Settings?.GetItem();

	public bool InteractOnlyByNotInteractedUnit => Game.Instance.BlueprintRoot.Interaction.GlobalSkillCheckRestrictionSettings.CheckInteractOnlyByNotInteractedUnit(base.Settings.GetSkill());

	public new MapObjectView View => (MapObjectView)base.View;

	public new MapObjectEntity Owner => (MapObjectEntity)base.Owner;

	public int? InteractionDC => base.Settings?.GetDC();

	public InteractionPart InteractionPart => Owner.GetAll<InteractionPart>().FirstOrDefault();

	public bool CheckOnlyOnce => false;

	public bool CanUse => true;

	protected virtual bool ShouldRestrictAfterFail(BaseUnitEntity user)
	{
		if (base.Settings.InteractOnlyWithToolIfFailed)
		{
			return base.Settings.GetItem() == null;
		}
		return false;
	}

	protected override string GetDefaultBark(BaseUnitEntity user, bool restricted)
	{
		if (restricted && base.Settings.GetItem() != null && !user.Inventory.Contains(base.Settings.GetItem()))
		{
			return string.Concat(Game.Instance.BlueprintRoot.LocalizedTexts.NeedSupplyPrefix, " ", base.Settings.GetItem().Name);
		}
		return restricted ? Game.Instance.BlueprintRoot.LocalizedTexts.LockedContainer : Game.Instance.BlueprintRoot.LocalizedTexts.UnlockedContainer;
	}

	public override int GetUserPriority(BaseUnitEntity user)
	{
		if (base.Settings.GetItem() != null && !user.Inventory.Contains(base.Settings.GetItem()))
		{
			return -1;
		}
		if ((user.Stats.GetStat(base.Settings.GetSkill())?.ModifiedValue ?? 0) <= 0 && !Game.Instance.Player.CapitalPartyMode)
		{
			return -1;
		}
		return user.Stats.GetStat(base.Settings.GetSkill());
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		if (IsDisabled || Unlocked)
		{
			return true;
		}
		if (Failed && ShouldRestrictAfterFail(user))
		{
			return false;
		}
		if (base.Settings.GetItem() != null && !user.Inventory.Contains(base.Settings.GetItem()))
		{
			return false;
		}
		if (!base.Settings.IsPartyCheck)
		{
			return CheckRestrictionInternal(user);
		}
		foreach (BaseUnitEntity item in Game.Instance.Player.Party.OrderByDescending((BaseUnitEntity x) => x.Stats.GetStatOptional(base.Settings.GetSkill())?.ModifiedValue ?? 0))
		{
			ModifiableValue stat = item.Stats.GetStat(base.Settings.GetSkill());
			if (stat != null && stat.ModifiedValue <= 0)
			{
				break;
			}
			if (CheckRestrictionInternal(item))
			{
				return true;
			}
		}
		return false;
	}

	public override void OnDidInteract(BaseUnitEntity user)
	{
		if (base.Settings.GetItem() != null && !IsDisabled)
		{
			GameHelper.GetPlayerCharacter().Inventory.Remove(base.Settings.GetItem());
		}
	}

	public override void OnFailedInteract(BaseUnitEntity user)
	{
		if (base.Settings.GetItem() != null && !IsDisabled)
		{
			GameHelper.GetPlayerCharacter().Inventory.Remove(base.Settings.GetItem());
		}
		Failed = true;
	}

	private bool CheckRestrictionInternal(BaseUnitEntity user)
	{
		int difficulty = ((DCOverride == 0) ? base.Settings.GetDC() : DCOverride);
		RulePerformSkillCheck rulePerformSkillCheck = GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(user, base.Settings.GetSkill(), difficulty)
		{
			Voice = RulePerformSkillCheck.VoicingType.All
		});
		if (rulePerformSkillCheck.ResultIsSuccess)
		{
			EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IPickLockHandler>)delegate(IPickLockHandler h)
			{
				h.HandlePickLockSuccess(View);
			}, isCheckRuntime: true);
		}
		else
		{
			EventBus.RaiseEvent((IBaseUnitEntity)user, (Action<IPickLockHandler>)delegate(IPickLockHandler h)
			{
				h.HandlePickLockFail(View, critical: false);
			}, isCheckRuntime: true);
		}
		return rulePerformSkillCheck.ResultIsSuccess;
	}

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		if (isNewSettings)
		{
			Unlocked = base.Settings.StartUnlocked;
			DCOverride = base.Settings.GetDC();
		}
	}

	public abstract string GetInteractionName();

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
		result.Append(ref DCOverride);
		result.Append(ref Unlocked);
		result.Append(ref Failed);
		return result;
	}
}
