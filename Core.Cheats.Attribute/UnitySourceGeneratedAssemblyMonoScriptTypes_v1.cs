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
		result.FilePathsData = new byte[63]
		{
			0, 0, 0, 2, 0, 0, 0, 55, 92, 65,
			115, 115, 101, 116, 115, 92, 80, 108, 117, 103,
			105, 110, 115, 92, 67, 111, 114, 101, 46, 67,
			104, 101, 97, 116, 115, 92, 65, 116, 116, 114,
			105, 98, 117, 116, 101, 92, 67, 104, 101, 97,
			116, 65, 116, 116, 114, 105, 98, 117, 116, 101,
			46, 99, 115
		};
		result.TypesData = new byte[69]
		{
			0, 0, 0, 0, 33, 67, 111, 114, 101, 46,
			67, 104, 101, 97, 116, 115, 124, 69, 120, 101,
			99, 117, 116, 105, 111, 110, 80, 111, 108, 105,
			99, 121, 72, 101, 108, 112, 101, 114, 0, 0,
			0, 0, 26, 67, 111, 114, 101, 46, 67, 104,
			101, 97, 116, 115, 124, 67, 104, 101, 97, 116,
			65, 116, 116, 114, 105, 98, 117, 116, 101
		};
		result.TotalFiles = 1;
		result.TotalTypes = 2;
		result.IsEditorOnly = false;
		return result;
	}
}
