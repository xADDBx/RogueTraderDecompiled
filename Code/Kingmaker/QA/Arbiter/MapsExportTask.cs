using System.Collections;

namespace Kingmaker.QA.Arbiter;

public class MapsExportTask : ArbiterTask
{
	protected override IEnumerator Routine()
	{
		PFLog.Arbiter.Log("Map export is not supported in build");
		yield return null;
	}
}
