using System.Collections.Generic;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IClockworkScenarioRecorder : ISubscriber
{
	void HandleMoveCommand(Vector3 worldPosition);

	void HandleInteractCommand(MapObjectView mapObject, bool force = false);

	void HandleInteractCommand(AbstractUnitEntityView unit);

	void HandleAnswerCommand(BlueprintAnswer answer);

	void HandleSetPartyCommand(List<UnitReference> unitRefs);
}
