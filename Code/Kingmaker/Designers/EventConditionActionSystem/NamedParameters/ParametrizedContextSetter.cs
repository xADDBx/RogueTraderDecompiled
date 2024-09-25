using System;
using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.NamedParameters;

[Serializable]
public class ParametrizedContextSetter
{
	public enum ParameterType
	{
		Unit,
		Locator,
		MapObject,
		Position,
		Blueprint,
		Float
	}

	public class ParamEvaluatorAttribute : PropertyAttribute
	{
	}

	[Serializable]
	public class ParameterEntry
	{
		public string Name;

		[InfoBox(Text = "Use float param for set rotation in cutscene")]
		public ParameterType Type;

		[ParamEvaluator]
		[SerializeReference]
		public Element Evaluator;

		public object GetValue()
		{
			return Type switch
			{
				ParameterType.Unit => (Evaluator is AbstractUnitEvaluator abstractUnitEvaluator) ? ((object)UnitReference.FromIAbstractUnitEntity(abstractUnitEvaluator.GetValue())) : null, 
				ParameterType.Locator => (Evaluator is LocatorEvaluator locatorEvaluator) ? locatorEvaluator.GetValue() : null, 
				ParameterType.MapObject => (Evaluator is MapObjectEvaluator mapObjectEvaluator) ? mapObjectEvaluator.GetValue() : null, 
				ParameterType.Position => (Evaluator is PositionEvaluator positionEvaluator) ? positionEvaluator.GetValue() : Vector3.zero, 
				ParameterType.Blueprint => (Evaluator is BlueprintEvaluator blueprintEvaluator) ? blueprintEvaluator.GetValue() : null, 
				ParameterType.Float => (Evaluator is FloatEvaluator floatEvaluator) ? floatEvaluator.GetValue() : 0f, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
	}

	public ParameterEntry[] Parameters;

	[NonSerialized]
	public Dictionary<string, object> AdditionalParams = new Dictionary<string, object>();
}
