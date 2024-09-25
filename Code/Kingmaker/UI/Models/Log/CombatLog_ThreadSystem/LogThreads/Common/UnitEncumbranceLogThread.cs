using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UnitLogic;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class UnitEncumbranceLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventUnitEncumbranceChanged>
{
	public void HandleEvent(GameLogEventUnitEncumbranceChanged evt)
	{
		BaseUnitEntity unit = evt.Unit;
		if (unit != null && !unit.LifeState.IsDead)
		{
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)unit;
			switch (evt.CurrentEncumbrance)
			{
			case Encumbrance.Light:
				AddMessage(LogThreadBase.Strings.UnitEncumbranceLight.CreateCombatLogMessage());
				break;
			case Encumbrance.Medium:
				AddMessage(LogThreadBase.Strings.UnitEncumbranceMedium.CreateCombatLogMessage());
				break;
			case Encumbrance.Heavy:
				AddMessage(LogThreadBase.Strings.UnitEncumbranceHeavy.CreateCombatLogMessage());
				break;
			case Encumbrance.Overload:
				AddMessage(LogThreadBase.Strings.UnitEncumbranceOverload.CreateCombatLogMessage());
				break;
			}
		}
	}
}
