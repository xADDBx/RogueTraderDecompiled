using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.View.Mechanics;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("a4aa1729f3bc4ff8bfffb1cc9d02c662")]
[PlayerUpgraderAllowed(true)]
public class MechanicEntityFromScene : MechanicEntityEvaluator
{
	[AllowedEntityType(typeof(MechanicEntityView))]
	[ValidateNotEmpty]
	public EntityReference EntityRef;

	protected override Entity GetValueInternal()
	{
		return EntityRef.FindData() as MechanicEntity;
	}

	public override string GetCaptionShort()
	{
		return EntityRef?.ToString();
	}

	public override string GetCaption()
	{
		return "Object from scene " + EntityRef;
	}
}
