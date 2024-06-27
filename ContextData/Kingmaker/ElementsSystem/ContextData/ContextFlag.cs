namespace Kingmaker.ElementsSystem.ContextData;

public abstract class ContextFlag<TFlag> : ContextData<TFlag> where TFlag : ContextFlag<TFlag>, new()
{
	protected sealed override void Reset()
	{
	}
}
