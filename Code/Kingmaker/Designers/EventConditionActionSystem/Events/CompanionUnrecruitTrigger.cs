using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
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
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[AllowMultipleComponents]
[TypeId("64e3c739b3dc4e38985e9201e8534b5c")]
public class CompanionUnrecruitTrigger : EntityFactComponentDelegate, ICompanionChangeHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("CompanionBlueprint")]
	private BlueprintUnitReference m_CompanionBlueprint;

	public bool TriggerOnDeath;

	public ActionList Actions;

	public BlueprintUnit CompanionBlueprint => m_CompanionBlueprint?.Get();

	public void HandleRecruit()
	{
	}

	public void HandleUnrecruit()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.Blueprint == CompanionBlueprint)
		{
			using (ContextData<RecruitedUnitData>.Request().Setup(baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
	}

	public void HandleUnitDeath()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (TriggerOnDeath && baseUnitEntity != null && baseUnitEntity.Blueprint == CompanionBlueprint && baseUnitEntity.LifeState.IsFinallyDead)
		{
			using (ContextData<RecruitedUnitData>.Request().Setup(baseUnitEntity))
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
