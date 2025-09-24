using System;
using System.Collections.Generic;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IVendorMultipleTransferHandler : ISubscriber
{
	void HandleTransitionWindow(List<ItemEntity> itemEntities = null, Action availabilityCheck = null);
}
