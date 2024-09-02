using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("e11ac2cb26534343ba689a3fdd4de609")]
public class LocatorOrientation : FloatEvaluator
{
	[SerializeReference]
	public LocatorEvaluator LocatorEval;

	[AllowedEntityType(typeof(LocatorView))]
	public EntityReference Locator;

	public override string GetDescription()
	{
		return "Возвращает угол поворота локатора:\n" + (LocatorEval ? LocatorEval.GetCaption() : Locator.ToString()) + "\n";
	}

	protected override float GetValueInternal()
	{
		IEntityViewBase entityViewBase;
		if (!LocatorEval)
		{
			entityViewBase = Locator.FindView();
		}
		else
		{
			IEntityViewBase entityViewBase2 = LocatorEval.GetValue()?.View;
			entityViewBase = entityViewBase2;
		}
		return entityViewBase?.ViewTransform.rotation.eulerAngles.y ?? 0f;
	}

	public override string GetCaption()
	{
		return "Rotation of " + (LocatorEval ? LocatorEval.GetCaption() : Locator.ToString());
	}
}
