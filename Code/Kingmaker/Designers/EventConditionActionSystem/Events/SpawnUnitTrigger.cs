using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[ComponentName("Events/SpawnUnit Trigger")]
[AllowMultipleComponents]
[TypeId("88605805d0fccd846997f790c580f063")]
public class SpawnUnitTrigger : EntityFactComponentDelegate, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	[ValidateNotNull]
	[SerializeField]
	public BlueprintUnitReference m_TargetUnit;

	public ActionList Actions;

	void IUnitSpawnHandler.HandleUnitSpawned()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.Blueprint.Equals(m_TargetUnit.Get()))
		{
			Actions.Run();
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
