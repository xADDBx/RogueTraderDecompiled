using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View;
using Owlcat.QA.Validation;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("e2e5f250da682644dbc648ec03f805ac")]
public class LocatorReference : LocatorEvaluator
{
	[AllowedEntityType(typeof(LocatorView))]
	[ValidateNotEmpty]
	public EntityReference Locator;

	protected override LocatorEntity GetValueInternal()
	{
		if (Locator == null)
		{
			return null;
		}
		IEntityViewBase entityViewBase = Locator.FindView();
		if (entityViewBase == null)
		{
			return null;
		}
		return entityViewBase.Data as LocatorEntity;
	}

	public override string GetCaption()
	{
		return Locator?.ToString() ?? "";
	}
}
