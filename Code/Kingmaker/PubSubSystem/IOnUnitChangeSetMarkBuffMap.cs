using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IOnUnitChangeSetMarkBuffMap : ISubscriber
{
	void HandleUnitChange(Dictionary<MechanicEntity, BuffVM> map);
}
