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
			0, 0, 0, 1, 0, 0, 0, 45, 92, 65,
			115, 115, 101, 116, 115, 92, 80, 108, 117, 103,
			105, 110, 115, 92, 67, 111, 114, 101, 46, 82,
			101, 115, 116, 83, 101, 114, 118, 101, 114, 92,
			82, 101, 115, 116, 83, 101, 114, 118, 101, 114,
			46, 99, 115
		};
		result.TypesData = new byte[31]
		{
			0, 0, 0, 0, 26, 67, 111, 114, 101, 46,
			82, 101, 115, 116, 83, 101, 114, 118, 101, 114,
			124, 82, 101, 115, 116, 83, 101, 114, 118, 101,
			114
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
