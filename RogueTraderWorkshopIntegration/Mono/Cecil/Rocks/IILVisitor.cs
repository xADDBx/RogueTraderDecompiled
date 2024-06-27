using System.Runtime.InteropServices;
using Mono.Cecil.Cil;

namespace Mono.Cecil.Rocks;

[ComVisible(false)]
public interface IILVisitor
{
	void OnInlineNone(OpCode opcode);

	void OnInlineSByte(OpCode opcode, sbyte value);

	void OnInlineByte(OpCode opcode, byte value);

	void OnInlineInt32(OpCode opcode, int value);

	void OnInlineInt64(OpCode opcode, long value);

	void OnInlineSingle(OpCode opcode, float value);

	void OnInlineDouble(OpCode opcode, double value);

	void OnInlineString(OpCode opcode, string value);

	void OnInlineBranch(OpCode opcode, int offset);

	void OnInlineSwitch(OpCode opcode, int[] offsets);

	void OnInlineVariable(OpCode opcode, VariableDefinition variable);

	void OnInlineArgument(OpCode opcode, ParameterDefinition parameter);

	void OnInlineSignature(OpCode opcode, CallSite callSite);

	void OnInlineType(OpCode opcode, TypeReference type);

	void OnInlineField(OpCode opcode, FieldReference field);

	void OnInlineMethod(OpCode opcode, MethodReference method);
}
