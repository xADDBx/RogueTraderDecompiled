using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Mechanics.Actions;

[TypeId("5f01002c7f9d46c383a4edc3e435aadc")]
public class ContextActionMoveEntityToPoint : ContextActionMove
{
	public override string GetCaption()
	{
		return $"Move entity to {m_TargetPoint}";
	}

	protected override void RunAction()
	{
		base.TargetEntity.Position = m_TargetPoint.GetValue();
	}
}
