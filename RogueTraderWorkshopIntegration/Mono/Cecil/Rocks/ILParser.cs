using System;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Mono.Cecil.Rocks;

[ComVisible(false)]
public static class ILParser
{
	private class ParseContext
	{
		public CodeReader Code { get; set; }

		public int Position { get; set; }

		public MetadataReader Metadata { get; set; }

		public Collection<VariableDefinition> Variables { get; set; }

		public IILVisitor Visitor { get; set; }
	}

	public static void Parse(MethodDefinition method, IILVisitor visitor)
	{
		if (method == null)
		{
			throw new ArgumentNullException("method");
		}
		if (visitor == null)
		{
			throw new ArgumentNullException("visitor");
		}
		if (!method.HasBody || !method.HasImage)
		{
			throw new ArgumentException();
		}
		method.Module.Read(method, delegate(MethodDefinition m, MetadataReader _)
		{
			ParseMethod(m, visitor);
			return true;
		});
	}

	private static void ParseMethod(MethodDefinition method, IILVisitor visitor)
	{
		ParseContext parseContext = CreateContext(method, visitor);
		CodeReader code = parseContext.Code;
		byte b = code.ReadByte();
		switch (b & 3)
		{
		case 2:
			ParseCode(b >> 2, parseContext);
			break;
		case 3:
			code.Advance(-1);
			ParseFatMethod(parseContext);
			break;
		default:
			throw new NotSupportedException();
		}
		code.MoveBackTo(parseContext.Position);
	}

	private static ParseContext CreateContext(MethodDefinition method, IILVisitor visitor)
	{
		CodeReader codeReader = method.Module.Read(method, (MethodDefinition _, MetadataReader reader) => reader.code);
		int position = codeReader.MoveTo(method);
		return new ParseContext
		{
			Code = codeReader,
			Position = position,
			Metadata = codeReader.reader,
			Visitor = visitor
		};
	}

	private static void ParseFatMethod(ParseContext context)
	{
		CodeReader code = context.Code;
		code.Advance(4);
		int code_size = code.ReadInt32();
		MetadataToken metadataToken = code.ReadToken();
		if (metadataToken != MetadataToken.Zero)
		{
			context.Variables = code.ReadVariables(metadataToken);
		}
		ParseCode(code_size, context);
	}

	private static void ParseCode(int code_size, ParseContext context)
	{
		CodeReader code = context.Code;
		MetadataReader metadata = context.Metadata;
		IILVisitor visitor = context.Visitor;
		int num = code.Position + code_size;
		while (code.Position < num)
		{
			byte b = code.ReadByte();
			OpCode opCode = ((b != 254) ? OpCodes.OneByteOpCode[b] : OpCodes.TwoBytesOpCode[code.ReadByte()]);
			switch (opCode.OperandType)
			{
			case OperandType.InlineNone:
				visitor.OnInlineNone(opCode);
				break;
			case OperandType.InlineSwitch:
			{
				int num2 = code.ReadInt32();
				int[] array = new int[num2];
				for (int i = 0; i < num2; i++)
				{
					array[i] = code.ReadInt32();
				}
				visitor.OnInlineSwitch(opCode, array);
				break;
			}
			case OperandType.ShortInlineBrTarget:
				visitor.OnInlineBranch(opCode, code.ReadSByte());
				break;
			case OperandType.InlineBrTarget:
				visitor.OnInlineBranch(opCode, code.ReadInt32());
				break;
			case OperandType.ShortInlineI:
				if (opCode == OpCodes.Ldc_I4_S)
				{
					visitor.OnInlineSByte(opCode, code.ReadSByte());
				}
				else
				{
					visitor.OnInlineByte(opCode, code.ReadByte());
				}
				break;
			case OperandType.InlineI:
				visitor.OnInlineInt32(opCode, code.ReadInt32());
				break;
			case OperandType.InlineI8:
				visitor.OnInlineInt64(opCode, code.ReadInt64());
				break;
			case OperandType.ShortInlineR:
				visitor.OnInlineSingle(opCode, code.ReadSingle());
				break;
			case OperandType.InlineR:
				visitor.OnInlineDouble(opCode, code.ReadDouble());
				break;
			case OperandType.InlineSig:
				visitor.OnInlineSignature(opCode, code.GetCallSite(code.ReadToken()));
				break;
			case OperandType.InlineString:
				visitor.OnInlineString(opCode, code.GetString(code.ReadToken()));
				break;
			case OperandType.ShortInlineArg:
				visitor.OnInlineArgument(opCode, code.GetParameter(code.ReadByte()));
				break;
			case OperandType.InlineArg:
				visitor.OnInlineArgument(opCode, code.GetParameter(code.ReadInt16()));
				break;
			case OperandType.ShortInlineVar:
				visitor.OnInlineVariable(opCode, GetVariable(context, code.ReadByte()));
				break;
			case OperandType.InlineVar:
				visitor.OnInlineVariable(opCode, GetVariable(context, code.ReadInt16()));
				break;
			case OperandType.InlineField:
			case OperandType.InlineMethod:
			case OperandType.InlineTok:
			case OperandType.InlineType:
			{
				IMetadataTokenProvider metadataTokenProvider = metadata.LookupToken(code.ReadToken());
				switch (metadataTokenProvider.MetadataToken.TokenType)
				{
				case TokenType.TypeRef:
				case TokenType.TypeDef:
				case TokenType.TypeSpec:
					visitor.OnInlineType(opCode, (TypeReference)metadataTokenProvider);
					break;
				case TokenType.Method:
				case TokenType.MethodSpec:
					visitor.OnInlineMethod(opCode, (MethodReference)metadataTokenProvider);
					break;
				case TokenType.Field:
					visitor.OnInlineField(opCode, (FieldReference)metadataTokenProvider);
					break;
				case TokenType.MemberRef:
					if (metadataTokenProvider is FieldReference field)
					{
						visitor.OnInlineField(opCode, field);
						break;
					}
					if (metadataTokenProvider is MethodReference method)
					{
						visitor.OnInlineMethod(opCode, method);
						break;
					}
					throw new InvalidOperationException();
				}
				break;
			}
			}
		}
	}

	private static VariableDefinition GetVariable(ParseContext context, int index)
	{
		return context.Variables[index];
	}
}
