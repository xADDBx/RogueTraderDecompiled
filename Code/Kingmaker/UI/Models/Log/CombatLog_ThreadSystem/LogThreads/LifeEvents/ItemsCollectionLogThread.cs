using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.Tooltips;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class ItemsCollectionLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventItemsCollection>, IGameLogEventHandler<MergeGameLogEvent<GameLogEventItemsCollection>>
{
	public void HandleEvent(GameLogEventItemsCollection evt)
	{
		CombatLogMessage combatLogMessage = GetCombatLogMessage(evt);
		AddMessage(combatLogMessage);
	}

	public void HandleEvent(MergeGameLogEvent<GameLogEventItemsCollection> evt)
	{
		IReadOnlyList<GameLogEventItemsCollection> events = evt.GetEvents();
		GameLogEventItemsCollection evt2 = events[0];
		if (events.Count == 1)
		{
			CombatLogMessage combatLogMessage = GetCombatLogMessage(evt2);
			AddMessage(combatLogMessage);
			return;
		}
		CombatLogMessage combatLogMessage2 = LogThreadBase.Strings.ItemGroup.CreateCombatLogMessage();
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

	private static CombatLogMessage GetCombatLogMessage(GameLogEventItemsCollection evt)
	{
		if (TurnController.IsInTurnBasedCombat())
		{
			return null;
		}
		ItemEntity item = evt.Item;
		int count = evt.Count;
		ItemEntity itemEntity;
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			itemEntity = ItemsEntityFactory.CreateItemCopy(item, count);
		}
		GameLogContext.Text = item.Name;
		GameLogContext.Tooltip = itemEntity;
		GameLogContext.Count = count;
		switch (evt.Event)
		{
		case GameLogEventItemsCollection.EventType.Added:
			return new CombatLogMessage(((count > 1) ? LogThreadBase.Strings.ItemsGained : LogThreadBase.Strings.ItemGained).CreateCombatLogMessage(), new TooltipTemplateItem(itemEntity));
		case GameLogEventItemsCollection.EventType.Removed:
			GameLogContext.Description = item.Name;
			return new CombatLogMessage(((count > 1) ? LogThreadBase.Strings.ItemsLost : LogThreadBase.Strings.ItemLost).CreateCombatLogMessage(), new TooltipTemplateItem(itemEntity));
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static IEnumerable<ITooltipBrick> CollectExtraBricks(IEnumerable<GameLogEventItemsCollection> events)
	{
		foreach (GameLogEventItemsCollection @event in events)
		{
			CombatLogMessage combatLogMessage = GetCombatLogMessage(@event);
			if (combatLogMessage != null)
			{
				yield return new TooltipBrickNestedMessage(combatLogMessage);
			}
		}
	}
}
