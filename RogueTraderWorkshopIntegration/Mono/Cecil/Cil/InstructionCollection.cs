using Mono.Collections.Generic;

namespace Mono.Cecil.Cil;

internal class InstructionCollection : Collection<Instruction>
{
	private struct InstructionOffsetResolver
	{
		private readonly Instruction[] items;

		private readonly Instruction removed_instruction;

		private readonly Instruction existing_instruction;

		private int cache_offset;

		private int cache_index;

		private Instruction cache_instruction;

		public int LastOffset => cache_offset;

		public InstructionOffsetResolver(Instruction[] instructions, Instruction removedInstruction, Instruction existingInstruction)
		{
			items = instructions;
			removed_instruction = removedInstruction;
			existing_instruction = existingInstruction;
			cache_offset = 0;
			cache_index = 0;
			cache_instruction = items[0];
		}

		public void Restart()
		{
			cache_offset = 0;
			cache_index = 0;
			cache_instruction = items[0];
		}

		public InstructionOffset Resolve(InstructionOffset inputOffset)
		{
			InstructionOffset result = ResolveInstructionOffset(inputOffset);
			if (!result.IsEndOfMethod && result.ResolvedInstruction == removed_instruction)
			{
				result = new InstructionOffset(existing_instruction);
			}
			return result;
		}

		private InstructionOffset ResolveInstructionOffset(InstructionOffset inputOffset)
		{
			if (inputOffset.IsResolved)
			{
				return inputOffset;
			}
			int offset = inputOffset.Offset;
			if (cache_offset == offset)
			{
				return new InstructionOffset(cache_instruction);
			}
			if (cache_offset > offset)
			{
				int num = 0;
				for (int i = 0; i < items.Length; i++)
				{
					if (items[i] == null)
					{
						return new InstructionOffset((i == 0) ? items[0] : items[i - 1]);
					}
					if (num == offset)
					{
						return new InstructionOffset(items[i]);
					}
					if (num > offset)
					{
						return new InstructionOffset((i == 0) ? items[0] : items[i - 1]);
					}
					num += items[i].GetSize();
				}
				return default(InstructionOffset);
			}
			int num2 = cache_offset;
			for (int j = cache_index; j < items.Length; j++)
			{
				cache_index = j;
				cache_offset = num2;
				Instruction instruction = items[j];
				if (instruction == null)
				{
					return new InstructionOffset((j == 0) ? items[0] : items[j - 1]);
				}
				cache_instruction = instruction;
				if (cache_offset == offset)
				{
					return new InstructionOffset(cache_instruction);
				}
				if (cache_offset > offset)
				{
					return new InstructionOffset((j == 0) ? items[0] : items[j - 1]);
				}
				num2 += instruction.GetSize();
			}
			return default(InstructionOffset);
		}
	}

	private readonly MethodDefinition method;

	internal InstructionCollection(MethodDefinition method)
	{
		this.method = method;
	}

	internal InstructionCollection(MethodDefinition method, int capacity)
		: base(capacity)
	{
		this.method = method;
	}

	protected override void OnAdd(Instruction item, int index)
	{
		if (index != 0)
		{
			Instruction instruction = items[index - 1];
			instruction.next = item;
			item.previous = instruction;
		}
	}

	protected override void OnInsert(Instruction item, int index)
	{
		if (size != 0)
		{
			Instruction instruction = items[index];
			if (instruction == null)
			{
				Instruction instruction2 = items[index - 1];
				instruction2.next = item;
				item.previous = instruction2;
				return;
			}
			_ = instruction.Offset;
			Instruction previous = instruction.previous;
			if (previous != null)
			{
				previous.next = item;
				item.previous = previous;
			}
			instruction.previous = item;
			item.next = instruction;
		}
		UpdateDebugInformation(null, null);
	}

	protected override void OnSet(Instruction item, int index)
	{
		Instruction instruction = items[index];
		item.previous = instruction.previous;
		item.next = instruction.next;
		instruction.previous = null;
		instruction.next = null;
		UpdateDebugInformation(item, instruction);
	}

	protected override void OnRemove(Instruction item, int index)
	{
		Instruction previous = item.previous;
		if (previous != null)
		{
			previous.next = item.next;
		}
		Instruction next = item.next;
		if (next != null)
		{
			next.previous = item.previous;
		}
		RemoveSequencePoint(item);
		UpdateDebugInformation(item, next ?? previous);
		item.previous = null;
		item.next = null;
	}

	private void RemoveSequencePoint(Instruction instruction)
	{
		MethodDebugInformation debug_info = method.debug_info;
		if (debug_info == null || !debug_info.HasSequencePoints)
		{
			return;
		}
		Collection<SequencePoint> sequence_points = debug_info.sequence_points;
		for (int i = 0; i < sequence_points.Count; i++)
		{
			if (sequence_points[i].Offset == instruction.offset)
			{
				sequence_points.RemoveAt(i);
				break;
			}
		}
	}

	private void UpdateDebugInformation(Instruction removedInstruction, Instruction existingInstruction)
	{
		InstructionOffsetResolver resolver = new InstructionOffsetResolver(items, removedInstruction, existingInstruction);
		if (method.debug_info != null)
		{
			UpdateLocalScope(method.debug_info.Scope, ref resolver);
		}
		Collection<CustomDebugInformation> collection = method.custom_infos ?? method.debug_info?.custom_infos;
		if (collection == null)
		{
			return;
		}
		foreach (CustomDebugInformation item in collection)
		{
			if (!(item is StateMachineScopeDebugInformation debugInfo))
			{
				if (item is AsyncMethodBodyDebugInformation debugInfo2)
				{
					UpdateAsyncMethodBody(debugInfo2, ref resolver);
				}
			}
			else
			{
				UpdateStateMachineScope(debugInfo, ref resolver);
			}
		}
	}

	private void UpdateLocalScope(ScopeDebugInformation scope, ref InstructionOffsetResolver resolver)
	{
		if (scope == null)
		{
			return;
		}
		scope.Start = resolver.Resolve(scope.Start);
		if (scope.HasScopes)
		{
			foreach (ScopeDebugInformation scope2 in scope.Scopes)
			{
				UpdateLocalScope(scope2, ref resolver);
			}
		}
		scope.End = resolver.Resolve(scope.End);
	}

	private void UpdateStateMachineScope(StateMachineScopeDebugInformation debugInfo, ref InstructionOffsetResolver resolver)
	{
		resolver.Restart();
		foreach (StateMachineScope scope in debugInfo.Scopes)
		{
			scope.Start = resolver.Resolve(scope.Start);
			scope.End = resolver.Resolve(scope.End);
		}
	}

	private void UpdateAsyncMethodBody(AsyncMethodBodyDebugInformation debugInfo, ref InstructionOffsetResolver resolver)
	{
		if (!debugInfo.CatchHandler.IsResolved)
		{
			resolver.Restart();
			debugInfo.CatchHandler = resolver.Resolve(debugInfo.CatchHandler);
		}
		resolver.Restart();
		for (int i = 0; i < debugInfo.Yields.Count; i++)
		{
			debugInfo.Yields[i] = resolver.Resolve(debugInfo.Yields[i]);
		}
		resolver.Restart();
		for (int j = 0; j < debugInfo.Resumes.Count; j++)
		{
			debugInfo.Resumes[j] = resolver.Resolve(debugInfo.Resumes[j]);
		}
	}
}
