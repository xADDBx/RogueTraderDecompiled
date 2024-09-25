using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Visual.Effects.LineRenderer;

public interface IJobLineRenderer : IJobParallelFor
{
	NativeArray<Point> Points { get; set; }

	NativeArray<LineDescriptor> Lines { get; set; }
}
