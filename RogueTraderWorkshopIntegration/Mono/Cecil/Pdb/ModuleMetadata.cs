using System;
using System.Collections.Generic;
using System.Text;

namespace Mono.Cecil.Pdb;

internal class ModuleMetadata : IMetaDataEmit, IMetaDataImport
{
	private readonly ModuleDefinition module;

	private Dictionary<uint, TypeDefinition> types;

	private Dictionary<uint, MethodDefinition> methods;

	private const uint S_OK = 0u;

	private const uint E_FAIL = 2147500037u;

	public ModuleMetadata(ModuleDefinition module)
	{
		this.module = module;
	}

	private bool TryGetType(uint token, out TypeDefinition type)
	{
		if (types == null)
		{
			InitializeMetadata(module);
		}
		return types.TryGetValue(token, out type);
	}

	private bool TryGetMethod(uint token, out MethodDefinition method)
	{
		if (methods == null)
		{
			InitializeMetadata(module);
		}
		return methods.TryGetValue(token, out method);
	}

	private void InitializeMetadata(ModuleDefinition module)
	{
		types = new Dictionary<uint, TypeDefinition>();
		methods = new Dictionary<uint, MethodDefinition>();
		foreach (TypeDefinition type in module.GetTypes())
		{
			types.Add(type.MetadataToken.ToUInt32(), type);
			InitializeMethods(type);
		}
	}

	private void InitializeMethods(TypeDefinition type)
	{
		foreach (MethodDefinition method in type.Methods)
		{
			methods.Add(method.MetadataToken.ToUInt32(), method);
		}
	}

	public void SetModuleProps(string szName)
	{
		throw new NotImplementedException();
	}

	public void Save(string szFile, uint dwSaveFlags)
	{
		throw new NotImplementedException();
	}

	public void SaveToStream(IntPtr pIStream, uint dwSaveFlags)
	{
		throw new NotImplementedException();
	}

	public uint GetSaveSize(uint fSave)
	{
		throw new NotImplementedException();
	}

	public uint DefineTypeDef(IntPtr szTypeDef, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements)
	{
		throw new NotImplementedException();
	}

	public uint DefineNestedType(IntPtr szTypeDef, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements, uint tdEncloser)
	{
		throw new NotImplementedException();
	}

	public void SetHandler(object pUnk)
	{
		throw new NotImplementedException();
	}

	public uint DefineMethod(uint td, IntPtr zName, uint dwMethodFlags, IntPtr pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags)
	{
		throw new NotImplementedException();
	}

	public void DefineMethodImpl(uint td, uint tkBody, uint tkDecl)
	{
		throw new NotImplementedException();
	}

	public uint DefineTypeRefByName(uint tkResolutionScope, IntPtr szName)
	{
		throw new NotImplementedException();
	}

	public uint DefineImportType(IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint tdImport, IntPtr pAssemEmit)
	{
		throw new NotImplementedException();
	}

	public uint DefineMemberRef(uint tkImport, string szName, IntPtr pvSigBlob, uint cbSigBlob)
	{
		throw new NotImplementedException();
	}

	public uint DefineImportMember(IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent)
	{
		throw new NotImplementedException();
	}

	public uint DefineEvent(uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, IntPtr rmdOtherMethods)
	{
		throw new NotImplementedException();
	}

	public void SetClassLayout(uint td, uint dwPackSize, IntPtr rFieldOffsets, uint ulClassSize)
	{
		throw new NotImplementedException();
	}

	public void DeleteClassLayout(uint td)
	{
		throw new NotImplementedException();
	}

	public void SetFieldMarshal(uint tk, IntPtr pvNativeType, uint cbNativeType)
	{
		throw new NotImplementedException();
	}

	public void DeleteFieldMarshal(uint tk)
	{
		throw new NotImplementedException();
	}

	public uint DefinePermissionSet(uint tk, uint dwAction, IntPtr pvPermission, uint cbPermission)
	{
		throw new NotImplementedException();
	}

	public void SetRVA(uint md, uint ulRVA)
	{
		throw new NotImplementedException();
	}

	public uint GetTokenFromSig(IntPtr pvSig, uint cbSig)
	{
		throw new NotImplementedException();
	}

	public uint DefineModuleRef(string szName)
	{
		throw new NotImplementedException();
	}

	public void SetParent(uint mr, uint tk)
	{
		throw new NotImplementedException();
	}

	public uint GetTokenFromTypeSpec(IntPtr pvSig, uint cbSig)
	{
		throw new NotImplementedException();
	}

	public void SaveToMemory(IntPtr pbData, uint cbData)
	{
		throw new NotImplementedException();
	}

	public uint DefineUserString(string szString, uint cchString)
	{
		throw new NotImplementedException();
	}

	public void DeleteToken(uint tkObj)
	{
		throw new NotImplementedException();
	}

	public void SetMethodProps(uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags)
	{
		throw new NotImplementedException();
	}

	public void SetTypeDefProps(uint td, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements)
	{
		throw new NotImplementedException();
	}

	public void SetEventProps(uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, IntPtr rmdOtherMethods)
	{
		throw new NotImplementedException();
	}

	public uint SetPermissionSetProps(uint tk, uint dwAction, IntPtr pvPermission, uint cbPermission)
	{
		throw new NotImplementedException();
	}

	public void DefinePinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL)
	{
		throw new NotImplementedException();
	}

	public void SetPinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL)
	{
		throw new NotImplementedException();
	}

	public void DeletePinvokeMap(uint tk)
	{
		throw new NotImplementedException();
	}

	public uint DefineCustomAttribute(uint tkObj, uint tkType, IntPtr pCustomAttribute, uint cbCustomAttribute)
	{
		throw new NotImplementedException();
	}

	public void SetCustomAttributeValue(uint pcv, IntPtr pCustomAttribute, uint cbCustomAttribute)
	{
		throw new NotImplementedException();
	}

	public uint DefineField(uint td, string szName, uint dwFieldFlags, IntPtr pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue)
	{
		throw new NotImplementedException();
	}

	public uint DefineProperty(uint td, string szProperty, uint dwPropFlags, IntPtr pvSig, uint cbSig, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue, uint mdSetter, uint mdGetter, IntPtr rmdOtherMethods)
	{
		throw new NotImplementedException();
	}

	public uint DefineParam(uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue)
	{
		throw new NotImplementedException();
	}

	public void SetFieldProps(uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue)
	{
		throw new NotImplementedException();
	}

	public void SetPropertyProps(uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue, uint mdSetter, uint mdGetter, IntPtr rmdOtherMethods)
	{
		throw new NotImplementedException();
	}

	public void SetParamProps(uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue)
	{
		throw new NotImplementedException();
	}

	public uint DefineSecurityAttributeSet(uint tkObj, IntPtr rSecAttrs, uint cSecAttrs)
	{
		throw new NotImplementedException();
	}

	public void ApplyEditAndContinue(object pImport)
	{
		throw new NotImplementedException();
	}

	public uint TranslateSigWithScope(IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport import, IntPtr pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, IntPtr pvTranslatedSig, uint cbTranslatedSigMax)
	{
		throw new NotImplementedException();
	}

	public void SetMethodImplFlags(uint md, uint dwImplFlags)
	{
		throw new NotImplementedException();
	}

	public void SetFieldRVA(uint fd, uint ulRVA)
	{
		throw new NotImplementedException();
	}

	public void Merge(IMetaDataImport pImport, IntPtr pHostMapToken, object pHandler)
	{
		throw new NotImplementedException();
	}

	public void MergeEnd()
	{
		throw new NotImplementedException();
	}

	public void CloseEnum(uint hEnum)
	{
		throw new NotImplementedException();
	}

	public uint CountEnum(uint hEnum)
	{
		throw new NotImplementedException();
	}

	public void ResetEnum(uint hEnum, uint ulPos)
	{
		throw new NotImplementedException();
	}

	public uint EnumTypeDefs(ref uint phEnum, uint[] rTypeDefs, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumInterfaceImpls(ref uint phEnum, uint td, uint[] rImpls, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumTypeRefs(ref uint phEnum, uint[] rTypeRefs, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint FindTypeDefByName(string szTypeDef, uint tkEnclosingClass)
	{
		throw new NotImplementedException();
	}

	public Guid GetScopeProps(StringBuilder szName, uint cchName, out uint pchName)
	{
		throw new NotImplementedException();
	}

	public uint GetModuleFromScope()
	{
		throw new NotImplementedException();
	}

	public unsafe uint GetTypeDefProps(uint td, char* szTypeDef, uint cchTypeDef, uint* pchTypeDef, uint* pdwTypeDefFlags, uint* ptkExtends)
	{
		if (!TryGetType(td, out var type))
		{
			return 2147500037u;
		}
		WriteNameBuffer(type.IsNested ? type.Name : type.FullName, szTypeDef, cchTypeDef, pchTypeDef);
		if (pdwTypeDefFlags != null)
		{
			*pdwTypeDefFlags = (uint)type.Attributes;
		}
		if (ptkExtends != null)
		{
			*ptkExtends = ((type.BaseType != null) ? type.BaseType.MetadataToken.ToUInt32() : 0u);
		}
		return 0u;
	}

	public uint GetInterfaceImplProps(uint iiImpl, out uint pClass)
	{
		throw new NotImplementedException();
	}

	public uint GetTypeRefProps(uint tr, out uint ptkResolutionScope, StringBuilder szName, uint cchName)
	{
		throw new NotImplementedException();
	}

	public uint ResolveTypeRef(uint tr, ref Guid riid, out object ppIScope)
	{
		throw new NotImplementedException();
	}

	public uint EnumMembers(ref uint phEnum, uint cl, uint[] rMembers, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumMembersWithName(ref uint phEnum, uint cl, string szName, uint[] rMembers, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumMethods(ref uint phEnum, uint cl, IntPtr rMethods, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumMethodsWithName(ref uint phEnum, uint cl, string szName, uint[] rMethods, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumFields(ref uint phEnum, uint cl, IntPtr rFields, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumFieldsWithName(ref uint phEnum, uint cl, string szName, uint[] rFields, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumParams(ref uint phEnum, uint mb, uint[] rParams, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumMemberRefs(ref uint phEnum, uint tkParent, uint[] rMemberRefs, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumMethodImpls(ref uint phEnum, uint td, uint[] rMethodBody, uint[] rMethodDecl, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumPermissionSets(ref uint phEnum, uint tk, uint dwActions, uint[] rPermission, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint FindMember(uint td, string szName, byte[] pvSigBlob, uint cbSigBlob)
	{
		throw new NotImplementedException();
	}

	public uint FindMethod(uint td, string szName, byte[] pvSigBlob, uint cbSigBlob)
	{
		throw new NotImplementedException();
	}

	public uint FindField(uint td, string szName, byte[] pvSigBlob, uint cbSigBlob)
	{
		throw new NotImplementedException();
	}

	public uint FindMemberRef(uint td, string szName, byte[] pvSigBlob, uint cbSigBlob)
	{
		throw new NotImplementedException();
	}

	public unsafe uint GetMethodProps(uint mb, uint* pClass, char* szMethod, uint cchMethod, uint* pchMethod, uint* pdwAttr, IntPtr ppvSigBlob, IntPtr pcbSigBlob, uint* pulCodeRVA, uint* pdwImplFlags)
	{
		if (!TryGetMethod(mb, out var method))
		{
			return 2147500037u;
		}
		if (pClass != null)
		{
			*pClass = method.DeclaringType.MetadataToken.ToUInt32();
		}
		WriteNameBuffer(method.Name, szMethod, cchMethod, pchMethod);
		if (pdwAttr != null)
		{
			*pdwAttr = (uint)method.Attributes;
		}
		if (pulCodeRVA != null)
		{
			*pulCodeRVA = (uint)method.RVA;
		}
		if (pdwImplFlags != null)
		{
			*pdwImplFlags = (uint)method.ImplAttributes;
		}
		return 0u;
	}

	private unsafe static void WriteNameBuffer(string name, char* buffer, uint bufferLength, uint* actualLength)
	{
		long num = System.Math.Min(name.Length, bufferLength - 1);
		if (actualLength != null)
		{
			*actualLength = (uint)num;
		}
		if (buffer != null && bufferLength != 0)
		{
			for (int i = 0; i < num; i++)
			{
				buffer[i] = name[i];
			}
			buffer[num + 1] = '\0';
		}
	}

	public uint GetMemberRefProps(uint mr, ref uint ptk, StringBuilder szMember, uint cchMember, out uint pchMember, out IntPtr ppvSigBlob)
	{
		throw new NotImplementedException();
	}

	public uint EnumProperties(ref uint phEnum, uint td, IntPtr rProperties, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumEvents(ref uint phEnum, uint td, IntPtr rEvents, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint GetEventProps(uint ev, out uint pClass, StringBuilder szEvent, uint cchEvent, out uint pchEvent, out uint pdwEventFlags, out uint ptkEventType, out uint pmdAddOn, out uint pmdRemoveOn, out uint pmdFire, uint[] rmdOtherMethod, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint EnumMethodSemantics(ref uint phEnum, uint mb, uint[] rEventProp, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint GetMethodSemantics(uint mb, uint tkEventProp)
	{
		throw new NotImplementedException();
	}

	public uint GetClassLayout(uint td, out uint pdwPackSize, IntPtr rFieldOffset, uint cMax, out uint pcFieldOffset)
	{
		throw new NotImplementedException();
	}

	public uint GetFieldMarshal(uint tk, out IntPtr ppvNativeType)
	{
		throw new NotImplementedException();
	}

	public uint GetRVA(uint tk, out uint pulCodeRVA)
	{
		throw new NotImplementedException();
	}

	public uint GetPermissionSetProps(uint pm, out uint pdwAction, out IntPtr ppvPermission)
	{
		throw new NotImplementedException();
	}

	public uint GetSigFromToken(uint mdSig, out IntPtr ppvSig)
	{
		throw new NotImplementedException();
	}

	public uint GetModuleRefProps(uint mur, StringBuilder szName, uint cchName)
	{
		throw new NotImplementedException();
	}

	public uint EnumModuleRefs(ref uint phEnum, uint[] rModuleRefs, uint cmax)
	{
		throw new NotImplementedException();
	}

	public uint GetTypeSpecFromToken(uint typespec, out IntPtr ppvSig)
	{
		throw new NotImplementedException();
	}

	public uint GetNameFromToken(uint tk)
	{
		throw new NotImplementedException();
	}

	public uint EnumUnresolvedMethods(ref uint phEnum, uint[] rMethods, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint GetUserString(uint stk, StringBuilder szString, uint cchString)
	{
		throw new NotImplementedException();
	}

	public uint GetPinvokeMap(uint tk, out uint pdwMappingFlags, StringBuilder szImportName, uint cchImportName, out uint pchImportName)
	{
		throw new NotImplementedException();
	}

	public uint EnumSignatures(ref uint phEnum, uint[] rSignatures, uint cmax)
	{
		throw new NotImplementedException();
	}

	public uint EnumTypeSpecs(ref uint phEnum, uint[] rTypeSpecs, uint cmax)
	{
		throw new NotImplementedException();
	}

	public uint EnumUserStrings(ref uint phEnum, uint[] rStrings, uint cmax)
	{
		throw new NotImplementedException();
	}

	public int GetParamForMethodIndex(uint md, uint ulParamSeq, out uint pParam)
	{
		throw new NotImplementedException();
	}

	public uint EnumCustomAttributes(ref uint phEnum, uint tk, uint tkType, uint[] rCustomAttributes, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint GetCustomAttributeProps(uint cv, out uint ptkObj, out uint ptkType, out IntPtr ppBlob)
	{
		throw new NotImplementedException();
	}

	public uint FindTypeRef(uint tkResolutionScope, string szName)
	{
		throw new NotImplementedException();
	}

	public uint GetMemberProps(uint mb, out uint pClass, StringBuilder szMember, uint cchMember, out uint pchMember, out uint pdwAttr, out IntPtr ppvSigBlob, out uint pcbSigBlob, out uint pulCodeRVA, out uint pdwImplFlags, out uint pdwCPlusTypeFlag, out IntPtr ppValue)
	{
		throw new NotImplementedException();
	}

	public uint GetFieldProps(uint mb, out uint pClass, StringBuilder szField, uint cchField, out uint pchField, out uint pdwAttr, out IntPtr ppvSigBlob, out uint pcbSigBlob, out uint pdwCPlusTypeFlag, out IntPtr ppValue)
	{
		throw new NotImplementedException();
	}

	public uint GetPropertyProps(uint prop, out uint pClass, StringBuilder szProperty, uint cchProperty, out uint pchProperty, out uint pdwPropFlags, out IntPtr ppvSig, out uint pbSig, out uint pdwCPlusTypeFlag, out IntPtr ppDefaultValue, out uint pcchDefaultValue, out uint pmdSetter, out uint pmdGetter, uint[] rmdOtherMethod, uint cMax)
	{
		throw new NotImplementedException();
	}

	public uint GetParamProps(uint tk, out uint pmd, out uint pulSequence, StringBuilder szName, uint cchName, out uint pchName, out uint pdwAttr, out uint pdwCPlusTypeFlag, out IntPtr ppValue)
	{
		throw new NotImplementedException();
	}

	public uint GetCustomAttributeByName(uint tkObj, string szName, out IntPtr ppData)
	{
		throw new NotImplementedException();
	}

	public bool IsValidToken(uint tk)
	{
		throw new NotImplementedException();
	}

	public unsafe uint GetNestedClassProps(uint tdNestedClass, uint* ptdEnclosingClass)
	{
		if (!TryGetType(tdNestedClass, out var type))
		{
			return 2147500037u;
		}
		if (ptdEnclosingClass != null)
		{
			*ptdEnclosingClass = (type.IsNested ? type.DeclaringType.MetadataToken.ToUInt32() : 0u);
		}
		return 0u;
	}

	public uint GetNativeCallConvFromSig(IntPtr pvSig, uint cbSig)
	{
		throw new NotImplementedException();
	}

	public int IsGlobal(uint pd)
	{
		throw new NotImplementedException();
	}
}
