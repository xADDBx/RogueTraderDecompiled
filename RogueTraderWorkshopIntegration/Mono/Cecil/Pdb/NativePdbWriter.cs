using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Mono.Cecil.Cil;
using Mono.Cecil.PE;
using Mono.Collections.Generic;

namespace Mono.Cecil.Pdb;

[ComVisible(false)]
public class NativePdbWriter : ISymbolWriter, IDisposable
{
	private readonly ModuleDefinition module;

	private readonly MetadataBuilder metadata;

	private readonly SymWriter writer;

	private readonly Dictionary<string, SymDocumentWriter> documents;

	private readonly Dictionary<ImportDebugInformation, MetadataToken> import_info_to_parent;

	private ImageDebugDirectory debug_directory;

	private byte[] debug_info;

	internal NativePdbWriter(ModuleDefinition module, SymWriter writer)
	{
		this.module = module;
		metadata = module.metadata_builder;
		this.writer = writer;
		documents = new Dictionary<string, SymDocumentWriter>();
		import_info_to_parent = new Dictionary<ImportDebugInformation, MetadataToken>();
	}

	public ISymbolReaderProvider GetReaderProvider()
	{
		return new NativePdbReaderProvider();
	}

	public ImageDebugHeader GetDebugHeader()
	{
		return new ImageDebugHeader(new ImageDebugHeaderEntry(debug_directory, debug_info));
	}

	public void Write(MethodDebugInformation info)
	{
		int methodToken = info.method.MetadataToken.ToInt32();
		if (info.HasSequencePoints || info.scope != null || info.HasCustomDebugInformations || info.StateMachineKickOffMethod != null)
		{
			writer.OpenMethod(methodToken);
			if (!info.sequence_points.IsNullOrEmpty())
			{
				DefineSequencePoints(info.sequence_points);
			}
			MetadataToken import_parent = default(MetadataToken);
			if (info.scope != null)
			{
				DefineScope(info.scope, info, out import_parent);
			}
			DefineCustomMetadata(info, import_parent);
			writer.CloseMethod();
		}
	}

	private void DefineCustomMetadata(MethodDebugInformation info, MetadataToken import_parent)
	{
		CustomMetadataWriter customMetadataWriter = new CustomMetadataWriter(writer);
		if (import_parent.RID != 0)
		{
			customMetadataWriter.WriteForwardInfo(import_parent);
		}
		else if (info.scope != null && info.scope.Import != null && info.scope.Import.HasTargets)
		{
			customMetadataWriter.WriteUsingInfo(info.scope.Import);
		}
		if (info.Method.HasCustomAttributes)
		{
			foreach (CustomAttribute customAttribute in info.Method.CustomAttributes)
			{
				TypeReference attributeType = customAttribute.AttributeType;
				if ((attributeType.IsTypeOf("System.Runtime.CompilerServices", "IteratorStateMachineAttribute") || attributeType.IsTypeOf("System.Runtime.CompilerServices", "AsyncStateMachineAttribute")) && customAttribute.ConstructorArguments[0].Value is TypeReference type)
				{
					customMetadataWriter.WriteForwardIterator(type);
				}
			}
		}
		if (info.HasCustomDebugInformations && info.CustomDebugInformations.FirstOrDefault((CustomDebugInformation cdi) => cdi.Kind == CustomDebugInformationKind.StateMachineScope) is StateMachineScopeDebugInformation state_machine)
		{
			customMetadataWriter.WriteIteratorScopes(state_machine, info);
		}
		customMetadataWriter.WriteCustomMetadata();
		DefineAsyncCustomMetadata(info);
	}

	private void DefineAsyncCustomMetadata(MethodDebugInformation info)
	{
		if (!info.HasCustomDebugInformations)
		{
			return;
		}
		foreach (CustomDebugInformation customDebugInformation in info.CustomDebugInformations)
		{
			if (!(customDebugInformation is AsyncMethodBodyDebugInformation asyncMethodBodyDebugInformation))
			{
				continue;
			}
			using MemoryStream memoryStream = new MemoryStream();
			BinaryStreamWriter binaryStreamWriter = new BinaryStreamWriter(memoryStream);
			binaryStreamWriter.WriteUInt32((info.StateMachineKickOffMethod != null) ? info.StateMachineKickOffMethod.MetadataToken.ToUInt32() : 0u);
			binaryStreamWriter.WriteUInt32((uint)asyncMethodBodyDebugInformation.CatchHandler.Offset);
			binaryStreamWriter.WriteUInt32((uint)asyncMethodBodyDebugInformation.Resumes.Count);
			for (int i = 0; i < asyncMethodBodyDebugInformation.Resumes.Count; i++)
			{
				binaryStreamWriter.WriteUInt32((uint)asyncMethodBodyDebugInformation.Yields[i].Offset);
				binaryStreamWriter.WriteUInt32(asyncMethodBodyDebugInformation.resume_methods[i].MetadataToken.ToUInt32());
				binaryStreamWriter.WriteUInt32((uint)asyncMethodBodyDebugInformation.Resumes[i].Offset);
			}
			writer.DefineCustomMetadata("asyncMethodInfo", memoryStream.ToArray());
		}
	}

	private void DefineScope(ScopeDebugInformation scope, MethodDebugInformation info, out MetadataToken import_parent)
	{
		int offset = scope.Start.Offset;
		int num = (scope.End.IsEndOfMethod ? info.code_size : scope.End.Offset);
		import_parent = new MetadataToken(0u);
		writer.OpenScope(offset);
		if (scope.Import != null && scope.Import.HasTargets && !import_info_to_parent.TryGetValue(info.scope.Import, out import_parent))
		{
			foreach (ImportTarget target in scope.Import.Targets)
			{
				switch (target.Kind)
				{
				case ImportTargetKind.ImportNamespace:
					writer.UsingNamespace("U" + target.@namespace);
					break;
				case ImportTargetKind.ImportType:
					writer.UsingNamespace("T" + TypeParser.ToParseable(target.type));
					break;
				case ImportTargetKind.DefineNamespaceAlias:
					writer.UsingNamespace("A" + target.Alias + " U" + target.@namespace);
					break;
				case ImportTargetKind.DefineTypeAlias:
					writer.UsingNamespace("A" + target.Alias + " T" + TypeParser.ToParseable(target.type));
					break;
				}
			}
			import_info_to_parent.Add(info.scope.Import, info.method.MetadataToken);
		}
		int local_var_token = info.local_var_token.ToInt32();
		if (!scope.variables.IsNullOrEmpty())
		{
			for (int i = 0; i < scope.variables.Count; i++)
			{
				VariableDebugInformation variable = scope.variables[i];
				DefineLocalVariable(variable, local_var_token, offset, num);
			}
		}
		if (!scope.constants.IsNullOrEmpty())
		{
			for (int j = 0; j < scope.constants.Count; j++)
			{
				ConstantDebugInformation constant = scope.constants[j];
				DefineConstant(constant);
			}
		}
		if (!scope.scopes.IsNullOrEmpty())
		{
			for (int k = 0; k < scope.scopes.Count; k++)
			{
				DefineScope(scope.scopes[k], info, out var _);
			}
		}
		writer.CloseScope(num);
	}

	private void DefineSequencePoints(Collection<SequencePoint> sequence_points)
	{
		for (int i = 0; i < sequence_points.Count; i++)
		{
			SequencePoint sequencePoint = sequence_points[i];
			writer.DefineSequencePoints(GetDocument(sequencePoint.Document), new int[1] { sequencePoint.Offset }, new int[1] { sequencePoint.StartLine }, new int[1] { sequencePoint.StartColumn }, new int[1] { sequencePoint.EndLine }, new int[1] { sequencePoint.EndColumn });
		}
	}

	private void DefineLocalVariable(VariableDebugInformation variable, int local_var_token, int start_offset, int end_offset)
	{
		writer.DefineLocalVariable2(variable.Name, variable.Attributes, local_var_token, variable.Index, 0, 0, start_offset, end_offset);
	}

	private void DefineConstant(ConstantDebugInformation constant)
	{
		uint rid = metadata.AddStandAloneSignature(metadata.GetConstantTypeBlobIndex(constant.ConstantType));
		MetadataToken metadataToken = new MetadataToken(TokenType.Signature, rid);
		writer.DefineConstant2(constant.Name, constant.Value, metadataToken.ToInt32());
	}

	private SymDocumentWriter GetDocument(Document document)
	{
		if (document == null)
		{
			return null;
		}
		if (documents.TryGetValue(document.Url, out var value))
		{
			return value;
		}
		value = writer.DefineDocument(document.Url, document.LanguageGuid, document.LanguageVendorGuid, document.TypeGuid);
		if (!document.Hash.IsNullOrEmpty())
		{
			value.SetCheckSum(document.HashAlgorithmGuid, document.Hash);
		}
		documents[document.Url] = value;
		return value;
	}

	public void Write()
	{
		MethodDefinition entryPoint = module.EntryPoint;
		if (entryPoint != null)
		{
			writer.SetUserEntryPoint(entryPoint.MetadataToken.ToInt32());
		}
		debug_info = writer.GetDebugInfo(out debug_directory);
		debug_directory.TimeDateStamp = (int)module.timestamp;
		writer.Close();
	}

	public void Dispose()
	{
	}
}
