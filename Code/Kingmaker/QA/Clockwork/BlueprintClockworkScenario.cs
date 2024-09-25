using System.Collections.Generic;
using System.ComponentModel;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

[ExcludeBlueprint]
[TypeId("95c1e2e3833476d4985e5672b2608a94")]
public class BlueprintClockworkScenario : BlueprintScriptableObject
{
	public enum ScenarioQA
	{
		[Description("nasibullin")]
		Unknown,
		[Description("vertyankin")]
		AlexanderVertyankin,
		[Description("cherkasov")]
		IlyaCherkasov,
		[Description("bogomazov")]
		DmitryBogomazov,
		[Description("nasibullin")]
		OscarNasibullin
	}

	public enum ClockworkTestMode
	{
		Replay,
		Exploration
	}

	public enum StartMode
	{
		FromCurrentPosition,
		FromPreset,
		FromLocalSave,
		FromRemoteSave
	}

	[HideInInspector]
	[SerializeField]
	private string m_ScenarioName;

	public ScenarioQA ScenarioAuthor;

	public ClockworkTestMode TestMode;

	[Header("Scenario start")]
	public StartMode startMode;

	[ShowIf("IsStartFromPreset")]
	public BlueprintAreaPresetReference Preset;

	[ShowIf("IsStartFromSave")]
	[SaveFilePicker]
	public string Save;

	[ShowIf("IsStartFromRemoteSave")]
	public string RemoteSave;

	[ShowIf("IsStartFromPreset")]
	public DifficultyPresetAsset OverridePresetDifficulty;

	[Header("Other Settings")]
	public float OnAreaEnterDelay = 1f;

	public float AreaTimeout = 600f;

	public float CutsceneTimeout = 180f;

	public float TaskTimeout = 10f;

	public int TaskMaxAttempts = 2;

	public int AreaGameOverLimit = 3;

	public bool AutoLevelUp;

	public bool AutoUseRest = true;

	public bool CheaterCombat = true;

	public List<EntityReference> RetrySkillChecks;

	public List<BlueprintItemReference> DoNotSellItems;

	public List<EntityReference> DoNotInterract;

	public List<BlueprintAnswerReference> DoNotUseAnswer;

	public List<BlueprintAreaReference> DoNotEnterAreas;

	private Dictionary<BlueprintArea, List<AreaTest>> m_AreaTests;

	private List<ConditionalCommandList> m_ConditionalCommandLists;

	public string ScenarioName => m_ScenarioName;

	public bool IsExplorationMode => TestMode == ClockworkTestMode.Exploration;

	public bool IsReplayMode => TestMode == ClockworkTestMode.Replay;

	public bool IsStartFromCurrentPosition => startMode == StartMode.FromCurrentPosition;

	public bool IsStartFromPreset => startMode == StartMode.FromPreset;

	public bool IsStartFromSave => startMode == StartMode.FromLocalSave;

	public bool IsStartFromRemoteSave => startMode == StartMode.FromRemoteSave;

	public void Initialize()
	{
		PFLog.Clockwork.Log("BlueprintClockworkScenario.Initialize()");
		if (AutoLevelUp)
		{
			SettingsRoot.Difficulty.AutoLevelUp.SetValueAndConfirm(AutoLevelUpOption.AllPossible);
		}
		m_AreaTests = new Dictionary<BlueprintArea, List<AreaTest>>();
		m_ConditionalCommandLists = new List<ConditionalCommandList>();
		BlueprintComponent[] componentsArray = base.ComponentsArray;
		foreach (BlueprintComponent obj in componentsArray)
		{
			if (obj is AreaTest areaTest)
			{
				areaTest.CommandList.Initialize();
				if (m_AreaTests.TryGetValue(areaTest.Area, out var value))
				{
					value.Add(areaTest);
				}
				else
				{
					m_AreaTests.Add(areaTest.Area, new List<AreaTest> { areaTest });
				}
			}
			if (obj is ConditionalCommandList conditionalCommandList)
			{
				conditionalCommandList.CommandList.Initialize();
				m_ConditionalCommandLists.Add(conditionalCommandList);
			}
		}
	}

	public ClockworkCommandList GetCommandList()
	{
		if (Game.Instance.CurrentlyLoadedArea != null && m_AreaTests.TryGetValue(Game.Instance.CurrentlyLoadedArea, out var value))
		{
			foreach (AreaTest item in value)
			{
				item.CommandList.UpdateComplited();
				if (!item.CommandList.IsCompleted)
				{
					return item.CommandList;
				}
			}
		}
		foreach (ConditionalCommandList conditionalCommandList in m_ConditionalCommandLists)
		{
			conditionalCommandList.CommandList.UpdateComplited();
			if (!conditionalCommandList.CommandList.IsCompleted && conditionalCommandList.Condition.Check())
			{
				return conditionalCommandList.CommandList;
			}
		}
		return null;
	}

	public bool IsSkillCheckRetriable(MapObjectEntity mapObjectEntityData)
	{
		return RetrySkillChecks?.Any((EntityReference x) => x.FindData() == mapObjectEntityData) ?? false;
	}

	public bool CanSellItem(BlueprintItem item)
	{
		return !DoNotSellItems.HasItem((BlueprintItemReference x) => x.Get() == item);
	}

	public bool CanInterractMapObject(MapObjectEntity entityDataBase)
	{
		return !DoNotInterract.HasItem((EntityReference x) => x.FindData() == entityDataBase);
	}

	public bool CanUseAnswer(BlueprintAnswer answer)
	{
		return !DoNotUseAnswer.HasItem((BlueprintAnswerReference x) => x.Get() == answer);
	}

	public bool CanEnterArea(BlueprintArea area)
	{
		return !DoNotEnterAreas.HasItem((BlueprintAreaReference x) => x.Get() == area);
	}

	public override void OnEnable()
	{
		base.OnEnable();
	}
}
