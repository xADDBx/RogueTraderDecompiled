using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public struct VariableIndex
{
	private readonly VariableDefinition variable;

	private readonly int? index;

	public int Index
	{
		get
		{
			if (variable != null)
			{
				return variable.Index;
			}
			if (index.HasValue)
			{
				return index.Value;
			}
			throw new NotSupportedException();
		}
	}

	internal bool IsResolved => variable != null;

	internal VariableDefinition ResolvedVariable => variable;

	public VariableIndex(VariableDefinition variable)
	{
		if (variable == null)
		{
			throw new ArgumentNullException("variable");
		}
		this.variable = variable;
		index = null;
	}

	public VariableIndex(int index)
	{
		variable = null;
		this.index = index;
	}
}
