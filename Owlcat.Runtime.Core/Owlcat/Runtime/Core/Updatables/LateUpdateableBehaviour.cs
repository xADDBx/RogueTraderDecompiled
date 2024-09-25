using Owlcat.Runtime.Core.Registry;

namespace Owlcat.Runtime.Core.Updatables;

public abstract class LateUpdateableBehaviour : RegisteredBehaviour, ILateUpdatable
{
	public abstract void DoLateUpdate();
}
