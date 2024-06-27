using System;
using System.IO;
using Mono.Cecil.Cil;
using Mono.Cecil.PE;

namespace Mono.Cecil;

internal static class ModuleWriter
{
	public static void WriteModule(ModuleDefinition module, Disposable<Stream> stream, WriterParameters parameters)
	{
		using (stream)
		{
			Write(module, stream, parameters);
		}
	}

	private static void Write(ModuleDefinition module, Disposable<Stream> stream, WriterParameters parameters)
	{
		if ((module.Attributes & ModuleAttributes.ILOnly) == 0)
		{
			throw new NotSupportedException("Writing mixed-mode assemblies is not supported");
		}
		if (module.HasImage && module.ReadingMode == ReadingMode.Deferred)
		{
			ImmediateModuleReader immediateModuleReader = new ImmediateModuleReader(module.Image);
			immediateModuleReader.ReadModule(module, resolve_attributes: false);
			immediateModuleReader.ReadSymbols(module);
		}
		module.MetadataSystem.Clear();
		if (module.symbol_reader != null)
		{
			module.symbol_reader.Dispose();
		}
		AssemblyNameDefinition assemblyNameDefinition = ((module.assembly != null && module.kind != ModuleKind.NetModule) ? module.assembly.Name : null);
		string fileName = stream.value.GetFileName();
		uint timestamp = parameters.Timestamp ?? module.timestamp;
		ISymbolWriterProvider symbolWriterProvider = parameters.SymbolWriterProvider;
		if (symbolWriterProvider == null && parameters.WriteSymbols)
		{
			symbolWriterProvider = new DefaultSymbolWriterProvider();
		}
		if (parameters.HasStrongNameKey && assemblyNameDefinition != null)
		{
			assemblyNameDefinition.PublicKey = CryptoService.GetPublicKey(parameters);
			module.Attributes |= ModuleAttributes.StrongNameSigned;
		}
		if (parameters.DeterministicMvid)
		{
			module.Mvid = Guid.Empty;
		}
		MetadataBuilder metadataBuilder = new MetadataBuilder(module, fileName, timestamp, symbolWriterProvider);
		try
		{
			module.metadata_builder = metadataBuilder;
			using ISymbolWriter symbolWriter = GetSymbolWriter(module, fileName, symbolWriterProvider, parameters);
			metadataBuilder.SetSymbolWriter(symbolWriter);
			BuildMetadata(module, metadataBuilder);
			symbolWriter?.Write();
			ImageWriter imageWriter = ImageWriter.CreateWriter(module, metadataBuilder, stream);
			stream.value.SetLength(0L);
			imageWriter.WriteImage();
			if (parameters.DeterministicMvid)
			{
				ComputeDeterministicMvid(imageWriter, module);
			}
			if (parameters.HasStrongNameKey)
			{
				CryptoService.StrongName(stream.value, imageWriter, parameters);
			}
		}
		finally
		{
			module.metadata_builder = null;
		}
	}

	private static void BuildMetadata(ModuleDefinition module, MetadataBuilder metadata)
	{
		if (!module.HasImage)
		{
			metadata.BuildMetadata();
			return;
		}
		module.Read(metadata, delegate(MetadataBuilder builder, MetadataReader _)
		{
			builder.BuildMetadata();
			return builder;
		});
	}

	private static ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fq_name, ISymbolWriterProvider symbol_writer_provider, WriterParameters parameters)
	{
		if (symbol_writer_provider == null)
		{
			return null;
		}
		if (parameters.SymbolStream != null)
		{
			return symbol_writer_provider.GetSymbolWriter(module, parameters.SymbolStream);
		}
		return symbol_writer_provider.GetSymbolWriter(module, fq_name);
	}

	private static void ComputeDeterministicMvid(ImageWriter writer, ModuleDefinition module)
	{
		long position = writer.BaseStream.Position;
		writer.BaseStream.Seek(0L, SeekOrigin.Begin);
		Guid mvid = CryptoService.ComputeGuid(CryptoService.ComputeHash(writer.BaseStream));
		writer.MoveToRVA(TextSegment.GuidHeap);
		writer.WriteBytes(mvid.ToByteArray());
		writer.Flush();
		module.Mvid = mvid;
		writer.BaseStream.Seek(position, SeekOrigin.Begin);
	}
}
