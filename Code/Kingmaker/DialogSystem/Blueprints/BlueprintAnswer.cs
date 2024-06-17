using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.DialogSystem.State;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Localization;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("df78945bb9f434e40b897758499cb714")]
public class BlueprintAnswer : BlueprintAnswerBase, ISoulMarkShiftProvider
{
	public LocalizedString Text;

	public CueSelection NextCue;

	public bool ShowOnce;

	[ShowIf("ShowOnce")]
	public bool ShowOnceCurrentDialog;

	public ShowCheck ShowCheck;

	[ShowIf("HasShowCheck")]
	[SerializeField]
	private ActionList OnCheckSuccess;

	[ShowIf("HasShowCheck")]
	[SerializeField]
	private ActionList OnCheckFail;

	public bool DebugMode;

	public CharacterSelection CharacterSelection;

	public ConditionsChecker ShowConditions;

	public ConditionsChecker SelectConditions;

	[Tooltip("Show this answer only if it is followed by a valid cue.")]
	public bool RequireValidCue;

	public bool AddToHistory = true;

	public ActionList OnSelect;

	public LocalizedString Description;

	[FormerlySerializedAs("CustomChecks")]
	[Tooltip("Show this check on answer in dialog interface. Instead of check calculated from BlueprintCheck node.")]
	public CheckData[] FakeChecks;

	[NotNull]
	public SoulMarkShift SoulMarkShift = new SoulMarkShift();

	public string DisplayText => Text;

	public bool HasShowCheck => ShowCheck.Type != StatType.Unknown;

	public bool CapitalPartyChecksEnabled
	{
		get
		{
			CharacterSelection characterSelection = CharacterSelection;
			if (characterSelection == null || characterSelection.SelectionType != CharacterSelection.Type.Capital)
			{
				CharacterSelection characterSelection2 = CharacterSelection;
				if (characterSelection2 != null && characterSelection2.SelectionType == CharacterSelection.Type.Clear)
				{
					return Game.Instance.Player.CapitalPartyMode;
				}
				return false;
			}
			return true;
		}
	}

	public bool HasExchangeData
	{
		get
		{
			if (!GetRewards().Any())
			{
				return HasExchangeDataOnSelect();
			}
			return true;
		}
	}

	public bool HasConditionsForTooltip
	{
		get
		{
			if (!GetRequirements().Any())
			{
				return HasConditionsOnSelect();
			}
			return true;
		}
	}

	public CheckData[] SkillChecks
	{
		get
		{
			if (FakeChecks.Length != 0)
			{
				return FakeChecks;
			}
			BlueprintCheck blueprintCheck = NextCue.Select() as BlueprintCheck;
			if ((bool)blueprintCheck && !blueprintCheck.Hidden)
			{
				return new CheckData[1]
				{
					new CheckData(blueprintCheck.Type, blueprintCheck.GetDC())
				};
			}
			return FakeChecks;
		}
	}

	public List<SkillCheckDC> SkillChecksDC
	{
		get
		{
			List<SkillCheckDC> list = new List<SkillCheckDC>();
			BaseUnitEntity baseUnitEntity = CharacterSelection.SelectUnit(this, Game.Instance.Player.MainCharacterEntity);
			foreach (BlueprintCueBase item in NextCue.Cues.Dereference())
			{
				BlueprintCheck blueprintCheck = item as BlueprintCheck;
				if ((blueprintCheck == null || blueprintCheck.Conditions.Check()) && (bool)blueprintCheck && !blueprintCheck.Hidden)
				{
					if (baseUnitEntity != null)
					{
						RulePerformSkillCheck rulePerformSkillCheck = new RulePerformSkillCheck(baseUnitEntity, blueprintCheck.Type, blueprintCheck.GetDC());
						rulePerformSkillCheck.Calculate(doCheck: false);
						list.Add(new SkillCheckDC(baseUnitEntity, rulePerformSkillCheck.StatType, rulePerformSkillCheck.Difficulty, rulePerformSkillCheck.StatValue, isBest: false, null));
					}
					else
					{
						RulePerformPartySkillCheck rulePerformPartySkillCheck = new RulePerformPartySkillCheck(blueprintCheck.Type, blueprintCheck.GetDC(), CapitalPartyChecksEnabled);
						rulePerformPartySkillCheck.Calculate(isTrigger: false, doCheck: false);
						list.Add(new SkillCheckDC(rulePerformPartySkillCheck.Roller, rulePerformPartySkillCheck.StatType, rulePerformPartySkillCheck.Difficulty, rulePerformPartySkillCheck.StatValue, isBest: true, null));
					}
				}
			}
			if (HasShowCheck)
			{
				if (baseUnitEntity != null)
				{
					RulePerformSkillCheck rulePerformSkillCheck2 = new RulePerformSkillCheck(baseUnitEntity, ShowCheck.Type, ShowCheck.DC);
					rulePerformSkillCheck2.Calculate(doCheck: false);
					list.Add(new SkillCheckDC(baseUnitEntity, rulePerformSkillCheck2.StatType, rulePerformSkillCheck2.Difficulty, rulePerformSkillCheck2.StatValue, isBest: true, true));
				}
				else
				{
					RulePerformPartySkillCheck rulePerformPartySkillCheck2 = new RulePerformPartySkillCheck(ShowCheck.Type, ShowCheck.DC, CapitalPartyChecksEnabled);
					rulePerformPartySkillCheck2.Calculate(isTrigger: false, doCheck: false);
					list.Add(new SkillCheckDC(rulePerformPartySkillCheck2.Roller, rulePerformPartySkillCheck2.StatType, rulePerformPartySkillCheck2.ResultDC, rulePerformPartySkillCheck2.StatValue, isBest: true, true));
				}
			}
			return list;
		}
	}

	SoulMarkShift ISoulMarkShiftProvider.SoulMarkShift => SoulMarkShift;

	public bool CanShow()
	{
		DialogState dialog = Game.Instance.Player.Dialog;
		DialogController dialogController = Game.Instance.DialogController;
		if (DebugMode && !Debug.isDebugBuild)
		{
			DialogDebug.Add(this, "not in debug build", Color.red);
			return false;
		}
		if (ShowOnce)
		{
			if (ShowOnceCurrentDialog)
			{
				if (dialogController.LocalSelectedAnswers.Contains(this))
				{
					DialogDebug.Add(this, "(show once) was selected before (in current dialog)", Color.red);
					return false;
				}
			}
			else if (dialog.SelectedAnswers.Contains(this))
			{
				DialogDebug.Add(this, "(show once) was selected before (global)", Color.red);
				return false;
			}
		}
		if (HasShowCheck)
		{
			if (dialog.AnswerChecks.TryGetValue(this, out var value))
			{
				if (value == CheckResult.Failed)
				{
					DialogDebug.Add(this, "check failed (before)", Color.red);
					return false;
				}
			}
			else
			{
				RulePerformPartySkillCheck rulePerformPartySkillCheck = Rulebook.Trigger(new RulePerformPartySkillCheck(ShowCheck.Type, ShowCheck.DC, CapitalPartyChecksEnabled));
				dialog.AnswerChecks[this] = ((!rulePerformPartySkillCheck.Success) ? CheckResult.Failed : CheckResult.Passed);
				if (rulePerformPartySkillCheck.Success)
				{
					OnCheckSuccess?.Run();
				}
				else
				{
					OnCheckFail?.Run();
				}
				if (!rulePerformPartySkillCheck.Success)
				{
					DialogDebug.Add(this, "check failed", Color.red);
					return false;
				}
			}
		}
		if (RequireValidCue && NextCue.Select() == null)
		{
			DialogDebug.Add(this, "no valid cue following", Color.red);
			return false;
		}
		if (!ShowConditions.Check(this))
		{
			DialogDebug.Add(this, "conditions failed", Color.red);
			return false;
		}
		DialogDebug.Add(this, "answer shown", Color.green);
		return true;
	}

	public bool CanSelect()
	{
		if (IsSoulMarkRequirementSatisfied() && SelectConditions.Check())
		{
			return IsRequirementsSatisfied();
		}
		return false;
	}

	public IEnumerable<Requirement> GetRequirements()
	{
		return this.GetComponents<Requirement>();
	}

	public IEnumerable<Reward> GetRewards()
	{
		return this.GetComponents<Reward>();
	}

	public bool IsRequirementsSatisfied()
	{
		Colony colony = ContextData<ColonyContextData>.Current?.Colony;
		foreach (Requirement requirement in GetRequirements())
		{
			if (!requirement.Check(colony))
			{
				return false;
			}
		}
		return true;
	}

	public void ApplyRequirements()
	{
		Colony colony = ContextData<ColonyContextData>.Current?.Colony;
		foreach (Requirement requirement in GetRequirements())
		{
			requirement.Apply(colony);
		}
	}

	public void ReceiveRewards()
	{
		Colony colony = ContextData<ColonyContextData>.Current?.Colony;
		foreach (Reward reward in GetRewards())
		{
			reward.ReceiveReward(colony);
		}
	}

	private bool HasExchangeDataOnSelect()
	{
		List<Type> exchangeActions = new List<Type>
		{
			typeof(GainPF),
			typeof(AddItemToPlayer),
			typeof(RemoveItemFromPlayer),
			typeof(GainFactionReputation),
			typeof(GainColonyResources),
			typeof(RemoveColonyResources)
		};
		return OnSelect.Actions.Any((GameAction a) => exchangeActions.Contains(a.GetType()));
	}

	private bool HasConditionsOnSelect()
	{
		List<Type> contextConditions = new List<Type>
		{
			typeof(ConditionHaveFullCargo),
			typeof(ContextConditionHasItem),
			typeof(ContextConditionHasPF)
		};
		if (SelectConditions.HasConditions)
		{
			return SelectConditions.Conditions.Any((Condition c) => contextConditions.Contains(c.GetType()));
		}
		return false;
	}
}
