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
		result.FilePathsData = new byte[48]
		{
			0, 0, 0, 1, 0, 0, 0, 40, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 83, 104, 97, 100, 101, 114, 67, 111, 110,
			115, 116, 115, 92, 83, 104, 97, 100, 101, 114,
			80, 114, 111, 112, 115, 46, 99, 115
		};
		result.TypesData = new byte[46]
		{
			0, 0, 0, 0, 41, 82, 111, 103, 117, 101,
			84, 114, 97, 100, 101, 114, 46, 67, 111, 100,
			101, 46, 83, 104, 97, 100, 101, 114, 67, 111,
			110, 115, 116, 115, 124, 83, 104, 97, 100, 101,
			114, 80, 114, 111, 112, 115
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
