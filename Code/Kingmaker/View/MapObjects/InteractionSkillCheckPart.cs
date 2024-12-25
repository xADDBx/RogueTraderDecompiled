using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.Code.UI.MVVM.VM.Fade;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Interaction;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Settings;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.InteractionComponentBase;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Kingmaker.View.Mechanics.Interactions.Restrictions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class InteractionSkillCheckPart : InteractionPart<InteractionSkillCheckSettings>, IAreaEnterPointReference, IHasInteractionVariantActors, IHashable
{
	[JsonProperty]
	public readonly HashSet<UnitReference> InteractedUnits = new HashSet<UnitReference>();

	[JsonProperty]
	private HashSet<UnitReference> m_PunishedUsers = new HashSet<UnitReference>();

	private bool m_InteractOnlyByNotInteractedUnit;

	[JsonProperty]
	private int m_VersionResetToDefault = -1;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public int DCOverride { get; set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool AlreadyUsed { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool ExperienceObtained { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool CheckPassed { get; private set; }

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public StatType SkillOverride { get; private set; }

	public override bool InteractThroughVariants { get; protected set; }

	public bool IsFailed
	{
		get
		{
			if (AlreadyUsed)
			{
				return !CheckPassed;
			}
			return false;
		}
	}

	protected override void OnSettingsDidSet(bool isNewSettings)
	{
		base.OnSettingsDidSet(isNewSettings);
		if (isNewSettings)
		{
			DCOverride = base.Settings.GetDC();
		}
	}

	public override bool CanInteract()
	{
		if (AlreadyUsed)
		{
			return base.CanInteract();
		}
		ConditionsReference condition = base.Settings.Condition;
		if (condition != null && condition.Get()?.Conditions.HasConditions == true)
		{
			using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
			{
				if (!condition.Get().Conditions.Check())
				{
					return false;
				}
			}
		}
		return base.CanInteract();
	}

	internal StatType GetSkill()
	{
		if (!SkillOverride.IsSkill())
		{
			return base.Settings.Skill;
		}
		return SkillOverride;
	}

	protected override void OnDidInteract(BaseUnitEntity user)
	{
		InteractedUnits.Add(user.FromBaseUnitEntity());
		if (!base.Settings.OnlyCheckOnce || !AlreadyUsed)
		{
			return;
		}
		foreach (InteractionRestrictionPart needItemRestriction in GetNeedItemRestrictions())
		{
			if (needItemRestriction != null)
			{
				needItemRestriction.IsDisabled = true;
			}
		}
		IEnumerable<InteractionRestrictionPart> GetNeedItemRestrictions()
		{
			yield return base.View.Data.Parts.GetOptional<MeltaChargeRestrictionPart>();
			yield return base.View.Data.Parts.GetOptional<RitualSetRestrictionPart>();
			yield return base.View.Data.Parts.GetOptional<MultikeyRestrictionPart>();
		}
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		bool isCriticalFail = false;
		bool flag = false;
		StatType skill = GetSkill();
		if (!base.Settings.OnlyCheckOnce || !AlreadyUsed || (base.Settings.CheckConditionsOnEveryInteraction && !base.Settings.OnlyCheckOnce))
		{
			int num = ((DCOverride == 0) ? base.Settings.GetDC() : DCOverride);
			int num2 = SettingsRoot.Difficulty.SkillCheckModifier;
			if (num + num2 >= 100)
			{
				CheckPassed = true;
			}
			else
			{
				foreach (BaseUnitEntity rollUnit in GetRollUnits(user, skill))
				{
					CheckPassed = skill != 0 && GameHelper.CheckSkillResult(rollUnit, skill, num, out isCriticalFail, RulePerformSkillCheck.VoicingType.All, GetEnsureSuccess());
					if (CheckPassed)
					{
						if (!ExperienceObtained && skill != 0)
						{
							GameHelper.GainExperienceForSkillCheck(ExperienceHelper.GetCheckExp(num, Game.Instance.CurrentlyLoadedArea?.GetCR() ?? 0), rollUnit);
							ExperienceObtained = true;
						}
						break;
					}
				}
			}
			AlreadyUsed = true;
			flag = true;
		}
		if (flag || base.Settings.TriggerActionsEveryClick)
		{
			MapObjectView mapObjectView = base.View.Or(null);
			if ((object)mapObjectView != null)
			{
				mapObjectView.FactHolder.Or(null)?.GetFact()?.CallComponents(delegate(ISkillCheckInteractionTrigger t)
				{
					t.OnInteract(user, this, CheckPassed);
				});
			}
			ActionsHolder actionsHolder = ((CheckPassed || (skill == StatType.Unknown && (bool)base.Settings.CheckPassedActions.Get())) ? base.Settings.CheckPassedActions.Get() : base.Settings.CheckFailedActions.Get());
			if (actionsHolder != null && actionsHolder.Actions.HasActions)
			{
				using (ContextData<MechanicEntityData>.Request().Setup(base.Owner))
				{
					using (ContextData<InteractingUnitData>.Request().Setup(user))
					{
						actionsHolder.Actions.Run();
					}
				}
			}
		}
		BlueprintAreaEnterPoint blueprintAreaEnterPoint = ((CheckPassed || (skill == StatType.Unknown && (bool)base.Settings.TeleportOnSuccess)) ? base.Settings.TeleportOnSuccess : base.Settings.TeleportOnFail);
		if ((bool)blueprintAreaEnterPoint)
		{
			Game.Instance.Teleport(blueprintAreaEnterPoint, includeFollowers: true);
		}
		if (base.Settings.DisableAfterUse)
		{
			base.Enabled = false;
		}
		SharedStringAsset sharedStringAsset = ((CheckPassed || (skill == StatType.Unknown && (bool)base.Settings.CheckPassedBark)) ? base.Settings.CheckPassedBark : base.Settings.CheckFailBark);
		if ((bool)sharedStringAsset)
		{
			if (base.Settings.ShowOnUser)
			{
				ShowBark(user, sharedStringAsset.String, user);
			}
			else
			{
				ShowBark(base.Owner, sharedStringAsset.String, user);
			}
		}
		else if (skill == StatType.SkillLogic)
		{
			ShowBark(user, (CheckPassed && skill == StatType.SkillLogic) ? Game.Instance.BlueprintRoot.LocalizedTexts.AccessReceived : Game.Instance.BlueprintRoot.LocalizedTexts.AccessDenied, user);
		}
		if (!CheckPassed)
		{
			if (base.Settings.FadeOnFail)
			{
				if (base.Settings.ApplyPenaltyAfterFade)
				{
					LoadingProcess.Instance.StartLoadingProcess(FadeOutCoroutine(), delegate
					{
						ApplyPenalty(user, skill, isCriticalFail);
					}, LoadingProcessTag.TeleportParty);
				}
				else
				{
					FadeCanvas.Fadeout(fade: true);
					FadeCanvas.Fadeout(fade: false);
					ApplyPenalty(user, skill, isCriticalFail);
				}
			}
			else
			{
				ApplyPenalty(user, skill, isCriticalFail);
			}
		}
		else if (base.Settings.FadeOnSuccess)
		{
			FadeCanvas.Fadeout(fade: true);
			FadeCanvas.Fadeout(fade: false);
		}
	}

	private static void ShowBark(Entity entity, LocalizedString text, BaseUnitEntity user)
	{
		BarkPlayer.Bark(entity, text, -1f, playVoiceOver: false, user);
	}

	private List<BaseUnitEntity> GetRollUnits(BaseUnitEntity user, StatType skill)
	{
		if (!base.Settings.IsPartyCheck)
		{
			return new List<BaseUnitEntity> { user };
		}
		List<BaseUnitEntity> list = (from x in Game.Instance.Player.Party.Where(delegate(BaseUnitEntity x)
			{
				ModifiableValue statOptional = x.Stats.GetStatOptional(skill);
				return statOptional != null && statOptional.ModifiedValue >= 0;
			})
			orderby x.Stats.GetStatOptional(skill)?.ModifiedValue ?? 0 descending
			select x).ToList();
		if (list.Count != 0)
		{
			return list;
		}
		return new List<BaseUnitEntity> { user };
	}

	private bool? GetEnsureSuccess()
	{
		return base.Settings.FakeResult switch
		{
			InteractionSkillCheckSettings.FakeType.None => null, 
			InteractionSkillCheckSettings.FakeType.FakeSuccess => true, 
			InteractionSkillCheckSettings.FakeType.FakeFailure => false, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private IEnumerator<object> FadeOutCoroutine()
	{
		FadeCanvas.Fadeout(fade: true);
		FadeCanvas.Fadeout(fade: false);
		yield break;
	}

	private void ApplyPenalty(BaseUnitEntity user, StatType skill, bool isCriticalFail = false)
	{
		if (CheckPassed)
		{
			return;
		}
		switch (base.Settings.PenaltyForFailedSkillCheck)
		{
		case InteractionSkillCheckSettings.PenaltyType.Damage:
			BlueprintWarhammerRoot.Instance.SkillCheckRoot.DamageSkillCheckRoot.DealDamage(user, isCriticalFail);
			break;
		case InteractionSkillCheckSettings.PenaltyType.Debuff:
			if (!m_PunishedUsers.Contains(user.FromBaseUnitEntity()))
			{
				BlueprintWarhammerRoot.Instance.SkillCheckRoot.DebuffSkillCheckRoot.SetDebuff(user, skill, isCriticalFail);
				m_PunishedUsers.Add(user.FromBaseUnitEntity());
			}
			break;
		}
	}

	public override BaseUnitEntity SelectUnit(ReadonlyList<BaseUnitEntity> units, bool muteEvents = false, StatType? skillFromVariant = null)
	{
		BaseUnitEntity baseUnitEntity = null;
		int num = int.MinValue;
		foreach (BaseUnitEntity item in units)
		{
			if ((units.Count <= 1 || !item.IsPet) && (!m_InteractOnlyByNotInteractedUnit || !InteractedUnits.Contains(item.FromBaseUnitEntity())))
			{
				StatType skill = GetSkill();
				int num2 = ((skill == StatType.Unknown) ? 1 : ((int)item.Stats.GetStat(skill)));
				if (item.State.CanAct && item.State.CanMove && (baseUnitEntity == null || num2 > num))
				{
					baseUnitEntity = item;
					num = num2;
				}
			}
		}
		return baseUnitEntity;
	}

	public bool GetUsagesFor(BlueprintAreaEnterPoint point)
	{
		if (point != base.Settings.TeleportOnSuccess)
		{
			return point == base.Settings.TeleportOnFail;
		}
		return true;
	}

	IEnumerable<IInteractionVariantActor> IHasInteractionVariantActors.GetInteractionVariantActors()
	{
		if (base.Type == InteractionType.Direct)
		{
			return null;
		}
		if (!InteractThroughVariants || (base.Settings.OnlyCheckOnce && AlreadyUsed && (CheckPassed || base.Settings.TriggerActionsEveryClick)))
		{
			return null;
		}
		return base.View.Data.Parts.GetAll<IInteractionVariantActor>();
	}

	protected override void ConfigureRestrictions()
	{
		switch (GetSkill())
		{
		case StatType.SkillDemolition:
			if (base.Settings.NeedSupply)
			{
				base.View.Data.Parts.GetOrCreate<MeltaChargeRestrictionPart>();
			}
			break;
		case StatType.SkillLogic:
			base.View.Data.Parts.GetOrCreate<SkillUseWithoutToolRestrictionPart>();
			base.View.Data.Parts.GetOrCreate<RitualSetRestrictionPart>();
			base.Settings.InteractOnlyWithToolAfterFail = true;
			InteractThroughVariants = true;
			break;
		case StatType.SkillLoreXenos:
			if (base.Settings.NeedSupply)
			{
				base.View.Data.Parts.GetOrCreate<SkillUseWithoutToolRestrictionPart>();
				base.View.Data.Parts.GetOrCreate<MultikeyRestrictionPart>();
				base.Settings.InteractOnlyWithToolAfterFail = true;
				InteractThroughVariants = true;
			}
			break;
		}
		m_InteractOnlyByNotInteractedUnit = Game.Instance.BlueprintRoot.Interaction.GlobalInteractionSkillCheckSettings.CheckInteractOnlyByNotInteractedUnit(GetSkill());
	}

	public override void OnResetToDefault()
	{
		base.OnResetToDefault();
		AlreadyUsed = false;
		ExperienceObtained = false;
		CheckPassed = false;
		InteractedUnits.Clear();
		m_PunishedUsers.Clear();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		int val2 = DCOverride;
		result.Append(ref val2);
		bool val3 = AlreadyUsed;
		result.Append(ref val3);
		bool val4 = ExperienceObtained;
		result.Append(ref val4);
		bool val5 = CheckPassed;
		result.Append(ref val5);
		StatType val6 = SkillOverride;
		result.Append(ref val6);
		HashSet<UnitReference> interactedUnits = InteractedUnits;
		if (interactedUnits != null)
		{
			int num = 0;
			foreach (UnitReference item in interactedUnits)
			{
				UnitReference obj = item;
				num ^= UnitReferenceHasher.GetHash128(ref obj).GetHashCode();
			}
			result.Append(num);
		}
		HashSet<UnitReference> punishedUsers = m_PunishedUsers;
		if (punishedUsers != null)
		{
			int num2 = 0;
			foreach (UnitReference item2 in punishedUsers)
			{
				UnitReference obj2 = item2;
				num2 ^= UnitReferenceHasher.GetHash128(ref obj2).GetHashCode();
			}
			result.Append(num2);
		}
		result.Append(ref m_VersionResetToDefault);
		return result;
	}
}
