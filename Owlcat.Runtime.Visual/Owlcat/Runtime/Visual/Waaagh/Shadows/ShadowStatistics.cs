using System.Text;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct ShadowStatistics
{
	public int CachedCount;

	public int UseCandidateCount;

	public int UseCount;

	public int RenderCandidateCount;

	public int RenderCount;

	public int UpdateRenderDataCandidateCount;

	public int UpdateRenderDataCount;

	public bool DynamicAtlasOverflowDetected;

	public bool CachedAtlasOverflowDetected;

	public bool ConstantBufferOverflowDetected;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		Append(stringBuilder);
		return stringBuilder.ToString();
	}

	public void Append(StringBuilder builder)
	{
		builder.Append("( cached: ").Append(CachedCount).Append(", used: ")
			.Append(UseCount)
			.Append("/")
			.Append(UseCandidateCount)
			.Append(", rendered: ")
			.Append(RenderCount)
			.Append("/")
			.Append(RenderCandidateCount)
			.Append(", renderDataUpdated: ")
			.Append(UpdateRenderDataCount)
			.Append("/")
			.Append(UpdateRenderDataCandidateCount)
			.Append(", dynamicAtlasOverflow: ")
			.Append(DynamicAtlasOverflowDetected)
			.Append(", cachedAtlasOverflow: ")
			.Append(CachedAtlasOverflowDetected)
			.Append(", constantBufferOverflow: ")
			.Append(ConstantBufferOverflowDetected)
			.Append(")");
	}
}
