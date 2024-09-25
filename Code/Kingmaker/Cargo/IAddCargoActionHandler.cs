using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Cargo;

public interface IAddCargoActionHandler : ISubscriber
{
	void HandleAddCargoAction(List<CargoEntity> cargoEntities);
}
