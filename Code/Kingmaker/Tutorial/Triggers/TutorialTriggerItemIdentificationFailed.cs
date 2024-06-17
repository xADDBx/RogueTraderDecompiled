using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[ClassInfoBox("`SourceItem` - item that no one in party can identify")]
[TypeId("1d29ab8c46581294aa37b18645916f18")]
public class TutorialTriggerItemIdentificationFailed : TutorialTrigger, IIdentifyHandler, ISubscriber<IItemEntity>, ISubscriber, IHashable
{
	public void OnItemIdentified(BaseUnitEntity character)
	{
	}

	public void OnFailedToIdentify()
	{
		TryToTrigger(null, delegate(TutorialContext x)
		{
			x.SourceItem = EventInvokerExtensions.GetEntity<ItemEntity>();
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
