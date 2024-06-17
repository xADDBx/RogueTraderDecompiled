using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects;

[Serializable]
public class InteractionSkillCheckSettings : InteractionSettings
{
	public enum FakeType
	{
		None,
		FakeSuccess,
		FakeFailure
	}

	public enum PenaltyType
	{
		None,
		Damage,
		Debuff
	}

	[Serializable]
	public class Experience
	{
		public EncounterType Encounter = EncounterType.SkillCheck;

		public float Modifier = 1f;

		public bool CRByZone = true;

		[HideIf("CRByZone")]
		public int CR;
	}

	[Space(20f)]
	public bool FadeOnSuccess;

	public bool FadeOnFail;

	public PenaltyType PenaltyForFailedSkillCheck;

	public bool ApplyPenaltyAfterFade = true;

	[Space(10f)]
	public StatType Skill;

	[ShowIf("CanUseWithoutSupply")]
	public bool NeedSupply = true;

	public int DC;

	public bool Exact;

	public FakeType FakeResult;

	public ViewDCModifier[] DCModifiers = new ViewDCModifier[0];

	[Space(10f)]
	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset DisplayName;

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset ShortDescription;

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset TooltipKeyword;

	[Space(10f)]
	[CanBeNull]
	public ConditionsReference Condition;

	[Space(10f)]
	public bool HideDC;

	public bool DisableAfterUse;

	[ConditionalHide("DisableAfterUse")]
	public bool OnlyCheckOnce = true;

	[ConditionalShow("OnlyCheckOnce")]
	public bool TriggerActionsEveryClick;

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset DisplayNameAfterUse;

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset ShortDescriptionPassed;

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset ShortDescriptionFailed;

	public bool IsPartyCheck;

	[Space(10f)]
	[CanBeNull]
	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.Bark)]
	public SharedStringAsset CheckPassedBark;

	[CanBeNull]
	public ActionsReference CheckPassedActions;

	[SerializeField]
	[FormerlySerializedAs("TeleportOnSuccess")]
	private BlueprintAreaEnterPointReference m_TeleportOnSuccess;

	[SerializeField]
	[FormerlySerializedAs("TeleportOnFail")]
	private BlueprintAreaEnterPointReference m_TeleportOnFail;

	[Space(10f)]
	[CanBeNull]
	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.Bark)]
	public SharedStringAsset CheckFailBark;

	[CanBeNull]
	public ActionsReference CheckFailedActions;

	[Tooltip("Show bark on MapObject user. By default bark is shown on MapObject.")]
	public bool ShowOnUser;

	public Experience Exp;

	public bool CanUseWithoutSupply
	{
		get
		{
			StatType skill = Skill;
			return skill == StatType.SkillDemolition || skill == StatType.SkillLoreXenos || skill == StatType.SkillLogic;
		}
	}

	public BlueprintAreaEnterPoint TeleportOnSuccess => m_TeleportOnSuccess?.Get();

	public BlueprintAreaEnterPoint TeleportOnFail => m_TeleportOnFail?.Get();

	public int GetDC()
	{
		int num = DC;
		if (DCModifiers != null)
		{
			ViewDCModifier[] dCModifiers = DCModifiers;
			foreach (ViewDCModifier viewDCModifier in dCModifiers)
			{
				if (viewDCModifier != null && viewDCModifier.Conditions != null && viewDCModifier.Conditions.Get().Check())
				{
					num += viewDCModifier.Mod;
				}
			}
		}
		return num;
	}
}
