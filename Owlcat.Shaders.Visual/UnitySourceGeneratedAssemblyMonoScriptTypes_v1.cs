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
		result.FilePathsData = new byte[74]
		{
			0, 0, 0, 1, 0, 0, 0, 66, 92, 76,
			105, 98, 114, 97, 114, 121, 92, 80, 97, 99,
			107, 97, 103, 101, 67, 97, 99, 104, 101, 92,
			99, 111, 109, 46, 111, 119, 108, 99, 97, 116,
			46, 118, 105, 115, 117, 97, 108, 64, 48, 46,
			49, 46, 50, 49, 54, 92, 83, 104, 97, 100,
			101, 114, 115, 92, 83, 104, 97, 100, 101, 114,
			115, 46, 99, 115
		};
		result.TypesData = new byte[39]
		{
			0, 0, 0, 0, 34, 79, 119, 108, 99, 97,
			116, 46, 83, 104, 97, 100, 101, 114, 115, 46,
			86, 105, 115, 117, 97, 108, 124, 83, 104, 97,
			100, 101, 114, 115, 68, 117, 109, 109, 121
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
