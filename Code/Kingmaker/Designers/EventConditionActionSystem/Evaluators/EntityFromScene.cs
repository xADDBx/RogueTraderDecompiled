using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("05a1b93328d24abbbedae76d461dae54")]
public class EntityFromScene : EntityEvaluator
{
	[AllowedEntityType(typeof(EntityViewBase))]
	[ValidateNotEmpty]
	public EntityReference EntityReference;

	public override string GetCaption()
	{
		return "Entity " + EntityReference?.EntityNameInEditor;
	}

	protected override Entity GetValueInternal()
	{
		return EntityReference.FindData() as Entity;
	}
}
