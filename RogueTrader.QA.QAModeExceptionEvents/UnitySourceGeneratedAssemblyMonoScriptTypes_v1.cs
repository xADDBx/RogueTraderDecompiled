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
		result.FilePathsData = new byte[84]
		{
			0, 0, 0, 2, 0, 0, 0, 76, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 81, 65, 92, 81, 65, 77, 111, 100, 101,
			69, 120, 99, 101, 112, 116, 105, 111, 110, 82,
			101, 112, 111, 114, 116, 101, 114, 92, 81, 65,
			77, 111, 100, 101, 69, 120, 99, 101, 112, 116,
			105, 111, 110, 69, 118, 101, 110, 116, 115, 92,
			76, 111, 103, 67, 104, 97, 110, 101, 108, 69,
			120, 46, 99, 115
		};
		result.TypesData = new byte[69]
		{
			0, 0, 0, 0, 25, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 81, 65, 124, 76, 111,
			103, 67, 104, 97, 110, 110, 101, 108, 69, 120,
			0, 0, 0, 0, 34, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 81, 65, 124, 81, 65,
			77, 111, 100, 101, 69, 120, 99, 101, 112, 116,
			105, 111, 110, 69, 118, 101, 110, 116, 115
		};
		result.TotalFiles = 1;
		result.TotalTypes = 2;
		result.IsEditorOnly = false;
		return result;
	}
}
