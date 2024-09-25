using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.SystemMap;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[PlayerUpgraderAllowed(true)]
[TypeId("194956a0b3d319f4483778c51d2b3d10")]
public class StarSystemObjectOnScene : MechanicEntityEvaluator, IEvaluator<StarSystemObjectEntity>
{
	[AllowedEntityType(typeof(StarSystemObjectView))]
	[ValidateNotEmpty]
	public EntityReference StarSystemObject;

	public override string GetCaption()
	{
		return "StarSystem object that is on scene";
	}

	protected override Entity GetValueInternal()
	{
		StarSystemObjectView starSystemObjectView = StarSystemObject.FindView() as StarSystemObjectView;
		if (!(starSystemObjectView != null))
		{
			return null;
		}
		return starSystemObjectView.Data;
	}

	public new StarSystemObjectEntity GetValue()
	{
		return (StarSystemObjectEntity)base.GetValue();
	}

	public bool TryGetValue(out StarSystemObjectEntity value)
	{
		if (TryGetValue(out MechanicEntity value2))
		{
			value = value2 as StarSystemObjectEntity;
			return value != null;
		}
		value = null;
		return false;
	}
}
