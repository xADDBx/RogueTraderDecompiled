using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;

public class DestructibleObjectOvertipsCollectionVM : BaseMapObjectOvertipsCollectionVM<OvertipDestructibleObjectVM>, IInteractionObjectUIHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IDestructibleEntityHandler
{
	protected override bool NeedOvertip(Entity entityData)
	{
		return entityData is DestructibleEntity;
	}

	public void HandleObjectHighlightChange()
	{
		GetOvertip(GetRevealedMapObject())?.HighlightChanged();
	}

	public void HandleObjectInteractChanged()
	{
	}

	public void HandleObjectInteract()
	{
	}

	public void ShowBark(Entity entity, string text)
	{
		GetOvertip(entity)?.ShowBark(text);
	}

	public void HideBark(Entity entity)
	{
		GetOvertip(entity)?.HideBark();
	}

	public void HandleDestructionStageChanged(DestructionStage stage)
	{
		if (stage == DestructionStage.Destroyed)
		{
			RemoveEntity(EventInvokerExtensions.GetEntity<DestructibleEntity>());
		}
	}
}
