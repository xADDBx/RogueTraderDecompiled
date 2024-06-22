using System;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Common.PlanetState;

public class StarSystemObjectStateVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IExplorationUIHandler, ISubscriber, IScanStarSystemObjectHandler, ISubscriber<StarSystemObjectEntity>, IColonizationHandler
{
	public readonly ReactiveProperty<Colony> Colony = new ReactiveProperty<Colony>();

	public readonly ReactiveProperty<bool> IsPlanet = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsScanned = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<StarSystemObjectView> StarSystemObjectView = new ReactiveProperty<StarSystemObjectView>();

	public readonly ReactiveProperty<PlanetView> PlanetView = new ReactiveProperty<PlanetView>();

	private bool m_IsExplorationScreenOpened;

	public static StarSystemObjectStateVM Instance => Game.Instance.RootUiContext.CommonVM.StarSystemObjectStateVM;

	public bool HasColony
	{
		get
		{
			if (PlanetView.Value != null)
			{
				return PlanetView.Value.Data.Colony != null;
			}
			return false;
		}
	}

	public StarSystemObjectStateVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
		StarSystemObjectView starSystemObjectView = explorationObjectView as StarSystemObjectView;
		if (starSystemObjectView == null)
		{
			PFLog.UI.Error("StarSystemObjectStateVM.OpenExplorationScreen - unsupported MapObjectView type!");
			return;
		}
		StarSystemObjectView.Value = starSystemObjectView;
		IsScanned.Value = starSystemObjectView.Data.IsScanned;
		PlanetView.Value = starSystemObjectView as PlanetView;
		IsPlanet.Value = PlanetView.Value != null;
		if (PlanetView.Value != null)
		{
			Colony.Value = PlanetView.Value.Data.Colony;
		}
		m_IsExplorationScreenOpened = true;
	}

	public void CloseExplorationScreen()
	{
		StarSystemObjectView.Value = null;
		PlanetView.Value = null;
		Colony.Value = null;
		IsScanned.Value = false;
		IsPlanet.Value = false;
		m_IsExplorationScreenOpened = false;
	}

	public void HandleStartScanningStarSystemObject()
	{
	}

	public void HandleScanStarSystemObject()
	{
		IsScanned.Value = true;
	}

	public void HandleColonyCreated(Colony colony, PlanetEntity planetEntity)
	{
		if (m_IsExplorationScreenOpened)
		{
			UISounds.Instance.Sounds.SpaceColonization.PlanetColonized.Play();
			Colony.Value = colony;
		}
	}
}
