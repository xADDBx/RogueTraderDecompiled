using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.UI.Models.Log.CombatLog_ThreadSystem.LogThreads.Common;

public class AwarenessLogThread : LogThreadBase, IGameLogEventHandler<GameLogEventAwareness>
{
	public void HandleEvent(GameLogEventAwareness evt)
	{
		BaseUnitEntity actor = evt.Actor;
		MapObjectEntity targetObject = evt.TargetObject;
		if (targetObject is TrapObjectData)
		{
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)actor;
			AddMessage(LogThreadBase.Strings.TrapSpotted.CreateCombatLogMessage());
		}
		else if (!(targetObject.View.GetComponent<InteractionDoor>() == null))
		{
			GameLogContext.SourceEntity = (GameLogContext.Property<IMechanicEntity>)(IMechanicEntity)actor;
			AddMessage(LogThreadBase.Strings.DoorSpotted.CreateCombatLogMessage());
		}
	}
}
