using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.StatefulRandom;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.LifeEvents;

public class UnitEquipmentLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventUnitEquipment>
{
	public void HandleEvent(GameLogEventUnitEquipment evt)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			ItemSlot slot = evt.Slot;
			ItemEntity previousItem = evt.PreviousItem;
			using (GameLogContext.Scope)
			{
				if (slot.Owner.IsPlayerFaction && slot.Owner.IsInState && slot.Owner.IsInGame && slot.Owner.IsInCompanionRoster() && !(slot.Owner?.GetBodyOptional()?.AdditionalLimbs?.Contains(slot)).GetValueOrDefault())
				{
					if (slot.MaybeItem != null)
					{
						ItemEntity itemEntity = ItemsEntityFactory.CreateItemCopy(slot.MaybeItem, 1);
						GameLogContext.Tooltip = itemEntity;
						GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)slot.Owner;
						GameLogContext.Text = slot.MaybeItem.Name;
						GameLogContext.Description = slot.MaybeItem.Description;
						AddMessage(new CombatLogMessage(LogThreadBase.Strings.ItemEquipped.CreateCombatLogMessage(), new TooltipTemplateItem(itemEntity)));
					}
					else if (previousItem != null)
					{
						ItemEntity itemEntity = ItemsEntityFactory.CreateItemCopy(previousItem, 1);
						GameLogContext.Tooltip = itemEntity;
						GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)slot.Owner;
						GameLogContext.Text = previousItem.Name;
						GameLogContext.Description = previousItem.Description;
						AddMessage(new CombatLogMessage(LogThreadBase.Strings.ItemUnequipped.CreateCombatLogMessage(), new TooltipTemplateItem(itemEntity)));
					}
				}
			}
		}
	}
}
