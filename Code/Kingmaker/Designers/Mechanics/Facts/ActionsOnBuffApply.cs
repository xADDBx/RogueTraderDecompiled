using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[ComponentName("Apply fact of event of applying another fact")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("d7bd163f6687c15448553fba4ad389ac")]
public class ActionsOnBuffApply : UnitBuffComponentDelegate, IEntityGainFactHandler<EntitySubscriber>, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IEntityGainFactHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("GainedFact")]
	private BlueprintUnitFactReference m_GainedFact;

	public ActionList Actions;

	public bool UseFactContext;

	public BlueprintUnitFact GainedFact => m_GainedFact?.Get();

	public void HandleEntityGainFact(EntityFact fact)
	{
		if (fact.Blueprint == GainedFact)
		{
			if (!UseFactContext)
			{
				Actions.Run();
			}
			else
			{
				base.Fact.RunActionInContext(Actions);
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
