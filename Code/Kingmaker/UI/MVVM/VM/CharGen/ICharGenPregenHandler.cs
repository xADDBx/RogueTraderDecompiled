using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.VM.CharGen;

public interface ICharGenPregenHandler : ISubscriber
{
	void HandleSetPregen([CanBeNull] BaseUnitEntity unit);
}
