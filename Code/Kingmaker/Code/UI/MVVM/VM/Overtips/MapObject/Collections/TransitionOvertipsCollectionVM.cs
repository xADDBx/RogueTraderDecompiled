using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject.Collections;

public class TransitionOvertipsCollectionVM : BaseMapObjectOvertipsCollectionVM<OvertipTransitionVM>, IInteractionObjectUIHandler, ISubscriber<IMapObjectEntity>, ISubscriber
{
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
		return mapObjectEntity.View.GetComponent<AreaTransition>() != null;
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
		GetOvertip(GetRevealedMapObject())?.OnClick();
	}
}
