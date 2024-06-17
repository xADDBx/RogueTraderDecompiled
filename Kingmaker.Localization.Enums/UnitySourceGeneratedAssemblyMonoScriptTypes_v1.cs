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
		result.FilePathsData = new byte[64]
		{
			0, 0, 0, 1, 0, 0, 0, 56, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 71, 97, 109, 101, 67, 111, 114, 101, 92,
			76, 111, 99, 97, 108, 105, 122, 97, 116, 105,
			111, 110, 92, 83, 104, 97, 114, 101, 100, 92,
			69, 110, 117, 109, 92, 76, 111, 99, 97, 108,
			101, 46, 99, 115
		};
		result.TypesData = new byte[50]
		{
			0, 0, 0, 0, 45, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 76, 111, 99, 97, 108,
			105, 122, 97, 116, 105, 111, 110, 46, 69, 110,
			117, 109, 115, 124, 76, 111, 99, 97, 108, 101,
			69, 120, 116, 101, 110, 115, 105, 111, 110, 115
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
