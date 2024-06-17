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
		result.FilePathsData = new byte[57]
		{
			0, 0, 0, 1, 0, 0, 0, 49, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 82, 117, 108, 101, 83, 121, 115, 116, 101,
			109, 92, 69, 110, 117, 109, 115, 92, 82, 117,
			108, 101, 98, 111, 111, 107, 83, 101, 116, 116,
			105, 110, 103, 115, 46, 99, 115
		};
		result.TypesData = new byte[50]
		{
			0, 0, 0, 0, 45, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 82, 117, 108, 101, 83,
			121, 115, 116, 101, 109, 46, 69, 110, 117, 109,
			124, 65, 116, 116, 97, 99, 107, 84, 121, 112,
			101, 69, 120, 116, 101, 110, 115, 105, 111, 110
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
