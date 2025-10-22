using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects.InteractionComponentBase;
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

	[Space(10f)]
	public StatType Skill;

	[ShowIf("CanUseWithoutSupply")]
	public bool NeedSupply = true;

	public SkillCheckDifficulty Difficulty;

	[ShowIf("DifficultyIsCustom")]
	public int DC;

	[ShowIf("DifficultyIsCustom")]
	public ViewDCModifier[] DCModifiers = new ViewDCModifier[0];

	public FakeType FakeResult;

	[Space(10f)]
	public bool HideDC;

	[Space(10f)]
	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset DisplayName;

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset ShortDescription;

	public bool DisableAfterUse;

	[HideIf("DisableAfterUse")]
	public bool OnlyCheckOnce = true;

	[HideIf("OnlyCheckOnce")]
	public bool CheckConditionsOnEveryInteraction;

	[ShowIf("OnlyCheckOnce")]
	public bool TriggerActionsEveryClick;

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset DisplayNameAfterUse;

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset ShortDescriptionPassed;

	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.MapObject)]
	public SharedStringAsset ShortDescriptionFailed;

	public bool IsPartyCheck;

	[Space(20f)]
	public bool FadeOnSuccess;

	public bool FadeOnFail;

	public PenaltyType PenaltyForFailedSkillCheck;

	public bool ApplyPenaltyAfterFade = true;

	[CanBeNull]
	[Space(10f)]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public SharedStringAsset CheckPassedBark;

	[CanBeNull]
	[ShowCreator]
	public ActionsReference CheckPassedActions;

	[SerializeField]
	[FormerlySerializedAs("TeleportOnSuccess")]
	private BlueprintAreaEnterPointReference m_TeleportOnSuccess;

	[SerializeField]
	[FormerlySerializedAs("TeleportOnFail")]
	private BlueprintAreaEnterPointReference m_TeleportOnFail;

	[CanBeNull]
	[Space(10f)]
	[StringCreateWindow(StringCreateWindowAttribute.StringType.Bark)]
	public SharedStringAsset CheckFailBark;

	[CanBeNull]
	[ShowCreator]
	public ActionsReference CheckFailedActions;

	[ShowCreator]
	[Space(10f)]
	[CanBeNull]
	public ConditionsReference Condition;

	[Tooltip("Show bark on MapObject user. By default bark is shown on MapObject.")]
	public bool ShowOnUser;

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

	private bool DifficultyIsCustom => Difficulty == SkillCheckDifficulty.Custom;

	public int GetDC()
	{
		if (Difficulty != 0)
		{
			return Difficulty.GetDC(Skill);
		}
		if (DCModifiers == null)
		{
			return DC;
		}
		int num = DC;
		ViewDCModifier[] dCModifiers = DCModifiers;
		foreach (ViewDCModifier viewDCModifier in dCModifiers)
		{
			if (viewDCModifier != null && viewDCModifier.Conditions?.Get().Check() == true)
			{
				num += viewDCModifier.Mod;
			}
		}
		return num;
	}
}
