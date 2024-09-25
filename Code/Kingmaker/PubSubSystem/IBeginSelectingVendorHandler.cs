using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IBeginSelectingVendorHandler : ISubscriber
{
	void HandleBeginSelectingVendor([CanBeNull] List<MechanicEntity> vendors);

	void HandleExitSelectingVendor();
}
