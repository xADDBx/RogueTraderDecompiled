using System.Collections.Generic;
using System.Linq;
using Kingmaker.QA.Profiling;
using Owlcat.Core.Overlays;
using Owlcat.Runtime.Core.ProfilingCounters;

namespace Kingmaker.QA.Overlays;

public class CountersOverlay : Overlay
{
	public CountersOverlay()
		: base("Counters", CreateElements().ToArray())
	{
		SharedGraphBounds = true;
	}

	private static IEnumerable<OverlayElement> CreateElements()
	{
		return from l in Kingmaker.QA.Profiling.Counters.All.Select(CreateLabel).Concat(Kingmaker.QA.Profiling.Counters.All.Select(CreateGraph))
			orderby l.Name
			select l;
	}

	private static OverlayElement CreateGraph(Counter arg)
	{
		return new Graph(arg)
		{
			Hidden = true
		};
	}

	private static OverlayElement CreateLabel(Counter arg)
	{
		return new Label(arg);
	}
}
