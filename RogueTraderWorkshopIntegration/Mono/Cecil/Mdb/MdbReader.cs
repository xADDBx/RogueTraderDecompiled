using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.Cecil.Mdb;

[ComVisible(false)]
public sealed class MdbReader : ISymbolReader, IDisposable
{
	private readonly ModuleDefinition module;

	private readonly MonoSymbolFile symbol_file;

	private readonly Dictionary<string, Document> documents;

	public MdbReader(ModuleDefinition module, MonoSymbolFile symFile)
	{
		this.module = module;
		symbol_file = symFile;
		documents = new Dictionary<string, Document>();
	}

	public ISymbolWriterProvider GetWriterProvider()
	{
		return new MdbWriterProvider();
	}

	public bool ProcessDebugHeader(ImageDebugHeader header)
	{
		return symbol_file.Guid == module.Mvid;
	}

	public MethodDebugInformation Read(MethodDefinition method)
	{
		MetadataToken metadataToken = method.MetadataToken;
		MethodEntry methodByToken = symbol_file.GetMethodByToken(metadataToken.ToInt32());
		if (methodByToken == null)
		{
			return null;
		}
		MethodDebugInformation methodDebugInformation = new MethodDebugInformation(method);
		methodDebugInformation.code_size = ReadCodeSize(method);
		ScopeDebugInformation[] scopes = ReadScopes(methodByToken, methodDebugInformation);
		ReadLineNumbers(methodByToken, methodDebugInformation);
		ReadLocalVariables(methodByToken, scopes);
		return methodDebugInformation;
	}

	private static int ReadCodeSize(MethodDefinition method)
	{
		return method.Module.Read(method, (MethodDefinition m, MetadataReader reader) => reader.ReadCodeSize(m));
	}

	private static void ReadLocalVariables(MethodEntry entry, ScopeDebugInformation[] scopes)
	{
		LocalVariableEntry[] locals = entry.GetLocals();
		for (int i = 0; i < locals.Length; i++)
		{
			LocalVariableEntry localVariableEntry = locals[i];
			VariableDebugInformation item = new VariableDebugInformation(localVariableEntry.Index, localVariableEntry.Name);
			int blockIndex = localVariableEntry.BlockIndex;
			if (blockIndex >= 0 && blockIndex < scopes.Length)
			{
				scopes[blockIndex]?.Variables.Add(item);
			}
		}
	}

	private void ReadLineNumbers(MethodEntry entry, MethodDebugInformation info)
	{
		LineNumberTable lineNumberTable = entry.GetLineNumberTable();
		info.sequence_points = new Collection<SequencePoint>(lineNumberTable.LineNumbers.Length);
		for (int i = 0; i < lineNumberTable.LineNumbers.Length; i++)
		{
			LineNumberEntry lineNumberEntry = lineNumberTable.LineNumbers[i];
			if (i <= 0 || lineNumberTable.LineNumbers[i - 1].Offset != lineNumberEntry.Offset)
			{
				info.sequence_points.Add(LineToSequencePoint(lineNumberEntry));
			}
		}
	}

	private Document GetDocument(SourceFileEntry file)
	{
		string fileName = file.FileName;
		if (documents.TryGetValue(fileName, out var value))
		{
			return value;
		}
		value = new Document(fileName)
		{
			Hash = file.Checksum
		};
		documents.Add(fileName, value);
		return value;
	}

	private static ScopeDebugInformation[] ReadScopes(MethodEntry entry, MethodDebugInformation info)
	{
		CodeBlockEntry[] codeBlocks = entry.GetCodeBlocks();
		ScopeDebugInformation[] array = new ScopeDebugInformation[codeBlocks.Length + 1];
		ScopeDebugInformation obj = new ScopeDebugInformation
		{
			Start = new InstructionOffset(0),
			End = new InstructionOffset(info.code_size)
		};
		ScopeDebugInformation scope = obj;
		array[0] = obj;
		info.scope = scope;
		CodeBlockEntry[] array2 = codeBlocks;
		foreach (CodeBlockEntry codeBlockEntry in array2)
		{
			if (codeBlockEntry.BlockType == CodeBlockEntry.Type.Lexical || codeBlockEntry.BlockType == CodeBlockEntry.Type.CompilerGenerated)
			{
				ScopeDebugInformation scopeDebugInformation = new ScopeDebugInformation();
				scopeDebugInformation.Start = new InstructionOffset(codeBlockEntry.StartOffset);
				scopeDebugInformation.End = new InstructionOffset(codeBlockEntry.EndOffset);
				array[codeBlockEntry.Index + 1] = scopeDebugInformation;
				if (!AddScope(info.scope.Scopes, scopeDebugInformation))
				{
					info.scope.Scopes.Add(scopeDebugInformation);
				}
			}
		}
		return array;
	}

	private static bool AddScope(Collection<ScopeDebugInformation> scopes, ScopeDebugInformation scope)
	{
		foreach (ScopeDebugInformation scope2 in scopes)
		{
			if (scope2.HasScopes && AddScope(scope2.Scopes, scope))
			{
				return true;
			}
			if (scope.Start.Offset >= scope2.Start.Offset && scope.End.Offset <= scope2.End.Offset)
			{
				scope2.Scopes.Add(scope);
				return true;
			}
		}
		return false;
	}

	private SequencePoint LineToSequencePoint(LineNumberEntry line)
	{
		SourceFileEntry sourceFile = symbol_file.GetSourceFile(line.File);
		return new SequencePoint(line.Offset, GetDocument(sourceFile))
		{
			StartLine = line.Row,
			EndLine = line.EndRow,
			StartColumn = line.Column,
			EndColumn = line.EndColumn
		};
	}

	public void Dispose()
	{
		symbol_file.Dispose();
	}
}
