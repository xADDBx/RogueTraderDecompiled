using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Enums;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class FactionAllVendors : StringsContainer
{
	[SerializeField]
	private FactionVendorInfo[] m_DrusiansVendorsInfo;

	[SerializeField]
	private FactionVendorInfo[] m_ExploratorsVendorsInfo;

	[SerializeField]
	private FactionVendorInfo[] m_KasballicaVendorsInfo;

	[SerializeField]
	private FactionVendorInfo[] m_PiratesVendorsInfo;

	private FactionVendorInfo[][] m_AllFactionsVendorsInfo;

	public FactionVendorInfo[] TakeFactionVendorsInfo(FactionType factionType)
	{
		m_AllFactionsVendorsInfo = new FactionVendorInfo[4][] { m_DrusiansVendorsInfo, m_ExploratorsVendorsInfo, m_KasballicaVendorsInfo, m_PiratesVendorsInfo };
		if (factionType == FactionType.None)
		{
			return null;
		}
		return m_AllFactionsVendorsInfo[(int)(factionType - 1)];
	}
}
