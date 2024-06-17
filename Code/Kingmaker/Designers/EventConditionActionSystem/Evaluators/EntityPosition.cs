using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[Serializable]
[TypeId("877e32af072546a5a3815c22a2fbd0e7")]
public class EntityPosition : PositionEvaluator
{
	[ValidateNotNull]
	public MechanicEntityEvaluator Entity;

	public override string GetCaption()
	{
		return $"Position of {Entity}";
	}

	protected override Vector3 GetValueInternal()
	{
		return Entity.GetValue().Position;
	}
}
