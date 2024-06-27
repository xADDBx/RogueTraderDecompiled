using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class VariableDefinition : VariableReference
{
	public bool IsPinned => variable_type.IsPinned;

	public VariableDefinition(TypeReference variableType)
		: base(variableType)
	{
	}

	public override VariableDefinition Resolve()
	{
		return this;
	}
}
