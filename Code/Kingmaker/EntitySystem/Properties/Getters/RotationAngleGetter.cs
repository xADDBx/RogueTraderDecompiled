using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("6c65a9a9d3c7dba45b61b518d00f0d87")]
public class RotationAngleGetter : PropertyGetter
{
	[ValidateNotNull]
	[SerializeReference]
	public PositionEvaluator From;

	[ValidateNotNull]
	[SerializeReference]
	public PositionEvaluator To;

	[SerializeField]
	public bool Assume0as1;

	protected override int GetBaseValue()
	{
		if (!From.TryGetValue(out var value) || !To.TryGetValue(out var value2))
		{
			return 0;
		}
		int num = (int)Vector3.SignedAngle(value, value2, Vector3.up);
		if (num == 0 && Assume0as1)
		{
			num = 1;
		}
		return num;
	}

	protected override string GetInnerCaption()
	{
		return $"Rotation angle from {From} to {To}";
	}
}
