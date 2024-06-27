using Mono.Collections.Generic;

namespace Mono.Cecil.Cil;

internal sealed class VariableDefinitionCollection : Collection<VariableDefinition>
{
	private readonly MethodDefinition method;

	internal VariableDefinitionCollection(MethodDefinition method)
	{
		this.method = method;
	}

	internal VariableDefinitionCollection(MethodDefinition method, int capacity)
		: base(capacity)
	{
		this.method = method;
	}

	protected override void OnAdd(VariableDefinition item, int index)
	{
		item.index = index;
	}

	protected override void OnInsert(VariableDefinition item, int index)
	{
		item.index = index;
		UpdateVariableIndices(index, 1);
	}

	protected override void OnSet(VariableDefinition item, int index)
	{
		item.index = index;
	}

	protected override void OnRemove(VariableDefinition item, int index)
	{
		UpdateVariableIndices(index + 1, -1, item);
		item.index = -1;
	}

	private void UpdateVariableIndices(int startIndex, int offset, VariableDefinition variableToRemove = null)
	{
		for (int i = startIndex; i < size; i++)
		{
			items[i].index = i + offset;
		}
		MethodDebugInformation methodDebugInformation = ((method == null) ? null : method.debug_info);
		if (methodDebugInformation == null || methodDebugInformation.Scope == null)
		{
			return;
		}
		foreach (ScopeDebugInformation scope in methodDebugInformation.GetScopes())
		{
			if (!scope.HasVariables)
			{
				continue;
			}
			Collection<VariableDebugInformation> variables = scope.Variables;
			int num = -1;
			for (int j = 0; j < variables.Count; j++)
			{
				VariableDebugInformation variableDebugInformation = variables[j];
				if (variableToRemove != null && ((variableDebugInformation.index.IsResolved && variableDebugInformation.index.ResolvedVariable == variableToRemove) || (!variableDebugInformation.index.IsResolved && variableDebugInformation.Index == variableToRemove.Index)))
				{
					num = j;
				}
				else if (!variableDebugInformation.index.IsResolved && variableDebugInformation.Index >= startIndex)
				{
					variableDebugInformation.index = new VariableIndex(variableDebugInformation.Index + offset);
				}
			}
			if (num >= 0)
			{
				variables.RemoveAt(num);
			}
		}
	}
}
