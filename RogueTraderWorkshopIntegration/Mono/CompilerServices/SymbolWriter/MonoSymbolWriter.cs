using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class MonoSymbolWriter
{
	private List<SourceMethodBuilder> methods;

	private List<SourceFileEntry> sources;

	private List<CompileUnitEntry> comp_units;

	protected readonly MonoSymbolFile file;

	private string filename;

	private SourceMethodBuilder current_method;

	private Stack<SourceMethodBuilder> current_method_stack = new Stack<SourceMethodBuilder>();

	public MonoSymbolFile SymbolFile => file;

	public MonoSymbolWriter(string filename)
	{
		methods = new List<SourceMethodBuilder>();
		sources = new List<SourceFileEntry>();
		comp_units = new List<CompileUnitEntry>();
		file = new MonoSymbolFile();
		this.filename = filename + ".mdb";
	}

	public void CloseNamespace()
	{
	}

	public void DefineLocalVariable(int index, string name)
	{
		if (current_method != null)
		{
			current_method.AddLocal(index, name);
		}
	}

	public void DefineCapturedLocal(int scope_id, string name, string captured_name)
	{
		file.DefineCapturedVariable(scope_id, name, captured_name, CapturedVariable.CapturedKind.Local);
	}

	public void DefineCapturedParameter(int scope_id, string name, string captured_name)
	{
		file.DefineCapturedVariable(scope_id, name, captured_name, CapturedVariable.CapturedKind.Parameter);
	}

	public void DefineCapturedThis(int scope_id, string captured_name)
	{
		file.DefineCapturedVariable(scope_id, "this", captured_name, CapturedVariable.CapturedKind.This);
	}

	public void DefineCapturedScope(int scope_id, int id, string captured_name)
	{
		file.DefineCapturedScope(scope_id, id, captured_name);
	}

	public void DefineScopeVariable(int scope, int index)
	{
		if (current_method != null)
		{
			current_method.AddScopeVariable(scope, index);
		}
	}

	public void MarkSequencePoint(int offset, SourceFileEntry file, int line, int column, bool is_hidden)
	{
		if (current_method != null)
		{
			current_method.MarkSequencePoint(offset, file, line, column, is_hidden);
		}
	}

	public SourceMethodBuilder OpenMethod(ICompileUnit file, int ns_id, IMethodDef method)
	{
		SourceMethodBuilder result = new SourceMethodBuilder(file, ns_id, method);
		current_method_stack.Push(current_method);
		current_method = result;
		methods.Add(current_method);
		return result;
	}

	public void CloseMethod()
	{
		current_method = current_method_stack.Pop();
	}

	public SourceFileEntry DefineDocument(string url)
	{
		SourceFileEntry sourceFileEntry = new SourceFileEntry(file, url);
		sources.Add(sourceFileEntry);
		return sourceFileEntry;
	}

	public SourceFileEntry DefineDocument(string url, byte[] guid, byte[] checksum)
	{
		SourceFileEntry sourceFileEntry = new SourceFileEntry(file, url, guid, checksum);
		sources.Add(sourceFileEntry);
		return sourceFileEntry;
	}

	public CompileUnitEntry DefineCompilationUnit(SourceFileEntry source)
	{
		CompileUnitEntry compileUnitEntry = new CompileUnitEntry(file, source);
		comp_units.Add(compileUnitEntry);
		return compileUnitEntry;
	}

	public int DefineNamespace(string name, CompileUnitEntry unit, string[] using_clauses, int parent)
	{
		if (unit == null || using_clauses == null)
		{
			throw new NullReferenceException();
		}
		return unit.DefineNamespace(name, using_clauses, parent);
	}

	public int OpenScope(int start_offset)
	{
		if (current_method == null)
		{
			return 0;
		}
		current_method.StartBlock(CodeBlockEntry.Type.Lexical, start_offset);
		return 0;
	}

	public void CloseScope(int end_offset)
	{
		if (current_method != null)
		{
			current_method.EndBlock(end_offset);
		}
	}

	public void OpenCompilerGeneratedBlock(int start_offset)
	{
		if (current_method != null)
		{
			current_method.StartBlock(CodeBlockEntry.Type.CompilerGenerated, start_offset);
		}
	}

	public void CloseCompilerGeneratedBlock(int end_offset)
	{
		if (current_method != null)
		{
			current_method.EndBlock(end_offset);
		}
	}

	public void StartIteratorBody(int start_offset)
	{
		current_method.StartBlock(CodeBlockEntry.Type.IteratorBody, start_offset);
	}

	public void EndIteratorBody(int end_offset)
	{
		current_method.EndBlock(end_offset);
	}

	public void StartIteratorDispatcher(int start_offset)
	{
		current_method.StartBlock(CodeBlockEntry.Type.IteratorDispatcher, start_offset);
	}

	public void EndIteratorDispatcher(int end_offset)
	{
		current_method.EndBlock(end_offset);
	}

	public void DefineAnonymousScope(int id)
	{
		file.DefineAnonymousScope(id);
	}

	public void WriteSymbolFile(Guid guid)
	{
		foreach (SourceMethodBuilder method in methods)
		{
			method.DefineMethod(file);
		}
		try
		{
			File.Delete(filename);
		}
		catch
		{
		}
		using FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
		file.CreateSymbolFile(guid, fs);
	}
}
