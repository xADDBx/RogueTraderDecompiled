using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.Interaction;

public interface IInteractionVariantActor
{
	int? InteractionDC { get; }

	InteractionActorType Type { get; }

	InteractionPart InteractionPart { get; }

	bool ShowInteractFx { get; }

	int? RequiredItemsCount { get; }

	BlueprintItem RequiredItem { get; }

	StatType Skill { get; }

	bool CheckOnlyOnce { get; }

	bool CanUse { get; }

	[CanBeNull]
	string GetInteractionName();

	bool CanInteract(BaseUnitEntity user);

	void ShowSuccessBark(BaseUnitEntity user);

	void ShowRestrictionBark(BaseUnitEntity user);

	void OnDidInteract(BaseUnitEntity user);

	void OnFailedInteract(BaseUnitEntity user);

	void OnInteract(BaseUnitEntity user);
}
