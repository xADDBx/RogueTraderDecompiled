using System;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;

namespace Mono.Cecil.Rocks;

[ComVisible(false)]
public static class MethodBodyRocks
{
	public static void SimplifyMacros(this MethodBody self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		foreach (Instruction instruction in self.Instructions)
		{
			if (instruction.OpCode.OpCodeType == OpCodeType.Macro)
			{
				switch (instruction.OpCode.Code)
				{
				case Mono.Cecil.Cil.Code.Ldarg_0:
					ExpandMacro(instruction, OpCodes.Ldarg, self.GetParameter(0));
					break;
				case Mono.Cecil.Cil.Code.Ldarg_1:
					ExpandMacro(instruction, OpCodes.Ldarg, self.GetParameter(1));
					break;
				case Mono.Cecil.Cil.Code.Ldarg_2:
					ExpandMacro(instruction, OpCodes.Ldarg, self.GetParameter(2));
					break;
				case Mono.Cecil.Cil.Code.Ldarg_3:
					ExpandMacro(instruction, OpCodes.Ldarg, self.GetParameter(3));
					break;
				case Mono.Cecil.Cil.Code.Ldloc_0:
					ExpandMacro(instruction, OpCodes.Ldloc, self.Variables[0]);
					break;
				case Mono.Cecil.Cil.Code.Ldloc_1:
					ExpandMacro(instruction, OpCodes.Ldloc, self.Variables[1]);
					break;
				case Mono.Cecil.Cil.Code.Ldloc_2:
					ExpandMacro(instruction, OpCodes.Ldloc, self.Variables[2]);
					break;
				case Mono.Cecil.Cil.Code.Ldloc_3:
					ExpandMacro(instruction, OpCodes.Ldloc, self.Variables[3]);
					break;
				case Mono.Cecil.Cil.Code.Stloc_0:
					ExpandMacro(instruction, OpCodes.Stloc, self.Variables[0]);
					break;
				case Mono.Cecil.Cil.Code.Stloc_1:
					ExpandMacro(instruction, OpCodes.Stloc, self.Variables[1]);
					break;
				case Mono.Cecil.Cil.Code.Stloc_2:
					ExpandMacro(instruction, OpCodes.Stloc, self.Variables[2]);
					break;
				case Mono.Cecil.Cil.Code.Stloc_3:
					ExpandMacro(instruction, OpCodes.Stloc, self.Variables[3]);
					break;
				case Mono.Cecil.Cil.Code.Ldarg_S:
					instruction.OpCode = OpCodes.Ldarg;
					break;
				case Mono.Cecil.Cil.Code.Ldarga_S:
					instruction.OpCode = OpCodes.Ldarga;
					break;
				case Mono.Cecil.Cil.Code.Starg_S:
					instruction.OpCode = OpCodes.Starg;
					break;
				case Mono.Cecil.Cil.Code.Ldloc_S:
					instruction.OpCode = OpCodes.Ldloc;
					break;
				case Mono.Cecil.Cil.Code.Ldloca_S:
					instruction.OpCode = OpCodes.Ldloca;
					break;
				case Mono.Cecil.Cil.Code.Stloc_S:
					instruction.OpCode = OpCodes.Stloc;
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_M1:
					ExpandMacro(instruction, OpCodes.Ldc_I4, -1);
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_0:
					ExpandMacro(instruction, OpCodes.Ldc_I4, 0);
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_1:
					ExpandMacro(instruction, OpCodes.Ldc_I4, 1);
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_2:
					ExpandMacro(instruction, OpCodes.Ldc_I4, 2);
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_3:
					ExpandMacro(instruction, OpCodes.Ldc_I4, 3);
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_4:
					ExpandMacro(instruction, OpCodes.Ldc_I4, 4);
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_5:
					ExpandMacro(instruction, OpCodes.Ldc_I4, 5);
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_6:
					ExpandMacro(instruction, OpCodes.Ldc_I4, 6);
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_7:
					ExpandMacro(instruction, OpCodes.Ldc_I4, 7);
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_8:
					ExpandMacro(instruction, OpCodes.Ldc_I4, 8);
					break;
				case Mono.Cecil.Cil.Code.Ldc_I4_S:
					ExpandMacro(instruction, OpCodes.Ldc_I4, (int)(sbyte)instruction.Operand);
					break;
				case Mono.Cecil.Cil.Code.Br_S:
					instruction.OpCode = OpCodes.Br;
					break;
				case Mono.Cecil.Cil.Code.Brfalse_S:
					instruction.OpCode = OpCodes.Brfalse;
					break;
				case Mono.Cecil.Cil.Code.Brtrue_S:
					instruction.OpCode = OpCodes.Brtrue;
					break;
				case Mono.Cecil.Cil.Code.Beq_S:
					instruction.OpCode = OpCodes.Beq;
					break;
				case Mono.Cecil.Cil.Code.Bge_S:
					instruction.OpCode = OpCodes.Bge;
					break;
				case Mono.Cecil.Cil.Code.Bgt_S:
					instruction.OpCode = OpCodes.Bgt;
					break;
				case Mono.Cecil.Cil.Code.Ble_S:
					instruction.OpCode = OpCodes.Ble;
					break;
				case Mono.Cecil.Cil.Code.Blt_S:
					instruction.OpCode = OpCodes.Blt;
					break;
				case Mono.Cecil.Cil.Code.Bne_Un_S:
					instruction.OpCode = OpCodes.Bne_Un;
					break;
				case Mono.Cecil.Cil.Code.Bge_Un_S:
					instruction.OpCode = OpCodes.Bge_Un;
					break;
				case Mono.Cecil.Cil.Code.Bgt_Un_S:
					instruction.OpCode = OpCodes.Bgt_Un;
					break;
				case Mono.Cecil.Cil.Code.Ble_Un_S:
					instruction.OpCode = OpCodes.Ble_Un;
					break;
				case Mono.Cecil.Cil.Code.Blt_Un_S:
					instruction.OpCode = OpCodes.Blt_Un;
					break;
				case Mono.Cecil.Cil.Code.Leave_S:
					instruction.OpCode = OpCodes.Leave;
					break;
				}
			}
		}
	}

	private static void ExpandMacro(Instruction instruction, OpCode opcode, object operand)
	{
		instruction.OpCode = opcode;
		instruction.Operand = operand;
	}

	private static void MakeMacro(Instruction instruction, OpCode opcode)
	{
		instruction.OpCode = opcode;
		instruction.Operand = null;
	}

	public static void Optimize(this MethodBody self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		self.OptimizeLongs();
		self.OptimizeMacros();
	}

	private static void OptimizeLongs(this MethodBody self)
	{
		for (int i = 0; i < self.Instructions.Count; i++)
		{
			Instruction instruction = self.Instructions[i];
			if (instruction.OpCode.Code == Mono.Cecil.Cil.Code.Ldc_I8)
			{
				long num = (long)instruction.Operand;
				if (num < int.MaxValue && num > int.MinValue)
				{
					ExpandMacro(instruction, OpCodes.Ldc_I4, (int)num);
					self.Instructions.Insert(++i, Instruction.Create(OpCodes.Conv_I8));
				}
			}
		}
	}

	public static void OptimizeMacros(this MethodBody self)
	{
		if (self == null)
		{
			throw new ArgumentNullException("self");
		}
		MethodDefinition method = self.Method;
		foreach (Instruction instruction in self.Instructions)
		{
			switch (instruction.OpCode.Code)
			{
			case Mono.Cecil.Cil.Code.Ldarg:
			{
				int index = ((ParameterDefinition)instruction.Operand).Index;
				if (index == -1 && instruction.Operand == self.ThisParameter)
				{
					index = 0;
				}
				else if (method.HasThis)
				{
					index++;
				}
				switch (index)
				{
				case 0:
					MakeMacro(instruction, OpCodes.Ldarg_0);
					break;
				case 1:
					MakeMacro(instruction, OpCodes.Ldarg_1);
					break;
				case 2:
					MakeMacro(instruction, OpCodes.Ldarg_2);
					break;
				case 3:
					MakeMacro(instruction, OpCodes.Ldarg_3);
					break;
				default:
					if (index < 256)
					{
						ExpandMacro(instruction, OpCodes.Ldarg_S, instruction.Operand);
					}
					break;
				}
				break;
			}
			case Mono.Cecil.Cil.Code.Ldloc:
			{
				int index = ((VariableDefinition)instruction.Operand).Index;
				switch (index)
				{
				case 0:
					MakeMacro(instruction, OpCodes.Ldloc_0);
					break;
				case 1:
					MakeMacro(instruction, OpCodes.Ldloc_1);
					break;
				case 2:
					MakeMacro(instruction, OpCodes.Ldloc_2);
					break;
				case 3:
					MakeMacro(instruction, OpCodes.Ldloc_3);
					break;
				default:
					if (index < 256)
					{
						ExpandMacro(instruction, OpCodes.Ldloc_S, instruction.Operand);
					}
					break;
				}
				break;
			}
			case Mono.Cecil.Cil.Code.Stloc:
			{
				int index = ((VariableDefinition)instruction.Operand).Index;
				switch (index)
				{
				case 0:
					MakeMacro(instruction, OpCodes.Stloc_0);
					break;
				case 1:
					MakeMacro(instruction, OpCodes.Stloc_1);
					break;
				case 2:
					MakeMacro(instruction, OpCodes.Stloc_2);
					break;
				case 3:
					MakeMacro(instruction, OpCodes.Stloc_3);
					break;
				default:
					if (index < 256)
					{
						ExpandMacro(instruction, OpCodes.Stloc_S, instruction.Operand);
					}
					break;
				}
				break;
			}
			case Mono.Cecil.Cil.Code.Ldarga:
			{
				int index = ((ParameterDefinition)instruction.Operand).Index;
				if (index == -1 && instruction.Operand == self.ThisParameter)
				{
					index = 0;
				}
				else if (method.HasThis)
				{
					index++;
				}
				if (index < 256)
				{
					ExpandMacro(instruction, OpCodes.Ldarga_S, instruction.Operand);
				}
				break;
			}
			case Mono.Cecil.Cil.Code.Ldloca:
				if (((VariableDefinition)instruction.Operand).Index < 256)
				{
					ExpandMacro(instruction, OpCodes.Ldloca_S, instruction.Operand);
				}
				break;
			case Mono.Cecil.Cil.Code.Ldc_I4:
			{
				int num = (int)instruction.Operand;
				switch (num)
				{
				case -1:
					MakeMacro(instruction, OpCodes.Ldc_I4_M1);
					break;
				case 0:
					MakeMacro(instruction, OpCodes.Ldc_I4_0);
					break;
				case 1:
					MakeMacro(instruction, OpCodes.Ldc_I4_1);
					break;
				case 2:
					MakeMacro(instruction, OpCodes.Ldc_I4_2);
					break;
				case 3:
					MakeMacro(instruction, OpCodes.Ldc_I4_3);
					break;
				case 4:
					MakeMacro(instruction, OpCodes.Ldc_I4_4);
					break;
				case 5:
					MakeMacro(instruction, OpCodes.Ldc_I4_5);
					break;
				case 6:
					MakeMacro(instruction, OpCodes.Ldc_I4_6);
					break;
				case 7:
					MakeMacro(instruction, OpCodes.Ldc_I4_7);
					break;
				case 8:
					MakeMacro(instruction, OpCodes.Ldc_I4_8);
					break;
				default:
					if (num >= -128 && num < 128)
					{
						ExpandMacro(instruction, OpCodes.Ldc_I4_S, (sbyte)num);
					}
					break;
				}
				break;
			}
			}
		}
		OptimizeBranches(self);
	}

	private static void OptimizeBranches(MethodBody body)
	{
		ComputeOffsets(body);
		foreach (Instruction instruction in body.Instructions)
		{
			if (instruction.OpCode.OperandType == OperandType.InlineBrTarget && OptimizeBranch(instruction))
			{
				ComputeOffsets(body);
			}
		}
	}

	private static bool OptimizeBranch(Instruction instruction)
	{
		int num = ((Instruction)instruction.Operand).Offset - (instruction.Offset + instruction.OpCode.Size + 4);
		if (num < -128 || num > 127)
		{
			return false;
		}
		switch (instruction.OpCode.Code)
		{
		case Mono.Cecil.Cil.Code.Br:
			instruction.OpCode = OpCodes.Br_S;
			break;
		case Mono.Cecil.Cil.Code.Brfalse:
			instruction.OpCode = OpCodes.Brfalse_S;
			break;
		case Mono.Cecil.Cil.Code.Brtrue:
			instruction.OpCode = OpCodes.Brtrue_S;
			break;
		case Mono.Cecil.Cil.Code.Beq:
			instruction.OpCode = OpCodes.Beq_S;
			break;
		case Mono.Cecil.Cil.Code.Bge:
			instruction.OpCode = OpCodes.Bge_S;
			break;
		case Mono.Cecil.Cil.Code.Bgt:
			instruction.OpCode = OpCodes.Bgt_S;
			break;
		case Mono.Cecil.Cil.Code.Ble:
			instruction.OpCode = OpCodes.Ble_S;
			break;
		case Mono.Cecil.Cil.Code.Blt:
			instruction.OpCode = OpCodes.Blt_S;
			break;
		case Mono.Cecil.Cil.Code.Bne_Un:
			instruction.OpCode = OpCodes.Bne_Un_S;
			break;
		case Mono.Cecil.Cil.Code.Bge_Un:
			instruction.OpCode = OpCodes.Bge_Un_S;
			break;
		case Mono.Cecil.Cil.Code.Bgt_Un:
			instruction.OpCode = OpCodes.Bgt_Un_S;
			break;
		case Mono.Cecil.Cil.Code.Ble_Un:
			instruction.OpCode = OpCodes.Ble_Un_S;
			break;
		case Mono.Cecil.Cil.Code.Blt_Un:
			instruction.OpCode = OpCodes.Blt_Un_S;
			break;
		case Mono.Cecil.Cil.Code.Leave:
			instruction.OpCode = OpCodes.Leave_S;
			break;
		}
		return true;
	}

	private static void ComputeOffsets(MethodBody body)
	{
		int num = 0;
		foreach (Instruction instruction in body.Instructions)
		{
			instruction.Offset = num;
			num += instruction.GetSize();
		}
	}
}
