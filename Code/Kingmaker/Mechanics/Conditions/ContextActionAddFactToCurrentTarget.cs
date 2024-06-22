using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using UnityEngine;

namespace Kingmaker.Mechanics.Conditions;

[TypeId("209ada9815224d0dbc237ac635171e65")]
public class ContextActionAddFactToCurrentTarget : ContextAction
{
	[SerializeField]
	private BlueprintMechanicEntityFact.Reference m_FactReference;

	private BlueprintMechanicEntityFact Fact => m_FactReference.Get();

	public override string GetCaption()
	{
		return "Add fact to Current Target";
	}

	protected override void RunAction()
	{
		base.Target.Entity?.AddFact(Fact)?.TryAddSource(this);
	}
}
