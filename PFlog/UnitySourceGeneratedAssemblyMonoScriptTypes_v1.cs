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
		result.FilePathsData = new byte[43]
		{
			0, 0, 0, 2, 0, 0, 0, 35, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 76, 111, 103, 103, 105, 110, 103, 92, 80,
			70, 76, 111, 103, 92, 80, 70, 76, 111, 103,
			46, 99, 115
		};
		result.TypesData = new byte[48]
		{
			0, 0, 0, 0, 15, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 124, 80, 70, 76, 111, 103,
			0, 0, 0, 0, 23, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 80, 70, 76, 111, 103,
			124, 72, 105, 115, 116, 111, 114, 121
		};
		result.TotalFiles = 1;
		result.TotalTypes = 2;
		result.IsEditorOnly = false;
		return result;
	}
}
