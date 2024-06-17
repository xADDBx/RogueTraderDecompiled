using System.Collections.Generic;
using Kingmaker.Stores.DlcInterfaces;

namespace Kingmaker.Stores;

public class DLCCache
{
	private Dictionary<IBlueprintDlc, IDLCStatus> m_DlcAvailability = new Dictionary<IBlueprintDlc, IDLCStatus>();

	public void OnDLCUpdate(IBlueprintDlc dlc, IDLCStatus res)
	{
		lock (m_DlcAvailability)
		{
			m_DlcAvailability[dlc] = res;
		}
	}

	public IDLCStatus Get(IBlueprintDlc dlc)
	{
		lock (m_DlcAvailability)
		{
			m_DlcAvailability.TryGetValue(dlc, out var value);
			return value;
		}
	}
}
