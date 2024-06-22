using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using UnityEngine;

namespace Kingmaker.UI.Pointer;

public class ClickPointerManager : MonoBehaviour, IGameModeHandler, ISubscriber, IViewDetachedHandler, ISubscriber<IEntity>, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IPartyCombatHandler
{
	public ClickPointerPrefab PointerPrefab;

	public ClickPointerPrefab PreviewPointerPrefab;

	public GameObject PreviewArrow;

	public GameObject PlayerShipPointer;

	private readonly Dictionary<BaseUnitEntity, ClickPointerPrefab> m_PreviewUnitMarks = new Dictionary<BaseUnitEntity, ClickPointerPrefab>();

	private readonly Dictionary<BaseUnitEntity, ClickPointerPrefab> m_UnitMarks = new Dictionary<BaseUnitEntity, ClickPointerPrefab>();

	private readonly Dictionary<BaseUnitEntity, Vector3> m_UnitMarksLocalMap = new Dictionary<BaseUnitEntity, Vector3>();

	private GameObject m_PlayerShipMark;

	public static ClickPointerManager Instance { get; private set; }

	public Dictionary<BaseUnitEntity, Vector3> UnitMarksLocalMap => m_UnitMarksLocalMap;

	private void OnEnable()
	{
		EventBus.Subscribe(this);
		Instance = this;
	}

	private void OnDisable()
	{
		Clear();
		EventBus.Unsubscribe(this);
		Instance = null;
	}

	public void AddPointer(Vector3 clickEventWorldPosition, BaseUnitEntity unit)
	{
		if (!IsCorrectMode(Game.Instance.CurrentMode) || !unit.IsDirectlyControllable())
		{
			return;
		}
		m_UnitMarksLocalMap[unit] = clickEventWorldPosition;
		if (!Game.Instance.IsControllerGamepad)
		{
			if (!m_UnitMarks.TryGetValue(unit, out var value))
			{
				value = Object.Instantiate(PointerPrefab, base.transform);
				m_UnitMarks.Add(unit, value);
			}
			value.SetVisible(visible: true);
			value.transform.localPosition = clickEventWorldPosition;
			if (unit == Game.Instance.Player.PlayerShip)
			{
				m_PlayerShipMark = m_PlayerShipMark ?? Object.Instantiate(PlayerShipPointer, base.transform);
				m_PlayerShipMark.SetActive(value: true);
				m_PlayerShipMark.transform.localPosition = clickEventWorldPosition;
			}
		}
	}

	public void AddPreviewPointer(Vector3 markerPos, BaseUnitEntity unit)
	{
		if (IsCorrectMode(Game.Instance.CurrentMode))
		{
			if (!m_PreviewUnitMarks.TryGetValue(unit, out var value))
			{
				value = Object.Instantiate(PreviewPointerPrefab, base.transform);
				m_PreviewUnitMarks.Add(unit, value);
			}
			value.SetVisible(visible: true);
			value.transform.localPosition = markerPos;
			if (unit == Game.Instance.Player.PlayerShip)
			{
				m_PlayerShipMark = m_PlayerShipMark ?? Object.Instantiate(PlayerShipPointer, base.transform);
				m_PlayerShipMark.SetActive(value: true);
				m_PlayerShipMark.transform.localPosition = markerPos;
			}
		}
	}

	[UsedImplicitly]
	private void Update()
	{
		if (Game.Instance.IsControllerGamepad)
		{
			return;
		}
		foreach (KeyValuePair<BaseUnitEntity, ClickPointerPrefab> unitMark in m_UnitMarks)
		{
			BaseUnitEntity key = unitMark.Key;
			ClickPointerPrefab value = unitMark.Value;
			UnitMoveTo currentOrQueued = key.Commands.GetCurrentOrQueued<UnitMoveTo>();
			UnitMoveToProper currentOrQueued2 = key.Commands.GetCurrentOrQueued<UnitMoveToProper>();
			UnitAreaTransition current2 = key.Commands.GetCurrent<UnitAreaTransition>();
			bool flag = (currentOrQueued != null && !currentOrQueued.IsFinished) || (currentOrQueued2 != null && !currentOrQueued2.IsFinished) || (current2 != null && !current2.IsFinished);
			bool flag2 = TurnController.IsInTurnBasedCombat();
			if (key.GetSaddledUnit() == null && flag && !Game.Instance.IsControllerGamepad && IsCorrectMode(Game.Instance.CurrentMode) && !flag2)
			{
				if (Game.Instance.SelectionCharacter.SelectedUnits.Contains(key))
				{
					value.SetVisible(visible: true);
				}
				else
				{
					value.SetVisible(visible: true, 0.5f);
				}
				continue;
			}
			value.SetVisible(visible: false);
			if (key == Game.Instance.Player.PlayerShip && (bool)m_PlayerShipMark)
			{
				m_PlayerShipMark.SetActive(value: false);
			}
		}
	}

	public void CancelPreview()
	{
		m_UnitMarksLocalMap.Clear();
		foreach (KeyValuePair<BaseUnitEntity, ClickPointerPrefab> previewUnitMark in m_PreviewUnitMarks)
		{
			previewUnitMark.Deconstruct(out var _, out var value);
			value.SetVisible(visible: false);
		}
		if ((bool)m_PlayerShipMark)
		{
			m_PlayerShipMark.SetActive(value: false);
		}
		PreviewArrow.SetActive(value: false);
	}

	public void ShowPreviewArrow(Vector3 worldPosition, Vector3 direction)
	{
		PreviewArrow.SetActive(value: true);
		float y = Mathf.Atan2(direction.x, direction.z) * 57.29578f;
		PreviewArrow.transform.eulerAngles = new Vector3(0f, y, 0f);
		Quaternion quaternion = Quaternion.LookRotation(direction);
		PreviewArrow.transform.localPosition = worldPosition + quaternion * Vector3.forward;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (!IsCorrectMode(gameMode))
		{
			CancelPreview();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	private bool IsCorrectMode(GameModeType gameMode)
	{
		if (gameMode != GameModeType.Dialog && gameMode != GameModeType.Cutscene)
		{
			return gameMode != GameModeType.GameOver;
		}
		return false;
	}

	public void OnViewDetached(IEntityViewBase view)
	{
		Entity entity = EventInvokerExtensions.Entity;
		if (entity is BaseUnitEntity key)
		{
			if (m_UnitMarksLocalMap.TryGetValue(key, out var _))
			{
				m_UnitMarksLocalMap.Remove(key);
			}
			if (m_UnitMarks.TryGetValue(key, out var value2))
			{
				Object.Destroy(value2);
				m_UnitMarks.Remove(key);
			}
			if (m_PreviewUnitMarks.TryGetValue(key, out var value3))
			{
				Object.Destroy(value3);
				m_PreviewUnitMarks.Remove(key);
			}
			if (entity == Game.Instance.Player.PlayerShip && (bool)m_PlayerShipMark)
			{
				m_PlayerShipMark.SetActive(value: false);
				Object.Destroy(m_PlayerShipMark);
			}
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			CancelPreview();
		}
	}

	private void Clear()
	{
		BaseUnitEntity key;
		ClickPointerPrefab value;
		foreach (KeyValuePair<BaseUnitEntity, ClickPointerPrefab> unitMark in m_UnitMarks)
		{
			unitMark.Deconstruct(out key, out value);
			Object.Destroy(value);
		}
		m_UnitMarks.Clear();
		foreach (KeyValuePair<BaseUnitEntity, ClickPointerPrefab> previewUnitMark in m_PreviewUnitMarks)
		{
			previewUnitMark.Deconstruct(out key, out value);
			Object.Destroy(value);
		}
		m_PreviewUnitMarks.Clear();
		if (m_PlayerShipMark != null)
		{
			m_PlayerShipMark.SetActive(value: false);
			Object.Destroy(m_PlayerShipMark);
		}
		m_UnitMarksLocalMap.Clear();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		CancelPreview();
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		CancelPreview();
	}
}
