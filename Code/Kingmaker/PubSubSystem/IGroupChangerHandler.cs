using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGroupChangerHandler : ISubscriber
{
	void HandleCall(Action goAction, Action closeAction, bool isCapital, bool sameFinishActions = false, bool canCancel = true, bool showRemoteCompanions = false);

	void HandleSetLastUnits(List<UnitReference> lastUnits);

	void HandleSetRequiredUnits(List<BlueprintUnit> requiredUnits);
}
