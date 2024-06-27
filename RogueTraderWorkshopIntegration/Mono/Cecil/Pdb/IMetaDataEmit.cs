using System;
using System.Runtime.InteropServices;

namespace Mono.Cecil.Pdb;

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("BA3FEE4C-ECB9-4e41-83B7-183FA41CD859")]
internal interface IMetaDataEmit
{
	void SetModuleProps(string szName);

	void Save(string szFile, uint dwSaveFlags);

	void SaveToStream(IntPtr pIStream, uint dwSaveFlags);

	uint GetSaveSize(uint fSave);

	uint DefineTypeDef(IntPtr szTypeDef, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements);

	uint DefineNestedType(IntPtr szTypeDef, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements, uint tdEncloser);

	void SetHandler([In][MarshalAs(UnmanagedType.IUnknown)] object pUnk);

	uint DefineMethod(uint td, IntPtr zName, uint dwMethodFlags, IntPtr pvSigBlob, uint cbSigBlob, uint ulCodeRVA, uint dwImplFlags);

	void DefineMethodImpl(uint td, uint tkBody, uint tkDecl);

	uint DefineTypeRefByName(uint tkResolutionScope, IntPtr szName);

	uint DefineImportType(IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint tdImport, IntPtr pAssemEmit);

	uint DefineMemberRef(uint tkImport, string szName, IntPtr pvSigBlob, uint cbSigBlob);

	uint DefineImportMember(IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport pImport, uint mbMember, IntPtr pAssemEmit, uint tkParent);

	uint DefineEvent(uint td, string szEvent, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, IntPtr rmdOtherMethods);

	void SetClassLayout(uint td, uint dwPackSize, IntPtr rFieldOffsets, uint ulClassSize);

	void DeleteClassLayout(uint td);

	void SetFieldMarshal(uint tk, IntPtr pvNativeType, uint cbNativeType);

	void DeleteFieldMarshal(uint tk);

	uint DefinePermissionSet(uint tk, uint dwAction, IntPtr pvPermission, uint cbPermission);

	void SetRVA(uint md, uint ulRVA);

	uint GetTokenFromSig(IntPtr pvSig, uint cbSig);

	uint DefineModuleRef(string szName);

	void SetParent(uint mr, uint tk);

	uint GetTokenFromTypeSpec(IntPtr pvSig, uint cbSig);

	void SaveToMemory(IntPtr pbData, uint cbData);

	uint DefineUserString(string szString, uint cchString);

	void DeleteToken(uint tkObj);

	void SetMethodProps(uint md, uint dwMethodFlags, uint ulCodeRVA, uint dwImplFlags);

	void SetTypeDefProps(uint td, uint dwTypeDefFlags, uint tkExtends, IntPtr rtkImplements);

	void SetEventProps(uint ev, uint dwEventFlags, uint tkEventType, uint mdAddOn, uint mdRemoveOn, uint mdFire, IntPtr rmdOtherMethods);

	uint SetPermissionSetProps(uint tk, uint dwAction, IntPtr pvPermission, uint cbPermission);

	void DefinePinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL);

	void SetPinvokeMap(uint tk, uint dwMappingFlags, string szImportName, uint mrImportDLL);

	void DeletePinvokeMap(uint tk);

	uint DefineCustomAttribute(uint tkObj, uint tkType, IntPtr pCustomAttribute, uint cbCustomAttribute);

	void SetCustomAttributeValue(uint pcv, IntPtr pCustomAttribute, uint cbCustomAttribute);

	uint DefineField(uint td, string szName, uint dwFieldFlags, IntPtr pvSigBlob, uint cbSigBlob, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue);

	uint DefineProperty(uint td, string szProperty, uint dwPropFlags, IntPtr pvSig, uint cbSig, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue, uint mdSetter, uint mdGetter, IntPtr rmdOtherMethods);

	uint DefineParam(uint md, uint ulParamSeq, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue);

	void SetFieldProps(uint fd, uint dwFieldFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue);

	void SetPropertyProps(uint pr, uint dwPropFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue, uint mdSetter, uint mdGetter, IntPtr rmdOtherMethods);

	void SetParamProps(uint pd, string szName, uint dwParamFlags, uint dwCPlusTypeFlag, IntPtr pValue, uint cchValue);

	uint DefineSecurityAttributeSet(uint tkObj, IntPtr rSecAttrs, uint cSecAttrs);

	void ApplyEditAndContinue([MarshalAs(UnmanagedType.IUnknown)] object pImport);

	uint TranslateSigWithScope(IntPtr pAssemImport, IntPtr pbHashValue, uint cbHashValue, IMetaDataImport import, IntPtr pbSigBlob, uint cbSigBlob, IntPtr pAssemEmit, IMetaDataEmit emit, IntPtr pvTranslatedSig, uint cbTranslatedSigMax);

	void SetMethodImplFlags(uint md, uint dwImplFlags);

	void SetFieldRVA(uint fd, uint ulRVA);

	void Merge(IMetaDataImport pImport, IntPtr pHostMapToken, [MarshalAs(UnmanagedType.IUnknown)] object pHandler);

	void MergeEnd();
}
