using System;
using System.Collections;
using System.Diagnostics.SymbolStore;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.CompilerServices.SymbolWriter;

[ComVisible(false)]
public class SymbolWriterImpl : ISymbolWriter
{
	private MonoSymbolWriter msw;

	private int nextLocalIndex;

	private int currentToken;

	private string methodName;

	private Stack namespaceStack = new Stack();

	private bool methodOpened;

	private Hashtable documents = new Hashtable();

	private Guid guid;

	public SymbolWriterImpl(Guid guid)
	{
		this.guid = guid;
	}

	public void Close()
	{
		msw.WriteSymbolFile(guid);
	}

	public void CloseMethod()
	{
		if (methodOpened)
		{
			methodOpened = false;
			nextLocalIndex = 0;
			msw.CloseMethod();
		}
	}

	public void CloseNamespace()
	{
		namespaceStack.Pop();
		msw.CloseNamespace();
	}

	public void CloseScope(int endOffset)
	{
		msw.CloseScope(endOffset);
	}

	public ISymbolDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
	{
		SymbolDocumentWriterImpl symbolDocumentWriterImpl = (SymbolDocumentWriterImpl)documents[url];
		if (symbolDocumentWriterImpl == null)
		{
			SourceFileEntry source = msw.DefineDocument(url);
			symbolDocumentWriterImpl = new SymbolDocumentWriterImpl(msw.DefineCompilationUnit(source));
			documents[url] = symbolDocumentWriterImpl;
		}
		return symbolDocumentWriterImpl;
	}

	public void DefineField(SymbolToken parent, string name, FieldAttributes attributes, byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3)
	{
	}

	public void DefineGlobalVariable(string name, FieldAttributes attributes, byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3)
	{
	}

	public void DefineLocalVariable(string name, FieldAttributes attributes, byte[] signature, SymAddressKind addrKind, int addr1, int addr2, int addr3, int startOffset, int endOffset)
	{
		msw.DefineLocalVariable(nextLocalIndex++, name);
	}

	public void DefineParameter(string name, ParameterAttributes attributes, int sequence, SymAddressKind addrKind, int addr1, int addr2, int addr3)
	{
	}

	public void DefineSequencePoints(ISymbolDocumentWriter document, int[] offsets, int[] lines, int[] columns, int[] endLines, int[] endColumns)
	{
		SourceFileEntry file = ((SymbolDocumentWriterImpl)document)?.Entry.SourceFile;
		for (int i = 0; i < offsets.Length; i++)
		{
			if (i <= 0 || offsets[i] != offsets[i - 1] || lines[i] != lines[i - 1] || columns[i] != columns[i - 1])
			{
				msw.MarkSequencePoint(offsets[i], file, lines[i], columns[i], is_hidden: false);
			}
		}
	}

	public void Initialize(IntPtr emitter, string filename, bool fFullBuild)
	{
		msw = new MonoSymbolWriter(filename);
	}

	public void OpenMethod(SymbolToken method)
	{
		currentToken = method.GetToken();
	}

	public void OpenNamespace(string name)
	{
		NamespaceInfo namespaceInfo = new NamespaceInfo();
		namespaceInfo.NamespaceID = -1;
		namespaceInfo.Name = name;
		namespaceStack.Push(namespaceInfo);
	}

	public int OpenScope(int startOffset)
	{
		return msw.OpenScope(startOffset);
	}

	public void SetMethodSourceRange(ISymbolDocumentWriter startDoc, int startLine, int startColumn, ISymbolDocumentWriter endDoc, int endLine, int endColumn)
	{
		int currentNamespace = GetCurrentNamespace(startDoc);
		SourceMethodImpl method = new SourceMethodImpl(methodName, currentToken, currentNamespace);
		msw.OpenMethod(((ICompileUnit)startDoc).Entry, currentNamespace, method);
		methodOpened = true;
	}

	public void SetScopeRange(int scopeID, int startOffset, int endOffset)
	{
	}

	public void SetSymAttribute(SymbolToken parent, string name, byte[] data)
	{
		if (name == "__name")
		{
			methodName = Encoding.UTF8.GetString(data);
		}
	}

	public void SetUnderlyingWriter(IntPtr underlyingWriter)
	{
	}

	public void SetUserEntryPoint(SymbolToken entryMethod)
	{
	}

	public void UsingNamespace(string fullName)
	{
		if (namespaceStack.Count == 0)
		{
			OpenNamespace("");
		}
		NamespaceInfo namespaceInfo = (NamespaceInfo)namespaceStack.Peek();
		if (namespaceInfo.NamespaceID != -1)
		{
			NamespaceInfo namespaceInfo2 = namespaceInfo;
			CloseNamespace();
			OpenNamespace(namespaceInfo2.Name);
			namespaceInfo = (NamespaceInfo)namespaceStack.Peek();
			namespaceInfo.UsingClauses = namespaceInfo2.UsingClauses;
		}
		namespaceInfo.UsingClauses.Add(fullName);
	}

	private int GetCurrentNamespace(ISymbolDocumentWriter doc)
	{
		if (namespaceStack.Count == 0)
		{
			OpenNamespace("");
		}
		NamespaceInfo namespaceInfo = (NamespaceInfo)namespaceStack.Peek();
		if (namespaceInfo.NamespaceID == -1)
		{
			string[] using_clauses = (string[])namespaceInfo.UsingClauses.ToArray(typeof(string));
			int parent = 0;
			if (namespaceStack.Count > 1)
			{
				namespaceStack.Pop();
				parent = ((NamespaceInfo)namespaceStack.Peek()).NamespaceID;
				namespaceStack.Push(namespaceInfo);
			}
			namespaceInfo.NamespaceID = msw.DefineNamespace(namespaceInfo.Name, ((ICompileUnit)doc).Entry, using_clauses, parent);
		}
		return namespaceInfo.NamespaceID;
	}
}
