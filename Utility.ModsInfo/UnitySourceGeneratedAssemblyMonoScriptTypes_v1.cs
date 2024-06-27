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
		result.FilePathsData = new byte[53]
		{
			0, 0, 0, 2, 0, 0, 0, 45, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 85, 116, 105, 108, 105, 116, 121, 92, 77,
			111, 100, 115, 73, 110, 102, 111, 92, 85, 115,
			101, 114, 77, 111, 100, 115, 68, 97, 116, 97,
			46, 99, 115
		};
		result.TypesData = new byte[83]
		{
			0, 0, 0, 0, 34, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 85, 116, 105, 108, 105,
			116, 121, 46, 77, 111, 100, 115, 73, 110, 102,
			111, 124, 77, 111, 100, 73, 110, 102, 111, 0,
			0, 0, 0, 39, 75, 105, 110, 103, 109, 97,
			107, 101, 114, 46, 85, 116, 105, 108, 105, 116,
			121, 46, 77, 111, 100, 115, 73, 110, 102, 111,
			124, 85, 115, 101, 114, 77, 111, 100, 115, 68,
			97, 116, 97
		};
		result.TotalFiles = 1;
		result.TotalTypes = 2;
		result.IsEditorOnly = false;
		return result;
	}
}
