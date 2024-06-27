using System;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Collections.Generic;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class ScopeDebugInformation : DebugInformation
{
	internal InstructionOffset start;

	internal InstructionOffset end;

	internal ImportDebugInformation import;

	internal Collection<ScopeDebugInformation> scopes;

	internal Collection<VariableDebugInformation> variables;

	internal Collection<ConstantDebugInformation> constants;

	public InstructionOffset Start
	{
		get
		{
			return start;
		}
		set
		{
			start = value;
		}
	}

	public InstructionOffset End
	{
		get
		{
			return end;
		}
		set
		{
			end = value;
		}
	}

	public ImportDebugInformation Import
	{
		get
		{
			return import;
		}
		set
		{
			import = value;
		}
	}

	public bool HasScopes => !scopes.IsNullOrEmpty();

	public Collection<ScopeDebugInformation> Scopes
	{
		get
		{
			if (scopes == null)
			{
				Interlocked.CompareExchange(ref scopes, new Collection<ScopeDebugInformation>(), null);
			}
			return scopes;
		}
	}

	public bool HasVariables => !variables.IsNullOrEmpty();

	public Collection<VariableDebugInformation> Variables
	{
		get
		{
			if (variables == null)
			{
				Interlocked.CompareExchange(ref variables, new Collection<VariableDebugInformation>(), null);
			}
			return variables;
		}
	}

	public bool HasConstants => !constants.IsNullOrEmpty();

	public Collection<ConstantDebugInformation> Constants
	{
		get
		{
			if (constants == null)
			{
				Interlocked.CompareExchange(ref constants, new Collection<ConstantDebugInformation>(), null);
			}
			return constants;
		}
	}

	internal ScopeDebugInformation()
	{
		token = new MetadataToken(TokenType.LocalScope);
	}

	public ScopeDebugInformation(Instruction start, Instruction end)
		: this()
	{
		if (start == null)
		{
			throw new ArgumentNullException("start");
		}
		this.start = new InstructionOffset(start);
		if (end != null)
		{
			this.end = new InstructionOffset(end);
		}
	}

	public bool TryGetName(VariableDefinition variable, out string name)
	{
		name = null;
		if (variables == null || variables.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < variables.Count; i++)
		{
			if (variables[i].Index == variable.Index)
			{
				name = variables[i].Name;
				return true;
			}
		}
		return false;
	}
}
