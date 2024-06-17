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
		result.FilePathsData = new byte[280]
		{
			0, 0, 0, 1, 0, 0, 0, 56, 92, 65,
			115, 115, 101, 116, 115, 92, 67, 111, 100, 101,
			92, 71, 97, 109, 101, 67, 111, 114, 101, 92,
			76, 111, 99, 97, 108, 105, 122, 97, 116, 105,
			111, 110, 92, 83, 104, 97, 114, 101, 100, 92,
			76, 111, 99, 97, 108, 101, 84, 114, 97, 105,
			116, 46, 99, 115, 0, 0, 0, 2, 0, 0,
			0, 67, 92, 65, 115, 115, 101, 116, 115, 92,
			67, 111, 100, 101, 92, 71, 97, 109, 101, 67,
			111, 114, 101, 92, 76, 111, 99, 97, 108, 105,
			122, 97, 116, 105, 111, 110, 92, 83, 104, 97,
			114, 101, 100, 92, 76, 111, 99, 97, 108, 105,
			122, 97, 116, 105, 111, 110, 65, 116, 116, 114,
			105, 98, 117, 116, 101, 115, 46, 99, 115, 0,
			0, 0, 3, 0, 0, 0, 61, 92, 65, 115,
			115, 101, 116, 115, 92, 67, 111, 100, 101, 92,
			71, 97, 109, 101, 67, 111, 114, 101, 92, 76,
			111, 99, 97, 108, 105, 122, 97, 116, 105, 111,
			110, 92, 83, 104, 97, 114, 101, 100, 92, 76,
			111, 99, 97, 108, 105, 122, 97, 116, 105, 111,
			110, 80, 97, 99, 107, 46, 99, 115, 0, 0,
			0, 3, 0, 0, 0, 64, 92, 65, 115, 115,
			101, 116, 115, 92, 67, 111, 100, 101, 92, 71,
			97, 109, 101, 67, 111, 114, 101, 92, 76, 111,
			99, 97, 108, 105, 122, 97, 116, 105, 111, 110,
			92, 83, 104, 97, 114, 101, 100, 92, 76, 111,
			99, 97, 108, 105, 122, 101, 100, 83, 116, 114,
			105, 110, 103, 68, 97, 116, 97, 46, 99, 115
		};
		result.TypesData = new byte[496]
		{
			0, 0, 0, 0, 42, 75, 105, 110, 103, 109,
			97, 107, 101, 114, 46, 76, 111, 99, 97, 108,
			105, 122, 97, 116, 105, 111, 110, 46, 83, 104,
			97, 114, 101, 100, 124, 84, 114, 97, 105, 116,
			85, 116, 105, 108, 105, 116, 121, 0, 0, 0,
			0, 57, 75, 105, 110, 103, 109, 97, 107, 101,
			114, 46, 76, 111, 99, 97, 108, 105, 122, 97,
			116, 105, 111, 110, 46, 83, 104, 97, 114, 101,
			100, 124, 83, 116, 114, 105, 110, 103, 67, 114,
			101, 97, 116, 101, 87, 105, 110, 100, 111, 119,
			65, 116, 116, 114, 105, 98, 117, 116, 101, 0,
			0, 0, 0, 59, 75, 105, 110, 103, 109, 97,
			107, 101, 114, 46, 76, 111, 99, 97, 108, 105,
			122, 97, 116, 105, 111, 110, 46, 83, 104, 97,
			114, 101, 100, 124, 83, 116, 114, 105, 110, 103,
			67, 114, 101, 97, 116, 101, 84, 101, 109, 112,
			108, 97, 116, 101, 65, 116, 116, 114, 105, 98,
			117, 116, 101, 0, 0, 0, 0, 46, 75, 105,
			110, 103, 109, 97, 107, 101, 114, 46, 76, 111,
			99, 97, 108, 105, 122, 97, 116, 105, 111, 110,
			46, 83, 104, 97, 114, 101, 100, 124, 76, 111,
			99, 97, 108, 105, 122, 97, 116, 105, 111, 110,
			80, 97, 99, 107, 0, 0, 0, 0, 58, 75,
			105, 110, 103, 109, 97, 107, 101, 114, 46, 76,
			111, 99, 97, 108, 105, 122, 97, 116, 105, 111,
			110, 46, 83, 104, 97, 114, 101, 100, 46, 76,
			111, 99, 97, 108, 105, 122, 97, 116, 105, 111,
			110, 80, 97, 99, 107, 124, 83, 116, 114, 105,
			110, 103, 69, 110, 116, 114, 121, 0, 0, 0,
			0, 61, 75, 105, 110, 103, 109, 97, 107, 101,
			114, 46, 76, 111, 99, 97, 108, 105, 122, 97,
			116, 105, 111, 110, 46, 83, 104, 97, 114, 101,
			100, 46, 76, 111, 99, 97, 108, 105, 122, 97,
			116, 105, 111, 110, 80, 97, 99, 107, 124, 69,
			110, 116, 114, 121, 67, 111, 110, 118, 101, 114,
			116, 101, 114, 0, 0, 0, 0, 49, 75, 105,
			110, 103, 109, 97, 107, 101, 114, 46, 76, 111,
			99, 97, 108, 105, 122, 97, 116, 105, 111, 110,
			46, 83, 104, 97, 114, 101, 100, 124, 76, 111,
			99, 97, 108, 105, 122, 101, 100, 83, 116, 114,
			105, 110, 103, 68, 97, 116, 97, 0, 0, 0,
			0, 40, 75, 105, 110, 103, 109, 97, 107, 101,
			114, 46, 76, 111, 99, 97, 108, 105, 122, 97,
			116, 105, 111, 110, 46, 83, 104, 97, 114, 101,
			100, 124, 76, 111, 99, 97, 108, 101, 68, 97,
			116, 97, 0, 0, 0, 0, 39, 75, 105, 110,
			103, 109, 97, 107, 101, 114, 46, 76, 111, 99,
			97, 108, 105, 122, 97, 116, 105, 111, 110, 46,
			83, 104, 97, 114, 101, 100, 124, 84, 114, 97,
			105, 116, 68, 97, 116, 97
		};
		result.TotalFiles = 4;
		result.TotalTypes = 9;
		result.IsEditorOnly = false;
		return result;
	}
}
