using System;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;

namespace Mono.Cecil.Pdb;

[ComVisible(false)]
public sealed class NativePdbWriterProvider : ISymbolWriterProvider
{
	public ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName)
	{
		Mixin.CheckModule(module);
		Mixin.CheckFileName(fileName);
		return new NativePdbWriter(module, CreateWriter(module, Mixin.GetPdbFileName(fileName)));
	}

	private static SymWriter CreateWriter(ModuleDefinition module, string pdb)
	{
		SymWriter symWriter = new SymWriter();
		if (File.Exists(pdb))
		{
			File.Delete(pdb);
		}
		symWriter.Initialize(new ModuleMetadata(module), pdb, fFullBuild: true);
		return symWriter;
	}

	public ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream)
	{
		throw new NotImplementedException();
	}
}
