namespace Core.Cheats;

public class CheatPropertyInfoInternal : CheatPropertyInfo
{
	public CheatPropertyMethods Methods { get; }

	public CheatPropertyInfoInternal(CheatPropertyMethods methods, string name, string description, string exampleUsage, ExecutionPolicy executionPolicy, string type)
		: base(name, description, exampleUsage, executionPolicy, type)
	{
		Methods = methods;
	}
}
