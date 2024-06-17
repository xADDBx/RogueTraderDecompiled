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
		result.FilePathsData = new byte[72]
		{
			0, 0, 0, 1, 0, 0, 0, 64, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 71, 97, 109, 101, 67, 111, 114, 101, 92,
			69, 108, 101, 109, 101, 110, 116, 115, 68, 101,
			115, 99, 114, 105, 112, 116, 105, 111, 110, 92,
			69, 108, 101, 109, 101, 110, 116, 115, 68, 101,
			115, 99, 114, 105, 112, 116, 105, 111, 110, 46,
			99, 115
		};
		result.TypesData = new byte[49]
		{
			0, 0, 0, 0, 44, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 69, 108, 101, 109, 101,
			110, 116, 115, 83, 121, 115, 116, 101, 109, 124,
			69, 108, 101, 109, 101, 110, 116, 115, 68, 101,
			115, 99, 114, 105, 112, 116, 105, 111, 110
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
