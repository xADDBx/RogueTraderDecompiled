using System;
using System.Collections.Generic;
using Kingmaker.Designers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Cheats;

internal class CheatsStats : IGlobalRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, ISubscriber, IGlobalRulebookSubscriber
{
	private static readonly CheatsStats s_Instance = new CheatsStats();

	public Dictionary<StatType, Dictionary<string, int>> SkillChecks => Game.Instance.Player.SkillChecks;

	public CheatsStats()
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			EventBus.Subscribe(this);
		}
	}

	public static void RegisterCommands()
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			SmartConsole.RegisterCommand("set_stat", "Setting stat on selected users; If zero characters selected setting stat on main character; To see all paramater call command without any", SetStat);
			SmartConsole.RegisterCommand("dump_skillchecks_info", DumpSkillchecksInfo);
		}
	}

	private static void SetStat(string parameters)
	{
		string paramString = Utilities.GetParamString(parameters, 1, null);
		if (paramString == null)
		{
			PFLog.SmartConsole.Log("Parameters are:");
		}
		foreach (StatType value in Enum.GetValues(typeof(StatType)))
		{
			if (paramString == null)
			{
				PFLog.SmartConsole.Log(value.ToString());
			}
			else
			{
				if (!value.ToString().Equals(paramString))
				{
					continue;
				}
				int? paramInt = Utilities.GetParamInt(parameters, 2, "Can't parse quantity name from given parameters");
				List<BaseUnitEntity> list = new List<BaseUnitEntity>(Game.Instance.SelectionCharacter.SelectedUnits);
				if (list.Count == 0)
				{
					list.Add(GameHelper.GetPlayerCharacter());
				}
				{
					foreach (BaseUnitEntity item in list)
					{
						item.Stats.GetStat(value).BaseValue = paramInt ?? 1;
					}
					return;
				}
			}
		}
		PFLog.SmartConsole.Log("Can't find stat with name: " + paramString);
	}

	private static void DumpSkillchecksInfo(string parameters)
	{
		string text = "";
		PFLog.SmartConsole.Log("SkillCheck stats are: ");
		foreach (KeyValuePair<StatType, Dictionary<string, int>> skillCheck in s_Instance.SkillChecks)
		{
			string text2 = string.Format("{0}: passed {1}, failed {2}", skillCheck.Key, skillCheck.Value.Get("passed", 0), skillCheck.Value.Get("failed", 0));
			PFLog.SmartConsole.Log(text2);
			text = text + text2 + "\n";
		}
		GUIUtility.systemCopyBuffer = text;
		PFLog.Default.Log("Skill check stats copied in buffer.");
		bool? paramBool = Utilities.GetParamBool(parameters, 1, null);
		if (!paramBool.HasValue || paramBool.Value)
		{
			s_Instance.SkillChecks.Clear();
			PFLog.Default.Log("Stats was purged");
		}
	}

	public void OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSkillCheck evt)
	{
		if (BuildModeUtility.IsDevelopment)
		{
			string key = (evt.ResultIsSuccess ? "passed" : "failed");
			if (!SkillChecks.ContainsKey(evt.StatType))
			{
				SkillChecks.Add(evt.StatType, new Dictionary<string, int>());
			}
			if (!SkillChecks.Get(evt.StatType).ContainsKey(key))
			{
				SkillChecks.Get(evt.StatType).Add(key, 0);
			}
			SkillChecks.Get(evt.StatType)[key] = SkillChecks.Get(evt.StatType).Get(key, 0) + 1;
		}
	}
}
