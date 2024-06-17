using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Combat;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Dialog;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility.Locator;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;

public class LogThreadService : IService, IDisposable
{
	private Dictionary<LogChannelType, List<LogThreadBase>> m_Logs = new Dictionary<LogChannelType, List<LogThreadBase>>
	{
		{
			LogChannelType.Common,
			new List<LogThreadBase>
			{
				new RulebookRollChanceLogThread(),
				new InteractionRestrictionLogThread(),
				new KnowledgeLogThread(),
				new PartyEncumbranceLogThread(),
				new UnitEncumbranceLogThread(),
				new AwarenessLogThread(),
				new GameTimeAdvancedLogThread(),
				new WarningNotificationLogThread(),
				new UnitFakeDeathMessageLogThread(),
				new AnomalyCheckLogThread(),
				new FactionReputationLogThread(),
				new VeilThicknessLogThread(),
				new ColonyCreateLogThread(),
				new ColonyStatChangeLogThread(),
				new ProfitFactorLogThread(),
				new NavigatorResourceLogThread(),
				new ColonyResourcesLogThread(),
				new ColonyProjectLogThread(),
				new ColonyChronicleLogThread()
			}
		},
		{
			LogChannelType.AnyCombat,
			new List<LogThreadBase>
			{
				new RulebookDealDamageLogThread(),
				new UnitInitiativeLogThread(),
				new UnitMissedTurnLogThread(),
				new InterruptCurrentTurnLogThread(),
				new HealingLogThread(),
				new RollSkillCheckLogThread(),
				new RulebookCastSpellLogThread(),
				new PartyUseAbilityLogThread(),
				new RulebookSavingThrowLogThread(),
				new RulePerformMomentumChangeLogThread(),
				new AddSeparatorLogThread(),
				new MergeRulePerformSavingThrowLogThread()
			}
		},
		{
			LogChannelType.TacticalCombat,
			new List<LogThreadBase>
			{
				new WarningNotificationLogThread()
			}
		},
		{
			LogChannelType.InGameCombat,
			new List<LogThreadBase>
			{
				new UnitLifeStateChangedLogThread(),
				new PerformAttackLogThread(),
				new GrenadeDealDamageLogThread(),
				new PerformScatterAttackLogThread(),
				new RuleRollScatterShotHitDirectionLogThread(),
				new RulebookPerformStarshipAttackLogThread(),
				new RulebookDealStatDamageLogThread(),
				new RulebookHealStatDamageLogThread(),
				new PickLockLogThread(),
				new DisarmTrapLogThread(),
				new RulebookCanApplyBuffLogThread(),
				new RulebookRollCoverHit(),
				new MergeRuleCalculateCanApplyBuffLogThread(),
				new ContextActionKillLogThread()
			}
		},
		{
			LogChannelType.LifeEvents,
			new List<LogThreadBase>
			{
				new UnitEquipmentLogThread(),
				new ItemsCollectionLogThread(),
				new FactsCollectionLogThread(),
				new CargoCollectionLogThread(),
				new ScrapCollectionLogThread(),
				new PartyGainExperienceLogThread(),
				new PartyCombatLogThread(),
				new IdentifyLogThread(),
				new UnitGainExperienceLogThread(),
				new SpaceCombatExperienceGainedPerAreaLogThread(),
				new StarshipExpToNextLevelLogThread(),
				new StarshipLevelUpLogThread(),
				new HealWoundOrTraumaLogThread()
			}
		},
		{
			LogChannelType.Dialog,
			new List<LogThreadBase>
			{
				new CombatLogBarkLogThread(),
				new DialogHistoryLogThread(),
				new DialogLogThread()
			}
		},
		{
			LogChannelType.DialogAndLife,
			new List<LogThreadBase>()
		}
	};

	private static ServiceProxy<LogThreadService> s_Proxy;

	public static LogThreadService Instance
	{
		get
		{
			s_Proxy = ((s_Proxy?.Instance != null) ? s_Proxy : Services.GetProxy<LogThreadService>());
			return s_Proxy?.Instance;
		}
	}

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public IEnumerable<LogThreadBase> AllThreads => m_Logs.SelectMany((KeyValuePair<LogChannelType, List<LogThreadBase>> i) => i.Value);

	public LogThreadService()
	{
		foreach (KeyValuePair<LogChannelType, List<LogThreadBase>> log in m_Logs)
		{
			foreach (LogThreadBase item in log.Value)
			{
				item.StartThread();
			}
		}
	}

	public List<LogThreadBase> GetThreadsByChannelType(params LogChannelType[] channelType)
	{
		List<LogThreadBase> list = new List<LogThreadBase>();
		foreach (LogChannelType key in channelType)
		{
			list.AddRange(m_Logs[key]);
		}
		return list;
	}

	public void Dispose()
	{
		m_Logs.ForEach(delegate(KeyValuePair<LogChannelType, List<LogThreadBase>> z)
		{
			z.Value.ForEach(delegate(LogThreadBase x)
			{
				x.Dispose();
			});
		});
		m_Logs.Clear();
		m_Logs = null;
	}

	public void Cleanup()
	{
		m_Logs.ForEach(delegate(KeyValuePair<LogChannelType, List<LogThreadBase>> z)
		{
			z.Value.ForEach(delegate(LogThreadBase x)
			{
				x.Cleanup();
			});
		});
	}
}
