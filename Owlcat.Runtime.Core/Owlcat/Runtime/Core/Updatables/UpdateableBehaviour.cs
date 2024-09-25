using Owlcat.Runtime.Core.Registry;

namespace Owlcat.Runtime.Core.Updatables;

public abstract class UpdateableBehaviour : RegisteredBehaviour, IUpdatable
{
	public abstract void DoUpdate();
}
