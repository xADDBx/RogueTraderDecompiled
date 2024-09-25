using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("424148b2b36840229eae734ef5e9b012")]
public class LastAttackPosition : PositionEvaluator
{
	[ValidateNotNull]
	public MechanicEntityEvaluator Entity;

	public override string GetCaption()
	{
		return $"Last attack position of {Entity}";
	}

	protected override Vector3 GetValueInternal()
	{
		return Entity.GetValue().GetLastAttackPosition() ?? Entity.GetValue().Position;
	}
}
