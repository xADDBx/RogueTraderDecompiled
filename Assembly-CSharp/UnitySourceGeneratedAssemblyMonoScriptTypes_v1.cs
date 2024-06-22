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
		result.FilePathsData = new byte[77]
		{
			0, 0, 0, 1, 0, 0, 0, 28, 92, 65,
			115, 115, 101, 116, 115, 92, 83, 111, 117, 110,
			100, 92, 83, 111, 117, 110, 100, 83, 112, 108,
			105, 110, 101, 46, 99, 115, 0, 0, 0, 1,
			0, 0, 0, 33, 92, 65, 115, 115, 101, 116,
			115, 92, 83, 111, 117, 110, 100, 92, 83, 111,
			117, 110, 100, 83, 112, 108, 105, 110, 101, 77,
			111, 118, 101, 114, 46, 99, 115
		};
		result.TypesData = new byte[39]
		{
			0, 0, 0, 0, 12, 124, 83, 111, 117, 110,
			100, 83, 112, 108, 105, 110, 101, 0, 0, 0,
			0, 17, 124, 83, 111, 117, 110, 100, 83, 112,
			108, 105, 110, 101, 77, 111, 118, 101, 114
		};
		result.TotalFiles = 2;
		result.TotalTypes = 2;
		result.IsEditorOnly = false;
		return result;
	}
}
