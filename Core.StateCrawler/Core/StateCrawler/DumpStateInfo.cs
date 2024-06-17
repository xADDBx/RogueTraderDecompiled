using System.Collections.Generic;

namespace Core.StateCrawler;

public struct DumpStateInfo
{
	public string RootObjectPath;

	public List<StateCrawler.ExpandedChildren> ExpandedChildren;
}
