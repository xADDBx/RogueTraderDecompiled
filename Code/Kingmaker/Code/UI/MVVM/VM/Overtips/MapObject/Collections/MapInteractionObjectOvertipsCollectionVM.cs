using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;

public class MapInteractionObjectOvertipsCollectionVM : BaseMapObjectOvertipsCollectionVM<OvertipMapObjectVM>, IInteractionHighlightUIHandler, ISubscriber, IInteractionObjectUIHandler, ISubscriber<IMapObjectEntity>, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, IUnitCommandEndHandler, IPartyCombatHandler, ITurnBasedModeHandler, ITurnBasedModeResumeHandler, IItemsCollectionHandler, IEntityRevealedHandler, ISubscriber<IEntity>
{
	protected override void AddEntity(Entity entityData)
	{
		if (NeedOvertip(entityData) && !ContainsOvertip(entityData))
		{
			OvertipMapObjectVM item = new OvertipMapObjectVM(entityData as MapObjectEntity);
			Overtips.Add(item);
		}
	}

	protected override bool NeedOvertip(Entity entityData)
	{
		if (!(entityData is MapObjectEntity mapObjectEntity))
		{
			return false;
		}
		if (mapObjectEntity.View == null)
		{
			return false;
		}
		return OvertipMapObjectVM.CheckNeedOvertip(mapObjectEntity);
	}

	protected override void RescanEntities()
	{
		base.RescanEntities();
		HandlePartyCombatStateChanged(Game.Instance.Player.IsInCombat);
		if (Game.Instance.InteractionHighlightController != null)
		{
			HandleHighlightChange(Game.Instance.InteractionHighlightController.IsHighlighting);
		}
	}

	public void ShowBark(Entity entity, string text)
	{
		GetOvertip(entity)?.ShowBark(text);
	}

	public void HideBark(Entity entity)
	{
		GetOvertip(entity)?.HideBark();
	}

	public void HandleHighlightChange(bool isOn)
	{
		Overtips.ForEach(delegate(OvertipMapObjectVM o)
		{
			o.HandleHighlightChange(isOn);
		});
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		Overtips.ForEach(delegate(OvertipMapObjectVM o)
		{
			o.HandleCombatStateChanged();
		});
	}

	public void HandleObjectHighlightChange()
	{
		GetOvertip(GetRevealedMapObject())?.HighlightChanged();
	}

	public void HandleObjectInteractChanged()
	{
		GetOvertip(GetMapObject())?.UpdateObjectData();
	}

	public void HandleObjectInteract()
	{
		GetOvertip(GetRevealedMapObject())?.Interact();
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		UpdateMapObjectInteraction(command, active: true);
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateMapObjectInteraction(command, active: false);
	}

	private void UpdateMapObjectInteraction(AbstractUnitCommand command, bool active)
	{
		if (command is UnitInteractWithObject unitInteractWithObject)
		{
			GetOvertip(unitInteractWithObject.Interaction.Owner)?.UpdateInteraction(active);
		}
	}

	public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count)
	{
		if (IsInteractionItem(item))
		{
			Overtips.Where((OvertipMapObjectVM o) => o.RequiredResourceCount.HasValue).ForEach(delegate(OvertipMapObjectVM o)
			{
				o?.InventoryChanged?.Execute();
			});
		}
	}

	public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count)
	{
		if (IsInteractionItem(item))
		{
			Overtips.Where((OvertipMapObjectVM o) => o.RequiredResourceCount.HasValue).ForEach(delegate(OvertipMapObjectVM o)
			{
				o?.InventoryChanged?.Execute();
			});
		}
	}

	private bool IsInteractionItem(ItemEntity item)
	{
		if (item.Blueprint != Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MultikeyItem && item.Blueprint != Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MeltaChargeItem)
		{
			return item.Blueprint == Game.Instance.BlueprintRoot.SystemMechanics.Consumables.RitualSetItem;
		}
		return true;
	}

	public void HandleEntityRevealed()
	{
		GetOvertip(GetMapObject())?.UpdateObjectData();
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		Overtips.ForEach(delegate(OvertipMapObjectVM o)
		{
			o.HandleCombatStateChanged();
		});
	}

	public void HandleTurnBasedModeResumed()
	{
		Overtips.ForEach(delegate(OvertipMapObjectVM o)
		{
			o.HandleCombatStateChanged();
		});
	}
}
