using System.Collections.Generic;

namespace Kingmaker.Stores.DlcInterfaces;

public interface IDlcRoot
{
	IEnumerable<IBlueprintDlc> Dlcs { get; }
}
