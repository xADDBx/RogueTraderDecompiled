using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.InputSystems;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.UniRx;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class UnitPredictionManager : MonoBehaviour, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IPartyCombatHandler, IAbilityTargetSelectionUIHandler, IUnitDirectHoverUIHandler, ITurnStartHandler, IContinueTurnHandler, IInterruptTurnStartHandler, IUnitPathManagerHandler, IUnitMovableAreaHandler, IAbilityTargetHoverUIHandler, IAbilityTargetMarkerHoverUIHandler, IInterruptTurnContinueHandler
{
	[SerializeField]
	private float m_LoSinHoverDelayTime = 1f;

	[SerializeField]
	private float m_AooUpdateDelayTime = 0.15f;

	private UnitHologram m_VirtualHologram;

	private UnitHologram m_MovementHologram;

	private Vector3? m_VirtualHologramPosition;

	private Vector3? m_VirtualHologramDirection;

	private Vector3? m_VirtualHoverPosition;

	private bool m_ShowHoverPosition;

	private IDisposable m_DelayedUpdateLos;

	private IDisposable m_DelayedUpdateAoo;

	private List<Vector3> m_MovableAreaPositions = new List<Vector3>();

	private bool m_IsCtrlHold;

	private Vector3? m_AbilityCasterPosition;

	private Vector3? m_AbilityTargetPosition;

	private AbilityData m_SelectedAbility;

	private AbilityData m_HoveredAbility;

	private AbstractUnitEntity m_HoveredUnit;

	private readonly Dictionary<BaseUnitEntity, GameObject> m_AttackOfOpportunityEffects = new Dictionary<BaseUnitEntity, GameObject>();

	private BaseUnitEntity m_PathUnitEntity;

	private Path m_UnitPath;

	private CameraRig m_CameraRig;

	private bool m_IsCameraRotation;

	private float m_BluePointsCost;

	public static UnitPredictionManager Instance { get; private set; }

	public static Vector3? RealHologramPosition { get; private set; }

	private bool HasAbility => m_SelectedAbility != null;

	private Path UnitPath
	{
		get
		{
			return m_UnitPath;
		}
		set
		{
			m_UnitPath?.Release(this);
			m_UnitPath = value;
			m_UnitPath?.Claim(this);
		}
	}

	[CanBeNull]
	private BaseUnitEntity CurrentUnit
	{
		get
		{
			object obj = m_SelectedAbility?.Caster as BaseUnitEntity;
			if (obj == null)
			{
				if (!(m_VirtualHologram != null))
				{
					return Game.Instance.TurnController.CurrentUnit as BaseUnitEntity;
				}
				obj = m_VirtualHologram.Parent;
			}
			return (BaseUnitEntity)obj;
		}
	}

	public float BluePointsCost => m_BluePointsCost;

	public Vector3 CurrentUnitDirection => m_VirtualHologramDirection ?? CurrentUnit?.Forward ?? Vector3.forward;

	private void Start()
	{
		m_CameraRig = UnityEngine.Object.FindObjectOfType<CameraRig>();
	}

	private void OnEnable()
	{
		Instance = this;
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		ClearAll();
		Instance = null;
		EventBus.Unsubscribe(this);
	}

	private void LateUpdate()
	{
		bool flag = KeyboardAccess.IsCtrlHold();
		if (m_IsCtrlHold != flag)
		{
			m_IsCtrlHold = flag;
			ShowHoverPosition();
		}
		bool flag2 = m_CameraRig?.RotationByMouse ?? false;
		if (m_IsCameraRotation != flag2)
		{
			m_IsCameraRotation = flag2;
			DelayedShowHoverPosition();
		}
	}

	public void SetVirtualHoverPosition(Vector3 position)
	{
		m_VirtualHoverPosition = position;
		if (m_IsCtrlHold)
		{
			ShowHoverPosition();
		}
		else
		{
			DelayedShowHoverPosition();
		}
	}

	private void ShowHoverPosition()
	{
		m_DelayedUpdateLos?.Dispose();
		m_ShowHoverPosition = true;
		UpdateVirtualLoSPosition();
	}

	private void DelayedShowHoverPosition()
	{
		m_ShowHoverPosition = false;
		UpdateVirtualLoSPosition();
		m_DelayedUpdateLos?.Dispose();
		m_DelayedUpdateLos = DelayedInvoker.InvokeInTime(delegate
		{
			m_ShowHoverPosition = true;
			UpdateVirtualLoSPosition();
		}, m_LoSinHoverDelayTime);
	}

	public void ResetVirtualHoverPosition()
	{
		m_VirtualHoverPosition = null;
		m_ShowHoverPosition = false;
		m_DelayedUpdateLos?.Dispose();
		UpdateVirtualLoSPosition();
	}

	public void SetMovementHologramPosition(BaseUnitEntity unit, Vector3 position, Vector3 direction)
	{
		m_VirtualHologramPosition = position;
		direction.y = 0f;
		m_VirtualHologramDirection = direction;
		CreateHologramIfNeeded(unit, allowSecondHologram: false, isMovementHologram: true);
		UpdateVirtualHologramPosition();
		UpdateVirtualLoSPosition();
	}

	public void ClearHologramPosition(BaseUnitEntity unit)
	{
		if (m_VirtualHologramPosition.HasValue && !(m_VirtualHologram == null) && m_VirtualHologram.Parent == unit)
		{
			m_VirtualHologramPosition = null;
			m_VirtualHologramDirection = null;
			UpdateVirtualHologramPosition();
		}
	}

	public void HandleSetUnitMovableArea(List<GraphNode> nodes)
	{
		if (nodes != null)
		{
			m_MovableAreaPositions = nodes.Select((GraphNode node) => node.Vector3Position).ToList();
		}
		else
		{
			ClearUnitMovableArea();
		}
		UpdateVirtualLoSPosition();
	}

	public void HandleRemoveUnitMovableArea()
	{
		ClearUnitMovableArea();
	}

	private void ClearUnitMovableArea()
	{
		m_MovableAreaPositions = new List<Vector3>();
	}

	public void SetAbilityPositions(Vector3 casterPosition, Vector3 targetPosition)
	{
		m_AbilityCasterPosition = casterPosition;
		m_AbilityTargetPosition = targetPosition;
		CreateHologramIfNeeded(CurrentUnit);
		UpdateVirtualHologramPosition();
	}

	private void ClearAbilityArea()
	{
		m_SelectedAbility = null;
		m_AbilityCasterPosition = null;
		m_AbilityTargetPosition = null;
	}

	private Vector3? GetBestShootingPosition()
	{
		Vector3 position;
		if (m_HoveredUnit != null && m_SelectedAbility.CanTargetFromDesiredPosition(m_HoveredUnit))
		{
			position = m_SelectedAbility.GetBestShootingPositionForDesiredPosition(m_HoveredUnit).Vector3Position;
		}
		else
		{
			if (!m_AbilityCasterPosition.HasValue)
			{
				return null;
			}
			position = m_AbilityCasterPosition.Value;
		}
		Vector3 desiredPosition = Game.Instance.VirtualPositionController.GetDesiredPosition(m_SelectedAbility.Caster);
		CustomGridNodeBase nearestNodeXZUnwalkable = position.GetNearestNodeXZUnwalkable();
		CustomGridNodeBase nearestNodeXZUnwalkable2 = desiredPosition.GetNearestNodeXZUnwalkable();
		if (m_SelectedAbility.Caster.IsUnitPositionContainsNode(desiredPosition, nearestNodeXZUnwalkable))
		{
			return nearestNodeXZUnwalkable2?.Vector3Position;
		}
		if (nearestNodeXZUnwalkable == nearestNodeXZUnwalkable2)
		{
			return null;
		}
		return nearestNodeXZUnwalkable?.Vector3Position;
	}

	private void UpdateVirtualHologramPosition()
	{
		if (!(m_VirtualHologram == null))
		{
			Vector3? position = m_VirtualHologramPosition ?? (HasAbility ? GetBestShootingPosition() : null);
			Vector3? vector = null;
			Vector3? vector2 = m_AbilityTargetPosition ?? m_HoveredUnit?.Position;
			bool flag = m_SelectedAbility?.Blueprint.ShouldTurnToTarget ?? false;
			if (position.HasValue && vector2.HasValue && flag)
			{
				Vector3 normalized = (vector2.Value - position.Value).normalized;
				normalized.y = 0f;
				vector = normalized;
			}
			Vector3? vector3 = vector;
			if (!vector3.HasValue)
			{
				vector = m_VirtualHologramDirection;
			}
			if (position.HasValue)
			{
				UpdateHologram(m_VirtualHologram, position, vector);
			}
			else
			{
				ClearHologram();
			}
		}
	}

	private void UpdateVirtualLoSPosition()
	{
		if (Game.Instance.VirtualPositionController == null)
		{
			return;
		}
		if (!HasAbility && m_ShowHoverPosition && !m_IsCameraRotation && m_VirtualHoverPosition.HasValue && ((m_MovableAreaPositions.Contains(m_VirtualHoverPosition.Value) && !m_VirtualHologramPosition.HasValue) || m_IsCtrlHold))
		{
			Game.Instance.VirtualPositionController.VirtualPosition = m_VirtualHoverPosition.Value;
			return;
		}
		Game.Instance.VirtualPositionController.VirtualPositionUnit = m_SelectedAbility?.Caster;
		if (m_VirtualHologramPosition.HasValue)
		{
			Game.Instance.VirtualPositionController.VirtualPosition = m_VirtualHologramPosition.Value;
			if (m_VirtualHologramDirection.HasValue)
			{
				Game.Instance.VirtualPositionController.VirtualRotation = m_VirtualHologramDirection.Value;
			}
		}
		else
		{
			Game.Instance.VirtualPositionController.CleanVirtualPosition();
		}
	}

	private void CreateHologramIfNeeded([CanBeNull] BaseUnitEntity unit, bool allowSecondHologram = false, bool isMovementHologram = false)
	{
		if (unit == null || !unit.Faction.IsPlayer)
		{
			return;
		}
		if (m_SelectedAbility == null && m_HoveredAbility == null)
		{
			m_AbilityCasterPosition = null;
			m_AbilityTargetPosition = null;
		}
		if ((!m_VirtualHologramPosition.HasValue && m_AbilityCasterPosition.HasValue && unit.GetOccupiedNodes().Contains(m_AbilityCasterPosition.Value.GetNearestNodeXZ())) || (!(m_VirtualHologram == null) && m_VirtualHologram.Parent == unit && !((m_MovementHologram == m_VirtualHologram) ? allowSecondHologram : isMovementHologram)))
		{
			return;
		}
		if (m_MovementHologram != null && m_MovementHologram != m_VirtualHologram && isMovementHologram)
		{
			UnityEngine.Object.Destroy(m_MovementHologram.gameObject);
			m_MovementHologram = null;
		}
		if (m_VirtualHologram != null)
		{
			if (m_VirtualHologram == m_MovementHologram && allowSecondHologram)
			{
				m_VirtualHologram = null;
			}
			else
			{
				UnityEngine.Object.Destroy(m_VirtualHologram.gameObject);
				m_VirtualHologram = null;
			}
		}
		if (!Game.Instance.TurnController.TurnBasedModeActive || (m_VirtualHologramPosition.HasValue && !WarhammerBlockManager.Instance.CanUnitStandOnNode(unit, m_VirtualHologramPosition.Value.GetNearestNodeXZUnwalkable())))
		{
			return;
		}
		StarshipView componentInChildren = unit.View.GetComponentInChildren<StarshipView>();
		using (ContextData<UnitHelper.UnitHologram>.Request())
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				m_VirtualHologram = (componentInChildren ? unit.CreateHologramSpaceship() : unit.CreateHologram());
			}
		}
		if (isMovementHologram)
		{
			m_MovementHologram = m_VirtualHologram;
		}
	}

	private static void UpdateHologram(UnitHologram hologram, Vector3? position, Vector3? direction)
	{
		hologram.SetupShading(FxRoot.Instance.Hologram.MainFx);
		if (!position.HasValue)
		{
			return;
		}
		RealHologramPosition = position;
		hologram.gameObject.transform.position = position.Value + SizePathfindingHelper.GetSizePositionOffset(hologram.Parent.SizeRect, direction ?? Vector3.forward);
		hologram.CoverType = LosCalculations.GetCoverType(position.Value);
		if (!direction.HasValue || direction == Vector3.zero)
		{
			BaseUnitEntity baseUnitEntity = (from u in Game.Instance.State.AllBaseUnits
				where u.Faction.IsPlayerEnemy && u.IsInCombat
				select u into i
				orderby (position.Value - i.Position).sqrMagnitude
				select i).FirstOrDefault();
			if (baseUnitEntity != null)
			{
				hologram.LookAt(baseUnitEntity);
			}
		}
		else
		{
			hologram.Direction = direction.Value;
		}
	}

	public void ClearMovementHologram(BaseUnitEntity unit)
	{
		if (m_MovementHologram != null && m_MovementHologram != m_VirtualHologram && m_MovementHologram.Parent == unit)
		{
			UnityEngine.Object.Destroy(m_MovementHologram.gameObject);
			m_MovementHologram = null;
		}
		if (!(m_VirtualHologram == null) && m_VirtualHologram.Parent == unit)
		{
			ClearHologram();
		}
	}

	private void ClearExtraHologramOnly()
	{
		if (m_VirtualHologram != null && m_MovementHologram == null)
		{
			ClearHologram();
		}
	}

	private void ClearHologram()
	{
		if (m_MovementHologram != null && m_MovementHologram != m_VirtualHologram)
		{
			UnityEngine.Object.Destroy(m_MovementHologram.gameObject);
		}
		if (m_VirtualHologram != null)
		{
			UnityEngine.Object.Destroy(m_VirtualHologram.gameObject);
		}
		m_VirtualHologram = null;
		m_MovementHologram = null;
		m_VirtualHologramPosition = null;
		m_VirtualHologramDirection = null;
		RealHologramPosition = null;
		ClearMoveCost();
	}

	public void ClearAll()
	{
		ClearHologram();
		ClearAttackOfOpportunityPrediction();
		Game.Instance.VirtualPositionController?.CleanVirtualPosition();
		m_VirtualHoverPosition = null;
	}

	public void SetMoveCost(float cost)
	{
		m_BluePointsCost = cost;
	}

	private void ClearMoveCost()
	{
		m_BluePointsCost = 0f;
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_SelectedAbility = ability;
		UpdateSecondHologramForSelectedAbility();
		UpdateVirtualLoSPosition();
	}

	public void UpdateSecondHologramForSelectedAbility()
	{
		if (!(m_SelectedAbility == null) && m_SelectedAbility.TryGetCasterDesiredPositionAndDirection(out var position, out var direction))
		{
			m_VirtualHologramPosition = position;
			m_VirtualHologramDirection = direction;
			CreateHologramIfNeeded(CurrentUnit, allowSecondHologram: true);
			UpdateVirtualHologramPosition();
		}
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		if (!(m_SelectedAbility == null))
		{
			ClearAbilityArea();
			ClearExtraHologramOnly();
			UpdateVirtualHologramPosition();
		}
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover)
	{
		m_HoveredUnit = (isHover ? unitEntityView.Data : null);
		if (m_HoveredUnit != null && m_SelectedAbility != null)
		{
			CreateHologramIfNeeded(CurrentUnit);
		}
		UpdateVirtualHologramPosition();
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if ((command.Executor == CurrentUnit || command.Executor == Game.Instance.TurnController.CurrentUnit) && CheckUnitCommand(command))
		{
			ClearAll();
		}
	}

	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		ClearAll();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		HandleUnitStartTurnInternal();
	}

	public void HandleUnitContinueTurn(bool isTurnBased)
	{
		HandleUnitStartTurnInternal();
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleUnitStartTurnInternal();
	}

	void IInterruptTurnContinueHandler.HandleUnitContinueInterruptTurn()
	{
		HandleUnitStartTurnInternal();
	}

	private bool CheckUnitCommand(AbstractUnitCommand command)
	{
		if (!(command is UnitUseAbility))
		{
			return command is UnitAttackOfOpportunity;
		}
		return true;
	}

	private void HandleUnitStartTurnInternal()
	{
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			ClearAll();
		}
	}

	public void HandlePathAdded(Path path, float cost)
	{
		m_PathUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		UnitPath = path;
		SetMoveCost(cost);
		DelayedUpdateAttackOfOpportunityPrediction();
	}

	public void HandlePathRemoved()
	{
		m_PathUnitEntity = null;
		UnitPath = null;
		DelayedUpdateAttackOfOpportunityPrediction();
	}

	public void HandleCurrentNodeChanged(float cost)
	{
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		HandleAbilityTargetHoverInternal(ability, hover);
	}

	public void HandleAbilityTargetMarkerHover(AbilityData ability, bool hover)
	{
		HandleAbilityTargetHoverInternal(ability, hover);
	}

	private void HandleAbilityTargetHoverInternal(AbilityData ability, bool hover)
	{
		m_HoveredAbility = (hover ? ability : null);
		DelayedUpdateAttackOfOpportunityPrediction();
	}

	private void DelayedUpdateAttackOfOpportunityPrediction()
	{
		m_DelayedUpdateAoo?.Dispose();
		m_DelayedUpdateAoo = DelayedInvoker.InvokeInTime(delegate
		{
			UpdateAttackOfOpportunityPrediction();
		}, m_AooUpdateDelayTime);
	}

	private void UpdateAttackOfOpportunityPrediction()
	{
		if (UnitPath == null && m_HoveredAbility == null && m_SelectedAbility == null)
		{
			ClearAttackOfOpportunityPrediction();
			return;
		}
		AbilityData abilityData = m_HoveredAbility ?? m_SelectedAbility;
		IEnumerable<BaseUnitEntity> enumerable = ((abilityData != null) ? ((BaseUnitEntity)abilityData.Caster).CalculateAttackOfOpportunity(abilityData) : m_PathUnitEntity.CalculateAttackOfOpportunity(UnitPath)).Select((AttackOfOpportunityData attack) => attack.Attacker);
		foreach (BaseUnitEntity item in enumerable)
		{
			if (!m_AttackOfOpportunityEffects.ContainsKey(item))
			{
				GameObject value = FxHelper.SpawnFxOnEntity(BlueprintRoot.Instance.FxRoot.FXAttackOfOpportunity?.Load(), item.View);
				m_AttackOfOpportunityEffects[item] = value;
			}
		}
		foreach (BaseUnitEntity item2 in m_AttackOfOpportunityEffects.Keys.Except(enumerable).ToList())
		{
			FxHelper.Destroy(m_AttackOfOpportunityEffects[item2]);
			m_AttackOfOpportunityEffects.Remove(item2);
		}
	}

	private void ClearAttackOfOpportunityPrediction()
	{
		m_AttackOfOpportunityEffects.Values.ForEach(delegate(GameObject fx)
		{
			FxHelper.Destroy(fx);
		});
		m_AttackOfOpportunityEffects.Clear();
	}
}
