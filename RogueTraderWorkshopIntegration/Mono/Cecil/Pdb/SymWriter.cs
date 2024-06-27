using System;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace Mono.Cecil.Pdb;

internal class SymWriter
{
	private static Guid s_symUnmangedWriterIID = new Guid("0b97726e-9e6d-4f05-9a26-424022093caa");

	private static Guid s_CorSymWriter_SxS_ClassID = new Guid("108296c1-281e-11d3-bd22-0000f80849bd");

	private readonly ISymUnmanagedWriter2 writer;

	private readonly Collection<ISymUnmanagedDocumentWriter> documents;

	[DllImport("ole32.dll")]
	private static extern int CoCreateInstance([In] ref Guid rclsid, [In][MarshalAs(UnmanagedType.IUnknown)] object pUnkOuter, [In] uint dwClsContext, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

	public SymWriter()
	{
		CoCreateInstance(ref s_CorSymWriter_SxS_ClassID, null, 1u, ref s_symUnmangedWriterIID, out var ppv);
		writer = (ISymUnmanagedWriter2)ppv;
		documents = new Collection<ISymUnmanagedDocumentWriter>();
	}

	public byte[] GetDebugInfo(out ImageDebugDirectory idd)
	{
		writer.GetDebugInfo(out idd, 0, out var pcData, null);
		byte[] array = new byte[pcData];
		writer.GetDebugInfo(out idd, pcData, out pcData, array);
		return array;
	}

	public void DefineLocalVariable2(string name, VariableAttributes attributes, int sigToken, int addr1, int addr2, int addr3, int startOffset, int endOffset)
	{
		writer.DefineLocalVariable2(name, (int)attributes, sigToken, 1, addr1, addr2, addr3, startOffset, endOffset);
	}

	public void DefineConstant2(string name, object value, int sigToken)
	{
		if (value == null)
		{
			writer.DefineConstant2(name, 0, sigToken);
		}
		else
		{
			writer.DefineConstant2(name, value, sigToken);
		}
	}

	public void Close()
	{
		writer.Close();
		Marshal.ReleaseComObject(writer);
		foreach (ISymUnmanagedDocumentWriter document in documents)
		{
			Marshal.ReleaseComObject(document);
		}
	}

	public void CloseMethod()
	{
		writer.CloseMethod();
	}

	public void CloseNamespace()
	{
		writer.CloseNamespace();
	}

	public void CloseScope(int endOffset)
	{
		writer.CloseScope(endOffset);
	}

	public SymDocumentWriter DefineDocument(string url, Guid language, Guid languageVendor, Guid documentType)
	{
		writer.DefineDocument(url, ref language, ref languageVendor, ref documentType, out var pRetVal);
		documents.Add(pRetVal);
		return new SymDocumentWriter(pRetVal);
	}

	public void DefineSequencePoints(SymDocumentWriter document, int[] offsets, int[] lines, int[] columns, int[] endLines, int[] endColumns)
	{
		writer.DefineSequencePoints(document.Writer, offsets.Length, offsets, lines, columns, endLines, endColumns);
	}

	public void Initialize(object emitter, string filename, bool fFullBuild)
	{
		writer.Initialize(emitter, filename, null, fFullBuild);
	}

	public void SetUserEntryPoint(int methodToken)
	{
		writer.SetUserEntryPoint(methodToken);
	}

	public void OpenMethod(int methodToken)
	{
		writer.OpenMethod(methodToken);
	}

	public void OpenNamespace(string name)
	{
		writer.OpenNamespace(name);
	}

	public int OpenScope(int startOffset)
	{
		writer.OpenScope(startOffset, out var pRetVal);
		return pRetVal;
	}

	public void UsingNamespace(string fullName)
	{
		writer.UsingNamespace(fullName);
	}

	public void DefineCustomMetadata(string name, byte[] metadata)
	{
		GCHandle gCHandle = GCHandle.Alloc(metadata, GCHandleType.Pinned);
		writer.SetSymAttribute(0u, name, (uint)metadata.Length, gCHandle.AddrOfPinnedObject());
		gCHandle.Free();
	}
}
