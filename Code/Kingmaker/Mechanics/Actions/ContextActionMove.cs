using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[TypeId("d8eb8545946e4e4b965b0c2ac2fab7ad")]
public abstract class ContextActionMove : ContextAction
{
	[SerializeField]
	[SerializeReference]
	protected PositionEvaluator m_TargetPoint;
}
