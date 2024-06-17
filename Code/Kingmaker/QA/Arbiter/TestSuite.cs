using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.QA.Arbiter;

internal class TestSuite
{
	public string Name;

	public Type Type;

	public IEnumerable<AbstractReportEntity> GetEntities(IEnumerable<AbstractReportEntity> entities)
	{
		return entities.Where((AbstractReportEntity x) => x.GetType() == Type).NotNull();
	}
}
