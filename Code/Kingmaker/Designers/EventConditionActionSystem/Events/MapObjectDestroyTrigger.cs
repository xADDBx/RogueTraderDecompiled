using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[TypeId("4419fe7ae9d614b428cac75f1e1cef4e")]
public class MapObjectDestroyTrigger : EntityFactComponentDelegate, IDestructionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	public ActionList DestroyedActions;

	public ActionList DestructionFailedActions;

	public void HandleDestructionSuccess(MapObjectView mapObjectView)
	{
		if ((object)mapObjectView != null && mapObjectView.IsOwnerOf(base.Runtime))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				DestroyedActions.Run();
			}
		}
	}

	public void HandleDestructionFail(MapObjectView mapObjectView)
	{
		if ((object)mapObjectView != null && mapObjectView.IsOwnerOf(base.Runtime))
		{
			using (ContextData<InteractingUnitData>.Request().Setup(EventInvokerExtensions.BaseUnitEntity))
			{
				DestructionFailedActions.Run();
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
