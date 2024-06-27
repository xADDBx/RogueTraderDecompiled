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
			0, 0, 0, 1, 0, 0, 0, 55, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 85, 116, 105, 108, 105, 116, 121, 92, 69,
			120, 116, 101, 110, 100, 101, 100, 77, 111, 100,
			73, 110, 102, 111, 92, 69, 120, 116, 101, 110,
			100, 101, 100, 77, 111, 100, 73, 110, 102, 111,
			46, 99, 115
		};
		result.TypesData = new byte[49]
		{
			0, 0, 0, 0, 44, 67, 111, 100, 101, 46,
			85, 116, 105, 108, 105, 116, 121, 46, 69, 120,
			116, 101, 110, 100, 101, 100, 77, 111, 100, 73,
			110, 102, 111, 124, 69, 120, 116, 101, 110, 100,
			101, 100, 77, 111, 100, 73, 110, 102, 111
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
