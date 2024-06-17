using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("05a1b93328d24abbbedae76d461dae54")]
public class SpawnerEntityEvaluator : EntityEvaluator
{
	[AllowedEntityType(typeof(EntityViewBase))]
	[ValidateNotEmpty]
	public EntityReference EntityReference;

	public override string GetCaption()
	{
		return "Spawner " + EntityReference?.EntityNameInEditor;
	}

	protected override Entity GetValueInternal()
	{
		return EntityReference.FindData() as Entity;
	}
}
