using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class CargoCollectionLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventCargoCollection>, IGameLogEventHandler<MergeGameLogEvent<GameLogEventCargoCollection>>
{
	public void HandleEvent(GameLogEventCargoCollection evt)
	{
		CombatLogMessage combatLogMessage = GetCombatLogMessage(evt);
		AddMessage(combatLogMessage);
	}

	public void HandleEvent(MergeGameLogEvent<GameLogEventCargoCollection> evt)
	{
		IReadOnlyList<GameLogEventCargoCollection> events = evt.GetEvents();
		GameLogEventCargoCollection evt2 = events[0];
		if (events.Count == 1)
		{
			CombatLogMessage combatLogMessage = GetCombatLogMessage(evt2);
			AddMessage(combatLogMessage);
			return;
		}
		CombatLogMessage combatLogMessage2 = LogThreadBase.Strings.CargoGroup.CreateCombatLogMessage();
		if (combatLogMessage2?.Tooltip is TooltipTemplateCombatLogMessage tooltipTemplateCombatLogMessage)
		{
			ITooltipBrick[] array = CollectExtraBricks(events).ToArray();
			if (array.Length == 0)
			{
				return;
			}
			if (array.Length == 1)
			{
				CombatLogMessage combatLogMessage3 = GetCombatLogMessage(evt2);
				AddMessage(combatLogMessage3);
				return;
			}
			tooltipTemplateCombatLogMessage.ExtraTooltipBricks = array;
			tooltipTemplateCombatLogMessage.ExtraInfoBricks = array;
		}
		AddMessage(combatLogMessage2);
	}

	private static CombatLogMessage GetCombatLogMessage(GameLogEventCargoCollection evt)
	{
		CargoEntity cargo = evt.Cargo;
		int countPercent = evt.CountPercent;
		ItemEntity item = evt.Item;
		ItemsItemOrigin originType = evt.Cargo.Blueprint.OriginType;
		string labelByOrigin = UIStrings.Instance.CargoTexts.GetLabelByOrigin(originType);
		GameLogContext.Text = (string.IsNullOrEmpty(labelByOrigin) ? (cargo.Name ?? string.Empty) : labelByOrigin);
		GameLogContext.SecondText = item?.Name ?? string.Empty;
		GameLogContext.Tooltip = cargo;
		GameLogContext.Count = countPercent;
		return evt.Event switch
		{
			GameLogEventCargoCollection.EventType.CargoCreated => new CombatLogMessage(((countPercent > 0) ? LogThreadBase.Strings.CargoCreatedWithCapacity : LogThreadBase.Strings.CargoCreated).CreateCombatLogMessage(), new TooltipTemplateCargo(cargo.Blueprint)), 
			GameLogEventCargoCollection.EventType.CargoReplenished => new CombatLogMessage(((item != null && item.Blueprint.IsNotable) ? LogThreadBase.Strings.ItemSendToCargo : LogThreadBase.Strings.CargoReplenished).CreateCombatLogMessage(), new TooltipTemplateCargo(cargo.Blueprint)), 
			GameLogEventCargoCollection.EventType.CargoFormed => new CombatLogMessage(((item != null && item.Blueprint.IsNotable) ? LogThreadBase.Strings.ItemSendToCargoAndCargoFormed : LogThreadBase.Strings.CargoFormed).CreateCombatLogMessage(), new TooltipTemplateCargo(cargo.Blueprint)), 
			GameLogEventCargoCollection.EventType.CargoRemoved => new CombatLogMessage(LogThreadBase.Strings.CargoRemoved.CreateCombatLogMessage(), new TooltipTemplateCargo(cargo.Blueprint)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(IEnumerable<GameLogEventCargoCollection> events)
	{
		foreach (GameLogEventCargoCollection @event in events)
		{
			CombatLogMessage combatLogMessage = GetCombatLogMessage(@event);
			if (combatLogMessage != null)
			{
				yield return new TooltipBrickNestedMessage(combatLogMessage);
			}
		}
	}
}
