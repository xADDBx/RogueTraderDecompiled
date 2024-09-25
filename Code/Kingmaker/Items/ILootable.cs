using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Items;

public interface ILootable
{
	[CanBeNull]
	string Name { get; }

	[CanBeNull]
	string Description { get; }

	[CanBeNull]
	BaseUnitEntity OwnerEntity { get; }

	[CanBeNull]
	ItemsCollection Items { get; }

	[CanBeNull]
	List<BlueprintCargoReference> Cargo { get; }

	[CanBeNull]
	Func<ItemEntity, bool> CanInsertItem { get; }
}
