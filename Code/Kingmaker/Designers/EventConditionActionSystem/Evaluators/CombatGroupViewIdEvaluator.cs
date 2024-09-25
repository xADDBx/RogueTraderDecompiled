using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.View.Spawners;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("7105fcbda4a848fda9b58a3595de3e44")]
public class CombatGroupViewIdEvaluator : StringEvaluator
{
	[AllowedEntityType(typeof(UnitGroupView))]
	[ValidateNotEmpty]
	public EntityReference UnitGroupView;

	public override string GetCaption()
	{
		return "Id of group " + UnitGroupView?.EntityNameInEditor;
	}

	protected override string GetValueInternal()
	{
		return UnitGroupView.UniqueId;
	}
}
