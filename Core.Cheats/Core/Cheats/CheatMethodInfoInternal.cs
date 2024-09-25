using System;

namespace Core.Cheats;

public class CheatMethodInfoInternal : CheatMethodInfo
{
	public Delegate Method { get; }

	public string MethodSignature { get; }

	public CheatMethodInfoInternal(Delegate method, string signature, string name, string description, string exampleUsage, ExecutionPolicy executionPolicy, CheatParameter[] parameters, string returnType)
		: base(name, description, exampleUsage, executionPolicy, parameters, returnType)
	{
		Method = method;
		MethodSignature = signature;
	}
}
