using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;

public class OvertipEntitySystemObjectVM : OvertipEntityVM, IExplorationUIHandler, ISubscriber, IGameModeHandler, IAreaLoadingStagesHandler
{
	public readonly MapObjectEntity SystemMapObject;

	public readonly ReactiveProperty<bool> IsPoi = new ReactiveProperty<bool>();

	public readonly BoolReactiveProperty IsScanned = new BoolReactiveProperty();

	public readonly List<string> PoiNamesList = new List<string>();

	protected override Vector3 GetEntityPosition()
	{
		return SystemMapObject.Position;
	}

	public OvertipEntitySystemObjectVM(MapObjectEntity systemMapObjectData)
	{
		AddDisposable(EventBus.Subscribe(this));
		SystemMapObject = systemMapObjectData;
		SetPlanetIconsState();
		IsScanned.Value = CheckScanned();
	}

	private void SetPlanetIconsState()
	{
		if (SystemMapObject is StarSystemObjectEntity starSystemObjectEntity)
		{
			IsPoi.Value = PoiIsVisible(starSystemObjectEntity, starSystemObjectEntity.IsScanned);
		}
	}

	public void RequestVisit()
	{
		VisitPlanet();
	}

	private void VisitPlanet()
	{
		Game.Instance.GameCommandQueue.MoveShip(SystemMapObject, MoveShipGameCommand.VisitType.MovePlayerShip);
	}

	private bool PoiIsVisible(StarSystemObjectEntity star, bool scan)
	{
		bool num = star.PointOfInterests.Count > 0 && !star.IsFullyExplored && scan;
		bool result = false;
		PoiNamesList.Clear();
		if (num)
		{
			IEnumerable<BasePointOfInterest> source = star.PointOfInterests.Where((BasePointOfInterest p) => p.IsVisible() && p.Status != BasePointOfInterest.ExplorationStatus.Explored);
			if (source.Count() != 0)
			{
				source.ForEach(delegate(BasePointOfInterest p)
				{
					PoiNamesList.Add("- " + p.Blueprint.Name);
				});
				result = true;
			}
		}
		return result;
	}

	public void OpenExplorationScreen(MapObjectView explorationObjectView)
	{
	}

	public void CloseExplorationScreen()
	{
		CheckIconStateAndScan();
	}

	private bool CheckScanned()
	{
		if (!(SystemMapObject is StarSystemObjectEntity starSystemObjectEntity))
		{
			return false;
		}
		if (!starSystemObjectEntity.IsScanned)
		{
			return starSystemObjectEntity.IsScannedOnStart;
		}
		return true;
	}

	private void CheckIconStateAndScan()
	{
		SetPlanetIconsState();
		IsScanned.Value = CheckScanned();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			CheckIconStateAndScan();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			CheckIconStateAndScan();
		}
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			CheckIconStateAndScan();
		}
	}
}
