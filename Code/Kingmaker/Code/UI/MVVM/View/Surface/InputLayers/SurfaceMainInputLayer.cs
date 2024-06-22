using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.Pointer;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Interaction;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.GameConst;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Surface.InputLayers;

public class SurfaceMainInputLayer : InputLayer, IDisposable, IAbilityTargetSelectionUIHandler, ISubscriber
{
	private IDisposable m_FrameUpdate;

	protected Vector2 m_LeftStickVector;

	protected Vector2 m_RightStickVector;

	private bool m_ForceCursorEnabled;

	private List<Entity> m_PotentialInteractables = new List<Entity>();

	private List<Entity> m_PotentialInteractablesFiltered = new List<Entity>();

	private Entity m_ClosestInteract;

	private GameObject m_LastChoosenInteract = new GameObject();

	private List<GameObject> m_LastInteractableObjects = new List<GameObject>();

	private List<GameObject> m_InteractableObjects = new List<GameObject>();

	private GameObject m_ChoosenInteractableObject;

	private GameObject m_OverrideClosestInteractableObject;

	public static SurfaceMainInputLayer Instance { get; private set; }

	public SurfaceMainInputLayer()
	{
		if (GetType() == typeof(SurfaceMainInputLayer))
		{
			Instance = this;
		}
		m_CursorEnabled = false;
		m_FrameUpdate = MainThreadDispatcher.LateUpdateAsObservable().Subscribe(delegate
		{
			OnUpdate();
		});
		EventBus.Subscribe(this);
		AddAxis2D(OnMoveLeftStick, 0, 1, repeat: false);
		AddAxis2D(OnMoveRightStick, 2, 3, repeat: false);
		OnUnbind = StopMovement;
		OnBind = ResetStopMovementFlag;
	}

	public virtual void Dispose()
	{
		m_FrameUpdate?.Dispose();
		m_FrameUpdate = null;
		EventBus.Unsubscribe(this);
	}

	private void OnMoveLeftStick(InputActionEventData eventData, Vector2 vec)
	{
		m_LeftStickVector = vec;
	}

	private void OnMoveRightStick(InputActionEventData eventData, Vector2 vec)
	{
		if ((!(Game.Instance.CurrentMode != GameModeType.Default) || !(Game.Instance.CurrentMode != GameModeType.Pause)) && !CutsceneLock.Active)
		{
			m_RightStickVector = vec;
		}
	}

	private void OnUpdate()
	{
		if (!Game.Instance.IsControllerMouse && LayerBinded.Value)
		{
			UpdateLeftStickMovement();
			if (m_RightStickVector != Vector2.zero)
			{
				UpdateRightStickMovement();
			}
			using (ProfileScope.New("Console UpdateInteractions"))
			{
				UpdateInteractions();
			}
			m_LeftStickVector = (m_RightStickVector = Vector2.zero);
		}
	}

	protected virtual void UpdateLeftStickMovement()
	{
		ConsoleCursor consoleCursor = ConsoleCursor.Instance.Or(null);
		if ((object)consoleCursor != null && consoleCursor.IsActive && (Game.Instance.CurrentMode == GameModeType.Default || Game.Instance.CurrentMode == GameModeType.Pause))
		{
			Game.Instance.CameraController.Follower.Release();
			CameraRig.Instance.ScrollBy2D(m_LeftStickVector);
			if (!CameraRig.Instance.OnLevelBound)
			{
				ConsoleCursor consoleCursor2 = ConsoleCursor.Instance.Or(null);
				if ((object)consoleCursor2 == null || consoleCursor2.OnScreenCenter)
				{
					ConsoleCursor.Instance.Or(null)?.SetToCenter();
					return;
				}
			}
			ConsoleCursor.Instance.Or(null)?.MoveCursor(m_LeftStickVector);
		}
		else
		{
			if (!GamepadInputController.CanProcessInput)
			{
				return;
			}
			SelectionManagerConsole selectionManagerConsole = SelectionManagerConsole.Instance.Or(null);
			if ((object)selectionManagerConsole != null && selectionManagerConsole.StopMoveFlag)
			{
				if (m_LeftStickVector == Vector2.zero)
				{
					SelectionManagerConsole.Instance.StopMoveFlag = false;
				}
				return;
			}
			BaseUnitEntity value = Game.Instance.SelectionCharacter.SelectedUnit.Value;
			if (value == null || !value.IsDirectlyControllable())
			{
				return;
			}
			CameraRig instance = CameraRig.Instance;
			if (!(instance == null))
			{
				float magnitude = m_LeftStickVector.magnitude;
				Vector2 moveDirection = (m_LeftStickVector.x * instance.Right + m_LeftStickVector.y * instance.Up).To2D();
				moveDirection.Normalize();
				Game.Instance.SynchronizedDataController.PushLeftStickMovement(value, moveDirection, magnitude);
				if (Mathf.Epsilon <= moveDirection.sqrMagnitude)
				{
					m_OverrideClosestInteractableObject = null;
				}
			}
		}
	}

	protected virtual void UpdateRightStickMovement()
	{
		MoveRotateCamera(m_RightStickVector);
	}

	public static void MoveRotateCamera(Vector2 vector)
	{
		CameraRig instance = CameraRig.Instance;
		bool flag = ConsoleCursor.Instance.Or(null)?.IsActive ?? false;
		if (!Game.Instance.Player.IsCameraRotateMode && !flag)
		{
			Game.Instance.CameraController.Follower.Release();
			instance.ScrollBy2D(vector);
			return;
		}
		if (Mathf.Abs(vector.x) < UIConsts.SurfaceRotateOnTriggerClamp && Mathf.Abs(vector.y) > UIConsts.SurfaceZoomOnTriggerClamp)
		{
			instance.CameraZoom.GamepadScrollPosition = vector.y * UIConsts.SurfaceZoomSpeed;
		}
		if (vector.x < 0f - UIConsts.SurfaceRotateOnTriggerClamp)
		{
			instance.RotateRight();
		}
		if (vector.x > UIConsts.SurfaceRotateOnTriggerClamp)
		{
			instance.RotateLeft();
		}
	}

	public static void MoveCursor(Vector2 vector)
	{
		ConsoleCursor consoleCursor = ConsoleCursor.Instance.Or(null);
		if ((object)consoleCursor == null || !consoleCursor.IsActive || (!(Game.Instance.CurrentMode == GameModeType.Default) && !(Game.Instance.CurrentMode == GameModeType.Pause)))
		{
			return;
		}
		Game.Instance.CameraController.Follower.Release();
		CameraRig.Instance.ScrollBy2D(vector);
		if (!CameraRig.Instance.OnLevelBound)
		{
			ConsoleCursor consoleCursor2 = ConsoleCursor.Instance.Or(null);
			if ((object)consoleCursor2 == null || consoleCursor2.OnScreenCenter)
			{
				ConsoleCursor.Instance.Or(null)?.SetToCenter();
				return;
			}
		}
		ConsoleCursor.Instance.Or(null)?.MoveCursor(vector);
	}

	protected void UpdateInteractions()
	{
		if (!Game.Instance.Player.IsInCombat)
		{
			if (!base.CursorEnabled && (Game.Instance.CurrentMode == GameModeType.Default || Game.Instance.CurrentMode == GameModeType.Pause))
			{
				UpdateInteractableSet();
			}
			else
			{
				m_ChoosenInteractableObject = null;
			}
		}
	}

	public void SwitchCursorEnabled()
	{
		base.CursorEnabled = !base.CursorEnabled;
	}

	protected override void SetupCursor(bool value)
	{
		Game.Instance.CursorController.SetActive(value);
		base.SetupCursor(value);
		TooltipHelper.HideInfo();
		TooltipHelper.HideTooltip();
	}

	public virtual void OnPrevInteractable()
	{
		if (m_InteractableObjects.Count <= 1)
		{
			return;
		}
		int num = m_InteractableObjects.FindIndex((GameObject o) => o == m_ChoosenInteractableObject);
		if (num != -1)
		{
			num--;
			if (num < 0)
			{
				num = m_InteractableObjects.Count - 1;
			}
			m_OverrideClosestInteractableObject = m_InteractableObjects[num];
		}
	}

	public virtual void OnNextInteractable()
	{
		if (m_InteractableObjects.Count <= 1)
		{
			return;
		}
		int num = m_InteractableObjects.FindIndex((GameObject o) => o == m_ChoosenInteractableObject);
		if (num != -1)
		{
			num++;
			if (num >= m_InteractableObjects.Count)
			{
				num = 0;
			}
			m_OverrideClosestInteractableObject = m_InteractableObjects[num];
		}
	}

	private bool TryRefreshInteractableObjectsList()
	{
		bool flag = false;
		m_ClosestInteract = null;
		m_LastInteractableObjects.Clear();
		m_LastInteractableObjects.AddRange(m_InteractableObjects);
		m_InteractableObjects.Clear();
		m_PotentialInteractables.Clear();
		m_PotentialInteractablesFiltered.Clear();
		BaseUnitEntity value = Game.Instance.SelectionCharacter.SelectedUnit.Value;
		if (value == null)
		{
			if (m_LastInteractableObjects.Any())
			{
				return true;
			}
			return false;
		}
		Vector3 interactorPosition = value.Position;
		Vector2 castDir = (value.View.transform.rotation * Vector3.forward).To2D();
		using (ProfileScope.New("Console UpdateInteractions - PotentialInteractables"))
		{
			m_PotentialInteractables.AddRange(EntityBoundsHelper.FindEntitiesInRange(interactorPosition, 12.7f));
		}
		float num = 0f;
		float num2 = float.MaxValue;
		for (int i = 0; i < m_PotentialInteractables.Count; i++)
		{
			Entity entity = m_PotentialInteractables[i];
			if (!(EntityViewBase)entity.View || entity?.View?.GO == null || !entity.View.GO.activeInHierarchy || entity == value)
			{
				continue;
			}
			float num3 = 6.35f;
			BaseUnitEntity baseUnitEntity = entity as BaseUnitEntity;
			if (baseUnitEntity != null)
			{
				if (Game.Instance.Player.PartyCharacters.Where((UnitReference c) => c.Entity.IsDirectlyControllable).Contains(baseUnitEntity.FromBaseUnitEntity()) || (baseUnitEntity.IsPet && Game.Instance.Player.PartyCharacters.Contains(baseUnitEntity.Master.FromBaseUnitEntity())) || baseUnitEntity.IsInFogOfWar)
				{
					continue;
				}
				bool flag2 = baseUnitEntity.IsDeadAndHasLoot && !baseUnitEntity.Faction.IsPlayer;
				IUnitInteraction unitInteraction = baseUnitEntity.GetOptional<UnitPartInteractions>()?.SelectClickInteraction(value);
				bool flag3 = unitInteraction != null && !baseUnitEntity.LifeState.IsDead && !baseUnitEntity.Faction.IsPlayerEnemy;
				if (!flag3 && !flag2)
				{
					continue;
				}
				if (flag3)
				{
					num3 += (float)unitInteraction.Distance;
				}
				if (flag2)
				{
					num3 += 4f;
				}
			}
			LightweightUnitEntity lightweightUnitEntity = entity as LightweightUnitEntity;
			if (lightweightUnitEntity != null)
			{
				if (lightweightUnitEntity.IsInFogOfWar)
				{
					continue;
				}
				bool isDeadAndHasLoot = lightweightUnitEntity.IsDeadAndHasLoot;
				IUnitInteraction unitInteraction2 = lightweightUnitEntity.GetOptional<UnitPartInteractions>()?.SelectClickInteraction(value);
				bool flag4 = unitInteraction2 != null && !lightweightUnitEntity.LifeState.IsDead;
				if (!flag4 && !isDeadAndHasLoot)
				{
					continue;
				}
				if (flag4)
				{
					num3 += (float)unitInteraction2.Distance;
				}
				if (isDeadAndHasLoot)
				{
					num3 += 4f;
				}
			}
			AreaTransitionPart areaTransitionPart = (entity as MapObjectEntity)?.GetOptional<AreaTransitionPart>();
			if (areaTransitionPart != null && !areaTransitionPart.CheckRestrictions(value))
			{
				continue;
			}
			IInteractionComponent component = entity.View.GO.GetComponent<IInteractionComponent>();
			if (component != null)
			{
				if (!ClickMapObjectHandler.HasAvailableInteractions(entity.View.GO))
				{
					continue;
				}
				num3 += (float)component.Settings.ProximityRadius;
				if (component is DisableTrapInteractionComponent)
				{
					num3 += 4f;
				}
			}
			if (baseUnitEntity == null && lightweightUnitEntity == null && areaTransitionPart == null && component == null)
			{
				continue;
			}
			Vector3 vector = baseUnitEntity?.Position ?? lightweightUnitEntity?.Position ?? entity.View.ViewTransform.position;
			Vector3 vector2 = interactorPosition;
			float sqrMagnitude = (vector - vector2).sqrMagnitude;
			if (!(sqrMagnitude > num3 * num3) && IsInCone(value.Position, vector, castDir, out var angleCos))
			{
				m_PotentialInteractablesFiltered.Add(entity);
				if (m_ClosestInteract == null || (num < 0.86f && angleCos > num) || (num > 0.86f && angleCos > 0.86f && sqrMagnitude < num2))
				{
					m_ClosestInteract = entity;
					num = angleCos;
					num2 = sqrMagnitude;
				}
			}
		}
		if (m_ClosestInteract != null)
		{
			Vector3 closestObjDir = m_ClosestInteract.Position;
			m_PotentialInteractablesFiltered.Sort((Entity a, Entity b) => GetCrossProduct2D(interactorPosition, a.Position, closestObjDir).CompareTo(GetCrossProduct2D(interactorPosition, b.Position, closestObjDir)));
		}
		foreach (Entity item in m_PotentialInteractablesFiltered)
		{
			GameObject gO = item.View.GO;
			if (!(gO == null))
			{
				if (!m_LastInteractableObjects.Remove(gO))
				{
					flag = true;
				}
				m_InteractableObjects.Add(item.View.GO);
			}
		}
		if (!flag)
		{
			return m_LastInteractableObjects.Any();
		}
		return true;
	}

	private static float GetCrossProduct2D(Vector3 launchPos, Vector3 targetPos, Vector3 closestObjDir)
	{
		return (targetPos.x - launchPos.x) * (closestObjDir.z - launchPos.z) - (targetPos.z - launchPos.z) * (closestObjDir.z - launchPos.z);
	}

	private static bool IsInCone(Vector3 launchPos, Vector3 targetPos, Vector2 castDir, out float angleCos)
	{
		Vector2 vector = targetPos.To2D();
		Vector2 vector2 = launchPos.To2D();
		angleCos = Vector3.Dot((vector - vector2).normalized, castDir.normalized);
		return angleCos > 0f;
	}

	private void UpdateInteractableSet()
	{
		m_LastChoosenInteract = m_ChoosenInteractableObject;
		m_ChoosenInteractableObject = null;
		bool num = TryRefreshInteractableObjectsList();
		if (m_OverrideClosestInteractableObject != null && !m_InteractableObjects.Contains(m_OverrideClosestInteractableObject))
		{
			m_OverrideClosestInteractableObject = null;
		}
		if (m_OverrideClosestInteractableObject != null)
		{
			m_ChoosenInteractableObject = m_OverrideClosestInteractableObject;
		}
		else if (m_InteractableObjects.Contains(m_LastChoosenInteract))
		{
			m_ChoosenInteractableObject = m_LastChoosenInteract;
		}
		else
		{
			m_ChoosenInteractableObject = m_ClosestInteract?.View?.GO;
		}
		if (!num && m_ChoosenInteractableObject == m_LastChoosenInteract)
		{
			return;
		}
		bool isInNavigation = m_InteractableObjects.Count > 1;
		foreach (GameObject interactableObject in m_InteractableObjects)
		{
			m_LastInteractableObjects.Remove(interactableObject);
			TryInvokeUpdateHandle(interactableObject, isInNavigation);
		}
		foreach (GameObject lastInteractableObject in m_LastInteractableObjects)
		{
			TryInvokeUpdateHandle(lastInteractableObject, isInNavigation: false);
		}
	}

	private void TryInvokeUpdateHandle(GameObject interactionObject, bool isInNavigation)
	{
		EntityViewBase view = ((interactionObject != null) ? interactionObject.GetComponent<EntityViewBase>() : null);
		if (view != null)
		{
			bool isChosen = m_ChoosenInteractableObject == interactionObject;
			UnitEntityView component = view.GetComponent<UnitEntityView>();
			if (component != null && component.MouseHighlighted != isChosen)
			{
				component.MouseHighlighted = isChosen;
			}
			LightweightUnitEntityView component2 = view.GetComponent<LightweightUnitEntityView>();
			if (component2 != null && component2.MouseHighlighted != isChosen)
			{
				component2.MouseHighlighted = isChosen;
			}
			MapObjectView component3 = interactionObject.GetComponent<MapObjectView>();
			if (component3 != null && component3.Highlighted != isChosen)
			{
				component3.Highlighted = isChosen;
			}
			EventBus.RaiseEvent(delegate(ISurroundingInteractableObjectsCountHandler h)
			{
				h.HandleSurroundingInteractableObjectsCountChanged(view, isInNavigation, isChosen);
			});
		}
	}

	public void OnInteract()
	{
		if (Game.Instance.CurrentMode != GameModeType.Default || ConsoleCursor.Instance.IsActive || CutsceneLock.Active)
		{
			return;
		}
		BaseUnitEntity value = Game.Instance.SelectionCharacter.SelectedUnit.Value;
		if (value == null || m_ChoosenInteractableObject == null || !value.IsDirectlyControllable())
		{
			return;
		}
		UnitEntityView component = m_ChoosenInteractableObject.GetComponent<UnitEntityView>();
		if (component != null)
		{
			ClickUnitHandler.HandleClickUnit(value, component);
		}
		LightweightUnitEntityView component2 = m_ChoosenInteractableObject.GetComponent<LightweightUnitEntityView>();
		if (component2 != null)
		{
			ClickUnitHandler.HandleClickUnit(value, component2);
		}
		MapObjectView component3 = m_ChoosenInteractableObject.GetComponent<MapObjectView>();
		if (component3 != null)
		{
			EventBus.RaiseEvent((IMapObjectEntity)component3.Data, (Action<IInteractionObjectUIHandler>)delegate(IInteractionObjectUIHandler h)
			{
				h.HandleObjectInteract();
			}, isCheckRuntime: true);
		}
	}

	public void StopMovement()
	{
		m_LeftStickVector = Vector3.zero;
		UpdateLeftStickMovement();
		if (CutsceneLock.Active && Game.Instance.CurrentMode == GameModeType.Cutscene)
		{
			return;
		}
		try
		{
			SelectionManagerConsole.Instance.Or(null)?.Stop();
		}
		catch (Exception value)
		{
			System.Console.WriteLine(value);
		}
	}

	private void ResetStopMovementFlag()
	{
		SelectionManagerConsole selectionManagerConsole = SelectionManagerConsole.Instance.Or(null);
		if ((object)selectionManagerConsole != null)
		{
			selectionManagerConsole.StopMoveFlag = false;
		}
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		if (LayerBinded.Value)
		{
			Game.Instance.CursorController.SetAbilityCursor(ability.Icon);
			if (!base.CursorEnabled)
			{
				m_ForceCursorEnabled = true;
				SwitchCursorEnabled();
			}
		}
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		if (LayerBinded.Value)
		{
			Game.Instance.CursorController.ClearCursor();
			if (m_ForceCursorEnabled)
			{
				m_ForceCursorEnabled = false;
				SwitchCursorEnabled();
			}
		}
	}
}
