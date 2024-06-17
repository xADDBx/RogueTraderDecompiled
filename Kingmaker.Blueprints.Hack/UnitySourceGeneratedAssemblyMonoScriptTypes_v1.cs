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
		result.FilePathsData = new byte[66]
		{
			0, 0, 0, 1, 0, 0, 0, 58, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 66, 108, 117, 101, 112, 114, 105, 110, 116,
			115, 92, 72, 97, 99, 107, 92, 73, 66, 108,
			117, 101, 112, 114, 105, 110, 116, 83, 99, 114,
			105, 112, 116, 97, 98, 108, 101, 79, 98, 106,
			101, 99, 116, 46, 99, 115
		};
		result.TypesData = new byte[57]
		{
			0, 0, 0, 0, 52, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 66, 108, 117, 101, 112,
			114, 105, 110, 116, 115, 46, 72, 97, 99, 107,
			124, 73, 66, 108, 117, 101, 112, 114, 105, 110,
			116, 83, 99, 114, 105, 112, 116, 97, 98, 108,
			101, 79, 98, 106, 101, 99, 116
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
