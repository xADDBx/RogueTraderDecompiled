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
[TypeId("15415d6ca28148d2a017989e562f7cbc")]
public class CompanionRecruitTrigger : EntityFactComponentDelegate, ICompanionChangeHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("CompanionBlueprint")]
	private BlueprintUnitReference m_CompanionBlueprint;

	public ActionList Actions;

	public BlueprintUnit CompanionBlueprint => m_CompanionBlueprint?.Get();

	public void HandleRecruit()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity.Blueprint == CompanionBlueprint)
		{
			using (ContextData<RecruitedUnitData>.Request().Setup(baseUnitEntity))
			{
				Actions.Run();
			}
		}
	}

	public void HandleUnrecruit()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
