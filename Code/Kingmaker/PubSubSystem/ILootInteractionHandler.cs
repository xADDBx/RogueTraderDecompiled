using System;
using Kingmaker.Controllers.Dialog;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;
using Kingmaker.View.MapObjects;

namespace Kingmaker.PubSubSystem;

public interface ILootInteractionHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback);

	void HandleSpaceLootInteraction(ILootable[] objects, LootContainerType containerType, Action closeCallback, SkillCheckResult skillCheckResult = null);

	void HandleZoneLootInteraction(AreaTransitionPart areaTransition);
}
