using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Code.Mechanics.Entities;

public class ExtraUnitContext : ContextFlag<ExtraUnitContext>
{
	[CanBeNull]
	public static ExtraUnitContext Request([NotNull] IBaseUnitEntity unit)
	{
		return ContextData<ExtraUnitContext>.RequestIf(unit.IsExtra);
	}
}
