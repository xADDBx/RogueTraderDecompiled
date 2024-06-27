using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Cil;

[ComVisible(false)]
public struct OpCode : IEquatable<OpCode>
{
	private readonly byte op1;

	private readonly byte op2;

	private readonly byte code;

	private readonly byte flow_control;

	private readonly byte opcode_type;

	private readonly byte operand_type;

	private readonly byte stack_behavior_pop;

	private readonly byte stack_behavior_push;

	public string Name => OpCodeNames.names[(int)Code];

	public int Size
	{
		get
		{
			if (op1 != byte.MaxValue)
			{
				return 2;
			}
			return 1;
		}
	}

	public byte Op1 => op1;

	public byte Op2 => op2;

	public short Value
	{
		get
		{
			if (op1 != byte.MaxValue)
			{
				return (short)((op1 << 8) | op2);
			}
			return op2;
		}
	}

	public Code Code => (Code)code;

	public FlowControl FlowControl => (FlowControl)flow_control;

	public OpCodeType OpCodeType => (OpCodeType)opcode_type;

	public OperandType OperandType => (OperandType)operand_type;

	public StackBehaviour StackBehaviourPop => (StackBehaviour)stack_behavior_pop;

	public StackBehaviour StackBehaviourPush => (StackBehaviour)stack_behavior_push;

	internal OpCode(int x, int y)
	{
		op1 = (byte)((uint)x & 0xFFu);
		op2 = (byte)((uint)(x >> 8) & 0xFFu);
		code = (byte)((uint)(x >> 16) & 0xFFu);
		flow_control = (byte)((uint)(x >> 24) & 0xFFu);
		opcode_type = (byte)((uint)y & 0xFFu);
		operand_type = (byte)((uint)(y >> 8) & 0xFFu);
		stack_behavior_pop = (byte)((uint)(y >> 16) & 0xFFu);
		stack_behavior_push = (byte)((uint)(y >> 24) & 0xFFu);
		if (op1 == byte.MaxValue)
		{
			OpCodes.OneByteOpCode[op2] = this;
		}
		else
		{
			OpCodes.TwoBytesOpCode[op2] = this;
		}
	}

	public override int GetHashCode()
	{
		return Value;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is OpCode opCode))
		{
			return false;
		}
		if (op1 == opCode.op1)
		{
			return op2 == opCode.op2;
		}
		return false;
	}

	public bool Equals(OpCode opcode)
	{
		if (op1 == opcode.op1)
		{
			return op2 == opcode.op2;
		}
		return false;
	}

	public static bool operator ==(OpCode one, OpCode other)
	{
		if (one.op1 == other.op1)
		{
			return one.op2 == other.op2;
		}
		return false;
	}

	public static bool operator !=(OpCode one, OpCode other)
	{
		if (one.op1 == other.op1)
		{
			return one.op2 != other.op2;
		}
		return true;
	}

	public override string ToString()
	{
		return Name;
	}
}
