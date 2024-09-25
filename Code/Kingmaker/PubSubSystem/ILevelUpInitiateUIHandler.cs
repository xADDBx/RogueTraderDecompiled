using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Levelup.Obsolete;

namespace Kingmaker.PubSubSystem;

public interface ILevelUpInitiateUIHandler : ISubscriber
{
	void HandleLevelUpStart(BaseUnitEntity unit, Action onCommit = null, Action onStop = null, LevelUpState.CharBuildMode mode = LevelUpState.CharBuildMode.LevelUp);
}
