using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Items;

public interface IItemsCollection : IEnumerable<ItemEntity>, IEnumerable
{
	[CanBeNull]
	IEntity Owner { get; }

	[CanBeNull]
	BaseUnitEntity OwnerUnit { get; }

	ReadonlyList<ItemEntity> Items { get; }

	bool ForceStackable { get; }

	bool IsVendorTable { get; }

	bool IsPlayerInventory { get; }

	bool IsSharedStash { get; }

	bool HasLoot { get; }

	bool Contains([NotNull] BlueprintItem item, int count = 1);

	bool ContainsAny([NotNull] IList<BlueprintItem> items);

	[CanBeNull]
	ItemEntity Add([NotNull] ItemEntity newItem, bool noAutoMerge = false);

	void Add([NotNull] BlueprintItem newBpItem, int count, [CanBeNull] Action<ItemEntity> callback = null, bool noAutoMerge = false);

	ItemEntity Add([NotNull] BlueprintItem newBpItem);

	ItemEntity Remove([NotNull] ItemEntity item, int? count = null);

	void Remove([NotNull] BlueprintItem bpItem, int count = 1);

	void RemoveAll();

	ItemEntity Transfer([NotNull] ItemEntity item, int count, [NotNull] ItemsCollection to);

	ItemEntity TransferWithoutMerge([NotNull] ItemEntity item, int count, [NotNull] ItemsCollection to);

	ItemEntity Transfer([NotNull] ItemEntity item, [NotNull] ItemsCollection to);
}
