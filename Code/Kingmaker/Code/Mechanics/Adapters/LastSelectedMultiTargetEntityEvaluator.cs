using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[TypeId("f61925e0ab2b4f7da6aeff9d1a5f0b98")]
public class LastSelectedMultiTargetEntityEvaluator : MechanicEntityEvaluator
{
	public override string GetCaption()
	{
		return "Evaluate last selected target";
	}

	protected override Entity GetValueInternal()
	{
		return Game.Instance.SelectedAbilityHandler?.MultiTargetHandler.GetLastTarget()?.Entity;
	}
}
