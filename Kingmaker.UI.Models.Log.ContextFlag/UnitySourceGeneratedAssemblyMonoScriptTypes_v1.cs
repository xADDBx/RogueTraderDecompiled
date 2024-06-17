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
		result.FilePathsData = new byte[65]
		{
			0, 0, 0, 1, 0, 0, 0, 57, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 85, 73, 92, 77, 111, 100, 101, 108, 115,
			92, 76, 111, 103, 92, 67, 111, 110, 116, 101,
			120, 116, 70, 108, 97, 103, 92, 71, 97, 109,
			101, 76, 111, 103, 68, 105, 115, 97, 98, 108,
			101, 100, 46, 99, 115
		};
		result.TypesData = new byte[56]
		{
			0, 0, 0, 0, 51, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 85, 73, 46, 77, 111,
			100, 101, 108, 115, 46, 76, 111, 103, 46, 67,
			111, 110, 116, 101, 120, 116, 70, 108, 97, 103,
			124, 71, 97, 109, 101, 76, 111, 103, 68, 105,
			115, 97, 98, 108, 101, 100
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
