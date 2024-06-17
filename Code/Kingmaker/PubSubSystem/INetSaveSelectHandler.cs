using JetBrains.Annotations;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetSaveSelectHandler : ISubscriber
{
	void HandleSaveSelect([CanBeNull] SaveInfo saveInfo);
}
