using System.Text;

namespace Owlcat.Runtime.Visual.IndirectRendering;

public struct IndirectRenderingStatisticsInfo
{
	public int InstanceCount;

	public int MeshCount;

	public int DrawcallCount;

	public int MeshBufferCount;

	public int ArgsBufferCount;

	public int InstanceBufferCount;

	public void BuildString(StringBuilder sb)
	{
		sb.AppendLine("<b>Indirect Rendering System Stats:</b>");
		sb.AppendLine($"Instance Count: {InstanceCount}");
		sb.AppendLine($"Mesh Count: {MeshCount}");
		sb.AppendLine($"Drawcall Count: {DrawcallCount}");
		sb.AppendLine($"MeshBuffer Count: {MeshBufferCount}");
		sb.AppendLine($"ArgsBuffer Count: {ArgsBufferCount}");
		sb.AppendLine($"InstanceBuffer Count: {InstanceBufferCount}");
	}
}
