using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics.Actions;

namespace Kingmaker.Mechanics.Actions;

[TypeId("9354b44a69e941408baa9ea82719e692")]
public class ContextActionRestrictStrategistAbility : ContextAction
{
	public bool m_Not;

	public override string GetCaption()
	{
		return (m_Not ? "Allow another" : "Restrict") + " Strategist ability this turn";
	}

	protected override void RunAction()
	{
		Game.Instance.Player.StrategistManager.IsCastRestricted = !m_Not;
		Game.Instance.Player.StrategistManager.AllowFirstRoundRule = m_Not;
	}
}
