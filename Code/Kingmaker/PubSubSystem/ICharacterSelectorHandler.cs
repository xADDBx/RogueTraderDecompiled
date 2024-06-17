using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICharacterSelectorHandler : ISubscriber
{
	void HandleSelectCharacter(List<BaseUnitEntity> characters, Action<BaseUnitEntity> successAction);
}
