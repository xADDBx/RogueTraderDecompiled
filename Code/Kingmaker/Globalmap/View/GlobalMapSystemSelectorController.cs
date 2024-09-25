using DG.Tweening;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.Globalmap.View;

public class GlobalMapSystemSelectorController : MonoBehaviour, ISectorMapStarSystemChangeHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber, IAreaLoadingStagesHandler, IGameModeHandler
{
	[SerializeField]
	private float DecalMoveSpeed = 1f;

	public void OnEnable()
	{
		EventBus.Subscribe(this);
		OnGameModeStart(Game.Instance.CurrentMode);
	}

	public void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleStarSystemChanged()
	{
		base.transform.DOMove(EventInvokerExtensions.Entity.Position, DecalMoveSpeed);
	}

	public void OnAreaScenesLoaded()
	{
	}

	public void OnAreaLoadingComplete()
	{
		base.transform.position = Game.Instance.SectorMapController.GetCurrentStarSystem().Data.Position;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (Game.Instance.CurrentMode == GameModeType.GlobalMap)
		{
			base.transform.position = Game.Instance.SectorMapController.GetCurrentStarSystem().Data.Position;
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		if (Game.Instance.CurrentMode == GameModeType.GlobalMap)
		{
			base.transform.position = Game.Instance.SectorMapController.GetCurrentStarSystem().Data.Position;
		}
	}
}
