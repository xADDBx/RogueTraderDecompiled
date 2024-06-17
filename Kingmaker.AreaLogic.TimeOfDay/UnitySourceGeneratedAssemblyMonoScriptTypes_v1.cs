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
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 65, 114, 101, 97, 76, 111, 103, 105, 99,
			92, 84, 105, 109, 101, 79, 102, 68, 97, 121,
			92, 84, 105, 109, 101, 79, 102, 68, 97, 121,
			46, 99, 115
		};
		result.TypesData = new byte[50]
		{
			0, 0, 0, 0, 45, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 65, 114, 101, 97, 76,
			111, 103, 105, 99, 46, 84, 105, 109, 101, 79,
			102, 68, 97, 121, 124, 84, 105, 109, 101, 79,
			102, 68, 97, 121, 72, 101, 108, 112, 101, 114
		};
		result.TotalFiles = 1;
		result.TotalTypes = 1;
		result.IsEditorOnly = false;
		return result;
	}
}
