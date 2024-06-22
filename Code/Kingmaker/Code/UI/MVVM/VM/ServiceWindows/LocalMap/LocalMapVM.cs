using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Markers;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap.Utils;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GameConst;
using Kingmaker.View;
using Kingmaker.Visual.LocalMap;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.LocalMap;

public class LocalMapVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetPingPosition, ISubscriber
{
	public readonly ReactiveProperty<string> Title = new ReactiveProperty<string>();

	public readonly ReactiveProperty<WarhammerLocalMapRenderer.DrawResults> DrawResult = new ReactiveProperty<WarhammerLocalMapRenderer.DrawResults>();

	public readonly ReactiveCollection<LocalMapMarkerVM> MarkersVm = new ReactiveCollection<LocalMapMarkerVM>();

	public readonly ReactiveProperty<float> CompassAngle = new ReactiveProperty<float>(0f);

	private readonly Vector2 m_MaxSize;

	private Action m_Close;

	public readonly ReactiveCommand<(NetPlayer, Vector3)> CoopPingPosition = new ReactiveCommand<(NetPlayer, Vector3)>();

	public readonly LocalMapLegendBlockVM LocalMapLegendBlockVM;

	public BlueprintAreaPart.LocalMapRotationDegree LocalMapRotation => Game.Instance.CurrentlyLoadedAreaPart.LocalMapRotationDeg;

	private static HashSet<ILocalMapMarker> Markers => LocalMapModel.Markers;

	public LocalMapVM()
	{
		m_MaxSize = UIConsts.LocalMapMaxSize;
		EventBus.RaiseEvent(delegate(IFullScreenLocalMapUIHandler h)
		{
			h.HandleFullScreenLocalMapChanged(state: true);
		});
		Title.Value = Game.Instance.State.LoadedAreaState.Area.Blueprint.AreaDisplayName;
		SetDrawResult();
		SetMarkers();
		AddDisposable(LocalMapLegendBlockVM = new LocalMapLegendBlockVM());
		AddDisposable(Observable.EveryUpdate().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		MarkersVm.ForEach(delegate(LocalMapMarkerVM m)
		{
			m.Dispose();
		});
		MarkersVm.Clear();
		EventBus.RaiseEvent(delegate(IFullScreenLocalMapUIHandler h)
		{
			h.HandleFullScreenLocalMapChanged(state: false);
		});
	}

	private void SetDrawResult()
	{
		if (!(WarhammerLocalMapRenderer.Instance == null))
		{
			WarhammerLocalMapRenderer.DrawResults value = WarhammerLocalMapRenderer.Instance.Draw();
			DrawResult.Value = value;
			Shader.SetGlobalTexture("_LocalMapColorTex", value.ColorRT);
			Shader.SetGlobalVector("LocalMapFowScaleOffset", value.LocalMapFowScaleOffset);
		}
	}

	private void SetMarkers()
	{
		LocalMapModel.Markers.RemoveWhere((ILocalMapMarker m) => m.GetMarkerType() == LocalMapMarkType.Invalid);
		foreach (ILocalMapMarker marker in LocalMapModel.Markers)
		{
			if (LocalMapModel.IsInCurrentArea(marker.GetPosition()))
			{
				MarkersVm.Add(new LocalMapCommonMarkerVM(marker));
			}
		}
		IEnumerable<BaseUnitEntity> enumerable = Game.Instance.Player.PartyAndPets;
		if (Game.Instance.Player.CapitalPartyMode)
		{
			enumerable = enumerable.Concat(Game.Instance.Player.RemoteCompanions.Where((BaseUnitEntity u) => !u.IsCustomCompanion()));
		}
		foreach (BaseUnitEntity item2 in enumerable)
		{
			if (!item2.LifeState.IsHiddenBecauseDead && item2.IsInGame && LocalMapModel.IsInCurrentArea(item2.Position))
			{
				MarkersVm.Add(new LocalMapCharacterMarkerVM(item2));
				MarkersVm.Add(new LocalMapDestinationMarkerVM(item2));
			}
		}
		foreach (UnitGroupMemory.UnitInfo units in Game.Instance.Player.MainCharacterEntity.CombatGroup.Memory.UnitsList)
		{
			if (!units.Unit.Faction.IsPlayer && units.Unit.IsVisibleForPlayer && !units.Unit.LifeState.IsDead && LocalMapModel.IsInCurrentArea(units.Unit.Position))
			{
				LocalMapUnitMarkerVM item = new LocalMapUnitMarkerVM(units);
				MarkersVm.Add(item);
			}
		}
	}

	private void OnUpdateHandler()
	{
		DrawResult.Value = WarhammerLocalMapRenderer.Instance.Draw();
		CameraRig instance = CameraRig.Instance;
		CompassAngle.Value = instance.transform.eulerAngles.y - (float)LocalMapRotation;
		LocalMapModel.Markers.RemoveWhere((ILocalMapMarker m) => m.GetMarkerType() == LocalMapMarkType.Invalid);
		List<UnitGroupMemory.UnitInfo> unitsList = Game.Instance.Player.MainCharacterEntity.CombatGroup.Memory.UnitsList;
		List<LocalMapUnitMarkerVM> source = MarkersVm.OfType<LocalMapUnitMarkerVM>().ToList();
		List<UnitGroupMemory.UnitInfo> list = new List<UnitGroupMemory.UnitInfo>();
		foreach (UnitGroupMemory.UnitInfo character in unitsList)
		{
			if (character.Unit.Faction.IsPlayer || !character.Unit.IsVisibleForPlayer || character.Unit.LifeState.IsDead || !LocalMapModel.IsInCurrentArea(character.Unit.Position))
			{
				list.Add(character);
			}
			else if (source.FirstOrDefault((LocalMapUnitMarkerVM vm) => vm.UnitInfo == character) == null)
			{
				MarkersVm.Add(new LocalMapUnitMarkerVM(character));
			}
		}
		for (int i = 0; i < MarkersVm.Count; i++)
		{
			if (MarkersVm[i] is LocalMapUnitMarkerVM localMapUnitMarkerVM && list.Contains(localMapUnitMarkerVM.UnitInfo))
			{
				MarkersVm[i].Dispose();
				MarkersVm.RemoveAt(i);
			}
		}
	}

	public void OnClick(Vector2 viewPortPos, bool state, Entity entity = null, bool canPing = true)
	{
		Vector3 worldPos = WarhammerLocalMapRenderer.Instance.ViewportToWorldPoint(viewPortPos);
		if (!LocalMapModel.IsInCurrentArea(worldPos))
		{
			worldPos = Game.Instance.CurrentlyLoadedAreaPart.Bounds.LocalMapBounds.ClosestPoint(worldPos);
		}
		if (!canPing || !PhotonManager.Ping.CheckPingCoop(delegate
		{
			if (entity != null)
			{
				PhotonManager.Ping.PingEntity(entity);
			}
			else
			{
				PhotonManager.Ping.PingPosition(worldPos);
			}
		}))
		{
			if (state)
			{
				CameraRig.Instance.ScrollTo(worldPos);
			}
			else
			{
				UnitCommandsRunner.MoveSelectedUnitsToPoint(worldPos);
			}
		}
	}

	public void ScrollCameraToRogueTrader()
	{
		CameraRig.Instance.ScrollTo(Game.Instance.Player.MainCharacter.Entity.Position);
	}

	public void HandlePingPosition(NetPlayer player, Vector3 position)
	{
		CoopPingPosition.Execute((player, position));
	}

	public void HandlePingPositionSound(GameObject gameObject)
	{
	}
}
