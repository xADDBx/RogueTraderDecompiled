using System.Collections.Generic;

namespace Kingmaker.QA.Arbiter.Tasks;

public class MapsExportTask : ArbiterTask
{
	protected override IEnumerator<ArbiterTask> Routine()
	{
		PFLog.System.Log("Map export is not supported in build");
		yield return null;
	}
}
