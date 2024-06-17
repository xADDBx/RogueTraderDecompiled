using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.CompilerServices;

[CompilerGenerated]
[EditorBrowsable(EditorBrowsableState.Never)]
[GeneratedCode("Unity.MonoScriptGenerator.MonoScriptInfoGenerator", null)]
internal class UnitySourceGeneratedAssemblyMonoScriptTypes_v1
{
	private struct MonoScriptData
	{
		public byte[] FilePathsData;

		public byte[] TypesData;

		public int TotalTypes;

		public int TotalFiles;

		public bool IsEditorOnly;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static MonoScriptData Get()
	{
		MonoScriptData result = default(MonoScriptData);
		result.FilePathsData = new byte[68]
		{
			0, 0, 0, 1, 0, 0, 0, 60, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 86, 105, 115, 117, 97, 108, 92, 72, 105,
			116, 83, 121, 115, 116, 101, 109, 92, 66, 97,
			115, 101, 84, 121, 112, 101, 115, 92, 68, 97,
			109, 97, 103, 101, 72, 105, 116, 83, 101, 116,
			116, 105, 110, 103, 115, 46, 99, 115
		};
		result.TypesData = new byte[49]
		{
			0, 0, 0, 0, 44, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 86, 105, 115, 117, 97,
			108, 46, 72, 105, 116, 83, 121, 115, 116, 101,
			109, 124, 68, 97, 109, 97, 103, 101, 72, 105,
			116, 83, 101, 116, 116, 105, 110, 103, 115
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
