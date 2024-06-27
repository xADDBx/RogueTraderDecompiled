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
		result.FilePathsData = new byte[45]
		{
			0, 0, 0, 2, 0, 0, 0, 37, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 85, 116, 105, 108, 105, 116, 121, 92, 82,
			111, 117, 110, 100, 115, 92, 82, 111, 117, 110,
			100, 115, 46, 99, 115
		};
		result.TypesData = new byte[67]
		{
			0, 0, 0, 0, 24, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 85, 116, 105, 108, 105,
			116, 121, 124, 82, 111, 117, 110, 100, 115, 0,
			0, 0, 0, 33, 75, 105, 110, 103, 109, 97,
			107, 101, 114, 46, 85, 116, 105, 108, 105, 116,
			121, 124, 82, 111, 117, 110, 100, 115, 69, 120,
			116, 101, 110, 115, 105, 111, 110
		};
		result.TotalFiles = 1;
		result.TotalTypes = 2;
		result.IsEditorOnly = false;
		return result;
	}
}
