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
		result.FilePathsData = new byte[73]
		{
			0, 0, 0, 1, 0, 0, 0, 65, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 85, 116, 105, 108, 105, 116, 121, 92, 67,
			111, 109, 109, 97, 110, 100, 76, 105, 110, 101,
			65, 114, 103, 117, 109, 101, 110, 116, 115, 92,
			67, 111, 109, 109, 97, 110, 100, 76, 105, 110,
			101, 65, 114, 103, 117, 109, 101, 110, 116, 115,
			46, 99, 115
		};
		result.TypesData = new byte[59]
		{
			0, 0, 0, 0, 54, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 85, 116, 105, 108, 105,
			116, 121, 46, 67, 111, 109, 109, 97, 110, 100,
			76, 105, 110, 101, 65, 114, 103, 115, 124, 67,
			111, 109, 109, 97, 110, 100, 76, 105, 110, 101,
			65, 114, 103, 117, 109, 101, 110, 116, 115
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
