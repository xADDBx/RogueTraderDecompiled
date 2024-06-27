using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public sealed class StateMachineScope
{
	internal InstructionOffset start;

	internal InstructionOffset end;

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

	internal StateMachineScope(int start, int end)
	{
		this.start = new InstructionOffset(start);
		this.end = new InstructionOffset(end);
	}

	public StateMachineScope(Instruction start, Instruction end)
	{
		this.start = new InstructionOffset(start);
		this.end = ((end != null) ? new InstructionOffset(end) : default(InstructionOffset));
	}
}
