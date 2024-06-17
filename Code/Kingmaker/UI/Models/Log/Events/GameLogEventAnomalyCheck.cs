using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventAnomalyCheck : GameLogEvent<GameLogEventAnomalyCheck>
{
	public enum CheckType
	{
		Checked,
		InteractionSucceed,
		InteractionFailed
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IAnomalyHandler, ISubscriber<AnomalyEntityData>, ISubscriber, IAnomalyResearchHandler
	{
		private void AddEvent(CheckType type, AnomalyEntityData anomaly, BaseUnitEntity unit, RulePerformSkillCheck skillCheck)
		{
			AddEvent(new GameLogEventAnomalyCheck(type, anomaly, unit, skillCheck));
		}

		public void HandleAnomalyStartResearch()
		{
		}

		public void HandleAnomalyResearched(BaseUnitEntity unit, RulePerformSkillCheck skillCheck)
		{
			AddEvent(skillCheck.ResultIsSuccess ? CheckType.InteractionSucceed : CheckType.InteractionFailed, EventInvokerExtensions.GetEntity<AnomalyEntityData>(), unit, skillCheck);
		}

		public void HandleAnomalyInteracted()
		{
		}
	}

	public readonly CheckType Type;

	public readonly AnomalyEntityData Anomaly;

	public readonly BaseUnitEntity Actor;

	public readonly RulePerformSkillCheck SkillCheckRule;

	public GameLogEventAnomalyCheck(CheckType type, AnomalyEntityData anomaly, BaseUnitEntity actor, RulePerformSkillCheck skillCheckRule)
	{
		Type = type;
		Anomaly = anomaly;
		Actor = actor;
		SkillCheckRule = skillCheckRule;
	}
}
