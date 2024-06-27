using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Mono.CompilerServices.SymbolWriter;

namespace Mono.Cecil.Mdb;

[ComVisible(false)]
public sealed class MdbWriter : ISymbolWriter, IDisposable
{
	private class SourceFile : ISourceFile
	{
		private readonly CompileUnitEntry compilation_unit;

		private readonly SourceFileEntry entry;

		public SourceFileEntry Entry => entry;

		public CompileUnitEntry CompilationUnit => compilation_unit;

		public SourceFile(CompileUnitEntry comp_unit, SourceFileEntry entry)
		{
			compilation_unit = comp_unit;
			this.entry = entry;
		}
	}

	private class SourceMethod : IMethodDef
	{
		private readonly MethodDefinition method;

		public string Name => method.Name;

		public int Token => method.MetadataToken.ToInt32();

		public SourceMethod(MethodDefinition method)
		{
			this.method = method;
		}
	}

	private readonly ModuleDefinition module;

	private readonly MonoSymbolWriter writer;

	private readonly Dictionary<string, SourceFile> source_files;

	public MdbWriter(ModuleDefinition module, string assembly)
	{
		this.module = module;
		writer = new MonoSymbolWriter(assembly);
		source_files = new Dictionary<string, SourceFile>();
	}

	public ISymbolReaderProvider GetReaderProvider()
	{
		return new MdbReaderProvider();
	}

	private SourceFile GetSourceFile(Document document)
	{
		string url = document.Url;
		if (source_files.TryGetValue(url, out var value))
		{
			return value;
		}
		SourceFileEntry sourceFileEntry = writer.DefineDocument(url, null, (document.Hash != null && document.Hash.Length == 16) ? document.Hash : null);
		value = new SourceFile(writer.DefineCompilationUnit(sourceFileEntry), sourceFileEntry);
		source_files.Add(url, value);
		return value;
	}

	private void Populate(Collection<SequencePoint> sequencePoints, int[] offsets, int[] startRows, int[] endRows, int[] startCols, int[] endCols, out SourceFile file)
	{
		SourceFile sourceFile = null;
		for (int i = 0; i < sequencePoints.Count; i++)
		{
			SequencePoint sequencePoint = sequencePoints[i];
			offsets[i] = sequencePoint.Offset;
			if (sourceFile == null)
			{
				sourceFile = GetSourceFile(sequencePoint.Document);
			}
			startRows[i] = sequencePoint.StartLine;
			endRows[i] = sequencePoint.EndLine;
			startCols[i] = sequencePoint.StartColumn;
			endCols[i] = sequencePoint.EndColumn;
		}
		file = sourceFile;
	}

	public void Write(MethodDebugInformation info)
	{
		SourceMethod method = new SourceMethod(info.method);
		Collection<SequencePoint> sequencePoints = info.SequencePoints;
		int count = sequencePoints.Count;
		if (count != 0)
		{
			int[] array = new int[count];
			int[] array2 = new int[count];
			int[] array3 = new int[count];
			int[] array4 = new int[count];
			int[] array5 = new int[count];
			Populate(sequencePoints, array, array2, array3, array4, array5, out var file);
			SourceMethodBuilder sourceMethodBuilder = writer.OpenMethod(file.CompilationUnit, 0, method);
			for (int i = 0; i < count; i++)
			{
				sourceMethodBuilder.MarkSequencePoint(array[i], file.CompilationUnit.SourceFile, array2[i], array4[i], array3[i], array5[i], is_hidden: false);
			}
			if (info.scope != null)
			{
				WriteRootScope(info.scope, info);
			}
			writer.CloseMethod();
		}
	}

	private void WriteRootScope(ScopeDebugInformation scope, MethodDebugInformation info)
	{
		WriteScopeVariables(scope);
		if (scope.HasScopes)
		{
			WriteScopes(scope.Scopes, info);
		}
	}

	private void WriteScope(ScopeDebugInformation scope, MethodDebugInformation info)
	{
		writer.OpenScope(scope.Start.Offset);
		WriteScopeVariables(scope);
		if (scope.HasScopes)
		{
			WriteScopes(scope.Scopes, info);
		}
		writer.CloseScope(scope.End.IsEndOfMethod ? info.code_size : scope.End.Offset);
	}

	private void WriteScopes(Collection<ScopeDebugInformation> scopes, MethodDebugInformation info)
	{
		for (int i = 0; i < scopes.Count; i++)
		{
			WriteScope(scopes[i], info);
		}
	}

	private void WriteScopeVariables(ScopeDebugInformation scope)
	{
		if (!scope.HasVariables)
		{
			return;
		}
		foreach (VariableDebugInformation variable in scope.variables)
		{
			if (!string.IsNullOrEmpty(variable.Name))
			{
				writer.DefineLocalVariable(variable.Index, variable.Name);
			}
		}
	}

	public ImageDebugHeader GetDebugHeader()
	{
		return new ImageDebugHeader();
	}

	public void Write()
	{
	}

	public void Dispose()
	{
		writer.WriteSymbolFile(module.Mvid);
	}
}
