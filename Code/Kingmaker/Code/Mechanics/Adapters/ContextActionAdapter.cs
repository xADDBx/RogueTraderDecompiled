using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Code.Mechanics.Adapters;

[Serializable]
[PlayerUpgraderAllowed(false)]
[TypeId("d09a01fc2701473183eda655aace4e9f")]
public class ContextActionAdapter : GameAction
{
	[SerializeReference]
	[CanBeNull]
	public MechanicEntityEvaluator Caster;

	[SerializeReference]
	[CanBeNull]
	public MechanicEntityEvaluator TargetEntity;

	[SerializeReference]
	[CanBeNull]
	public PositionEvaluator TargetPosition;

	public ActionList Actions;

	public override string GetCaption()
	{
		return $"Setup context: Caster [{Caster}]; Target [{TargetEntity}, {TargetPosition}]";
	}

	protected override void RunAction()
	{
		if (!(base.Owner is BlueprintScriptableObject blueprint))
		{
			throw new Exception("Valid Blueprint is missing");
		}
		MechanicEntity obj = Caster?.GetValue() ?? SimpleCaster.GetFree();
		MechanicEntity mechanicEntity = TargetEntity?.GetValue();
		Vector3? vector = TargetPosition?.GetValue();
		if (obj == null)
		{
			throw new Exception("Caster is missing");
		}
		if (mechanicEntity == null && !vector.HasValue)
		{
			throw new Exception("Target is missing");
		}
		TargetWrapper targetWrapper = ((!vector.HasValue) ? new TargetWrapper(mechanicEntity) : ((mechanicEntity == null) ? new TargetWrapper(vector.Value) : new TargetWrapper(vector.Value, null, mechanicEntity)));
		using (new MechanicsContext(obj, obj, blueprint, null, targetWrapper).GetDataScope(targetWrapper))
		{
			Actions.Run();
		}
	}
}
