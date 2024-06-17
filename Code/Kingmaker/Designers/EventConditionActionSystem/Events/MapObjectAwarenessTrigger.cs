using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[TypeId("b2afa2f2aade1cb42a50cc4dc13925de")]
public class MapObjectAwarenessTrigger : EntityFactComponentDelegate, IAwarenessHandler, ISubscriber<IMapObjectEntity>, ISubscriber, IHashable
{
	public ActionList Actions;

	public void OnEntityNoticed(BaseUnitEntity character)
	{
		MapObjectView view = EventInvokerExtensions.GetEntity<MapObjectEntity>().View;
		if (view == null || !view.IsOwnerOf(base.Runtime))
		{
			return;
		}
		using (ContextData<SpotterData>.Request().Setup(character))
		{
			using (ContextData<MechanicEntityData>.Request().Setup(view.Data))
			{
				Actions.Run();
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
