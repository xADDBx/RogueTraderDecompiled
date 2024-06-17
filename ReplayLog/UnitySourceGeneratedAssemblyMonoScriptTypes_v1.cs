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
		result.FilePathsData = new byte[51]
		{
			0, 0, 0, 1, 0, 0, 0, 43, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 85, 116, 105, 108, 105, 116, 121, 92, 82,
			101, 112, 108, 97, 121, 76, 111, 103, 92, 82,
			101, 112, 108, 97, 121, 76, 111, 103, 46, 99,
			115
		};
		result.TypesData = new byte[42]
		{
			0, 0, 0, 0, 37, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 85, 116, 105, 108, 105,
			116, 121, 46, 82, 101, 112, 108, 97, 121, 76,
			111, 103, 124, 82, 101, 112, 108, 97, 121, 76,
			111, 103
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
