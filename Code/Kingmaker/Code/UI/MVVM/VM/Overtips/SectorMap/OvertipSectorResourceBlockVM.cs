using System;
using System.Collections.Generic;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.SectorMap;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SectorMap;

public class OvertipSectorResourceBlockVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly SectorMapObjectEntity m_SectorMapObject;

	private readonly Action m_CloseAction;

	public OvertipSectorResourceBlockVM(SectorMapObjectEntity sectorMapObject, Action closeAction)
	{
		m_SectorMapObject = sectorMapObject;
		m_CloseAction = closeAction;
	}

	protected override void DisposeImplementation()
	{
	}

	private void Close()
	{
		m_CloseAction?.Invoke();
	}

	public Dictionary<BlueprintResource, int> GetResourcesFromPlanet()
	{
		return new Dictionary<BlueprintResource, int>();
	}

	public Dictionary<BlueprintResource, int> GetNeeds()
	{
		return Game.Instance.ColonizationController.GetColony(m_SectorMapObject.View)?.RequiredResourcesForColony();
	}
}
