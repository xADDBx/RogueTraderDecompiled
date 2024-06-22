using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.VariativeInteraction;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Interaction;
using Kingmaker.Items;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.Traps.Simple;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.MapObject;

public class OvertipMapObjectVM : BaseOvertipMapObjectVM
{
	public readonly OvertipBarkBlockVM BarkBlockVM;

	public readonly ReactiveProperty<string> Name = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> ObjectDescription = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> ObjectSkillCheckText = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<Vector3> CameraDistance = new ReactiveProperty<Vector3>(Vector3.zero);

	private readonly IEnumerable<InteractionPart> m_Interactions;

	public readonly bool HasInteractions;

	public readonly bool HasInteractionsWithOvertip;

	public bool NotAvailable;

	private readonly float m_ProximityRadius;

	public readonly ReactiveProperty<bool> ForceHotKeyPressed = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> ForceHideInCombat = new ReactiveProperty<bool>();

	private readonly Transform m_Bone;

	private readonly bool m_IsInSpace;

	private bool m_IsLoot;

	private bool m_IsNotInCombat;

	public readonly ReactiveProperty<int?> HasResourceCount = new ReactiveProperty<int?>();

	public readonly ReactiveProperty<bool> CanInteract = new ReactiveProperty<bool>(initialValue: true);

	public int? RequiredResourceCount;

	public string ResourceName;

	public readonly ReactiveCommand InventoryChanged = new ReactiveCommand();

	private IDisposable m_SelectedUnitsSubscription;

	private List<BaseUnitEntity> m_TryApproachingUnits = new List<BaseUnitEntity>();

	private Dictionary<BaseUnitEntity, ForcedPath> m_CurrentlyApproachingUnits = new Dictionary<BaseUnitEntity, ForcedPath>();

	protected override bool UpdateEnabled => MapObjectEntity.IsVisibleForPlayer;

	public InteractionPart FirstInteractionPart => m_Interactions.FirstOrDefault();

	public bool ActiveCharacterIsNear { get; private set; }

	public bool IsInteract { get; private set; }

	public IReadOnlyReactiveProperty<bool> IsBarkActive => BarkBlockVM.IsBarkActive;

	public bool IsInCombat => Game.Instance.Player.IsInCombat;

	public UIInteractionType Type => FirstInteractionPart?.UIInteractionType ?? UIInteractionType.None;

	public bool ForceOnScreen
	{
		get
		{
			if (!IsBarkActive.Value)
			{
				return ActiveCharacterIsNear;
			}
			return true;
		}
	}

	public OvertipMapObjectVM(MapObjectEntity mapObjectEntity)
		: base(mapObjectEntity)
	{
		m_Bone = mapObjectEntity.View.ViewTransform.FindChildRecursive("UI_Overtip_Bone");
		m_IsInSpace = Game.Instance.CurrentMode == GameModeType.SpaceCombat || Game.Instance.CurrentMode == GameModeType.StarSystem || Game.Instance.CurrentMode == GameModeType.GlobalMap;
		m_Interactions = mapObjectEntity.Parts.GetAll<InteractionPart>();
		HasInteractions = m_Interactions.Any();
		HasInteractionsWithOvertip = m_Interactions.Any((InteractionPart i) => i.Settings?.ShowOvertip ?? false);
		m_ProximityRadius = (HasInteractionsWithOvertip ? Mathf.Max(FirstInteractionPart.ApproachRadius, 6.35f) : 6.35f);
		AddDisposable(BarkBlockVM = new OvertipBarkBlockVM());
		AddDisposable(CameraDistance.ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			VisibilityChanged.Execute();
		}));
		AddDisposable(IsBarkActive.ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			VisibilityChanged.Execute();
		}));
		AddDisposable(ForceHotKeyPressed.ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			VisibilityChanged.Execute();
		}));
		AddDisposable(ForceHideInCombat.ObserveLastValueOnLateUpdate().Subscribe(delegate
		{
			VisibilityChanged.Execute();
		}));
		UpdateObjectData();
		HighlightChanged();
	}

	protected override void DisposeImplementation()
	{
		m_SelectedUnitsSubscription?.Dispose();
	}

	protected override void OnUpdateHandler()
	{
		base.OnUpdateHandler();
		if (MapObjectEntity.IsVisibleForPlayer && HasInteractions)
		{
			UpdateUnitsNear();
			UpdateCanInteract();
		}
	}

	public void HighlightChanged()
	{
		MapObjectIsHighlited.Value = MapObjectEntity?.View.Highlighted ?? false;
	}

	public void ShowBark(string text)
	{
		BarkBlockVM.ShowBark(text);
	}

	public void HideBark()
	{
		BarkBlockVM.HideBark();
	}

	public void Interact()
	{
		if (!FirstInteractionPart)
		{
			return;
		}
		if (VariativeInteractionVM.HasVariativeInteraction(MapObjectEntity.View))
		{
			EventBus.RaiseEvent(delegate(IVariativeInteractionUIHandler h)
			{
				h.HandleInteractionRequest(MapObjectEntity.View);
			});
		}
		else
		{
			if (Game.Instance.TurnController.IsPreparationTurn)
			{
				return;
			}
			if (FirstInteractionPart.Type == InteractionType.Approach)
			{
				List<BaseUnitEntity> units = FirstInteractionPart.SelectAllUnits(Game.Instance.SelectionCharacter.SelectedUnits.ToList()).EmptyIfNull().ToList();
				TryApproachAndInteract(units);
			}
			else if (FirstInteractionPart.Type == InteractionType.Direct)
			{
				BaseUnitEntity baseUnitEntity = FirstInteractionPart.SelectUnit(Game.Instance.SelectionCharacter.SelectedUnits.ToList());
				ClickMapObjectHandler.ShowWarningIfNeeded(baseUnitEntity, FirstInteractionPart);
				if (baseUnitEntity != null && FirstInteractionPart.CanInteract())
				{
					UnitCommandsRunner.DirectInteract(baseUnitEntity, FirstInteractionPart);
				}
			}
			else
			{
				PFLog.UI.Error("Proximity interactions doesn't supported");
			}
		}
	}

	private void TryApproachAndInteract(List<BaseUnitEntity> units)
	{
		m_CurrentlyApproachingUnits.Clear();
		m_TryApproachingUnits.Clear();
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			BaseUnitEntity baseUnitEntity = units.FirstItem();
			if (baseUnitEntity != null)
			{
				FirstInteractionPart.SelectUnit(new List<BaseUnitEntity> { baseUnitEntity });
				if (!FirstInteractionPart.IsEnoughCloseForInteractionFromDesiredPosition(baseUnitEntity))
				{
					ClickMapObjectHandler.ShowWarningIfNeeded(baseUnitEntity, FirstInteractionPart);
				}
				else
				{
					UnitCommandsRunner.TryApproachAndInteract(baseUnitEntity, FirstInteractionPart);
				}
			}
			return;
		}
		foreach (BaseUnitEntity approachingUser in units)
		{
			if (approachingUser == null)
			{
				break;
			}
			m_TryApproachingUnits.Add(approachingUser);
			PathfindingService.Instance.FindPathRT(approachingUser.MovementAgent, FirstInteractionPart.Owner.Position, FirstInteractionPart.ApproachRadius, delegate(ForcedPath path)
			{
				if (path.error)
				{
					PFLog.Pathfinding.Error("An error path was returned. Ignoring");
					m_TryApproachingUnits.Remove(approachingUser);
				}
				else if (approachingUser.IsMovementLockedByGameModeOrCombat())
				{
					PFLog.Pathfinding.Log("Movement is locked due to GameMode or Combat. Ignoring");
					m_TryApproachingUnits.Remove(approachingUser);
				}
				else
				{
					m_CurrentlyApproachingUnits.Add(approachingUser, path);
					TryApproachAndInteract(approachingUser);
				}
			});
		}
	}

	private void TryApproachAndInteract(BaseUnitEntity u)
	{
		foreach (BaseUnitEntity tryApproachingUnit in m_TryApproachingUnits)
		{
			if (!m_CurrentlyApproachingUnits.TryGetValue(tryApproachingUnit, out var _))
			{
				return;
			}
		}
		List<BaseUnitEntity> list = new List<BaseUnitEntity>();
		foreach (KeyValuePair<BaseUnitEntity, ForcedPath> currentlyApproachingUnit in m_CurrentlyApproachingUnits)
		{
			currentlyApproachingUnit.Deconstruct(out var key, out var value2);
			BaseUnitEntity item = key;
			ForcedPath forcedPath = value2;
			InteractionPart firstInteractionPart = FirstInteractionPart;
			List<Vector3> vectorPath = forcedPath.vectorPath;
			if (firstInteractionPart.IsEnoughCloseForInteraction(vectorPath[vectorPath.Count - 1]))
			{
				list.Add(item);
			}
		}
		BaseUnitEntity unit = ((!list.Empty()) ? FirstInteractionPart.SelectUnit(list) : FirstInteractionPart.SelectUnit(Game.Instance.SelectionCharacter.SelectedUnits.ToList()));
		m_CurrentlyApproachingUnits.Clear();
		m_TryApproachingUnits.Clear();
		ClickMapObjectHandler.ShowWarningIfNeeded(unit, FirstInteractionPart);
		UnitCommandsRunner.TryApproachAndInteract(unit, FirstInteractionPart);
	}

	public void UpdateObjectData()
	{
		m_SelectedUnitsSubscription?.Dispose();
		if (MapObjectEntity.View == null || m_Interactions.Empty())
		{
			return;
		}
		OvertipVerticalCorrection = m_Interactions.FirstOrDefault()?.Settings.OvertipVerticalCorrection ?? 0f;
		bool value = false;
		InteractionSkillCheckPart skillCheck = m_Interactions.Select((InteractionPart interaction) => interaction as InteractionSkillCheckPart).FirstOrDefault();
		if (skillCheck != null && skillCheck.Enabled)
		{
			value = true;
			m_IsNotInCombat = skillCheck.Settings.NotInCombat;
			InteractionSkillCheckSettings settings = skillCheck.Settings;
			if (skillCheck.AlreadyUsed && settings.OnlyCheckOnce)
			{
				ObjectDescription.Value = (skillCheck.CheckPassed ? GetString(settings.ShortDescriptionPassed, string.Empty) : GetString(settings.ShortDescriptionFailed, string.Empty));
				ObjectSkillCheckText.Value = string.Empty;
				Name.Value = GetString(settings.DisplayNameAfterUse, string.Empty);
			}
			else
			{
				Name.Value = GetString(settings.DisplayName, string.Empty);
				ObjectDescription.Value = GetString(settings.ShortDescription, string.Empty);
				ObjectSkillCheckText.Value = UIUtility.GetOvertipSkillCheckText(skillCheck, Game.Instance.SelectionCharacter.SelectedUnits.ToList(), out var needChance);
				if (needChance)
				{
					m_SelectedUnitsSubscription = UniRxExtensionMethods.Subscribe(Game.Instance.SelectionCharacter.ActualGroupUpdated, delegate
					{
						ObjectSkillCheckText.Value = UIUtility.GetOvertipSkillCheckText(skillCheck, Game.Instance.SelectionCharacter.SelectedUnits.ToList(), out needChance);
					});
				}
			}
			IInteractionVariantActor interactionVariantActor = MapObjectEntity.GetAll<IInteractionVariantActor>().FirstOrDefault();
			if (interactionVariantActor != null)
			{
				BlueprintItem item = interactionVariantActor.RequiredItem;
				if (item != null)
				{
					RequiredResourceCount = interactionVariantActor.RequiredItemsCount;
					UpdateResourceCount(item);
					AddDisposable(ObservableExtensions.Subscribe(InventoryChanged, delegate
					{
						UpdateResourceCount(item);
					}));
					ResourceName = GetItemName(item);
				}
			}
		}
		DisableTrapInteractionPart trap = m_Interactions.Select((InteractionPart interaction) => interaction as DisableTrapInteractionPart).FirstOrDefault();
		if (trap != null && trap.Enabled)
		{
			value = true;
			Name.Value = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.Trap;
			if (trap.Owner.TrapActive)
			{
				ObjectDescription.Value = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.TrapNeutralize;
				Entry entry = BlueprintRoot.Instance.LocalizedTexts.Stats.Entries.FirstOrDefault((Entry k) => k.Stat == (trap.View as SimpleTrapObjectView)?.Info.DisarmSkill);
				if (entry != null)
				{
					ObjectSkillCheckText.Value = "[" + entry.Text.Text + "]";
				}
			}
			else
			{
				ObjectDescription.Value = string.Empty;
				ObjectSkillCheckText.Value = string.Empty;
			}
		}
		InteractionDoorPart interactionDoorPart = m_Interactions.Select((InteractionPart interaction) => interaction as InteractionDoorPart).FirstOrDefault();
		if (interactionDoorPart != null && interactionDoorPart.Enabled)
		{
			value = true;
			m_IsNotInCombat = interactionDoorPart.Settings.NotInCombat;
			Name.Value = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.Door;
			ObjectDescription.Value = (interactionDoorPart.GetState() ? Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.DoorClose : Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.DoorOpen);
			ObjectSkillCheckText.Value = string.Empty;
		}
		InteractionLootPart interactionLootPart = m_Interactions.Select((InteractionPart interaction) => interaction as InteractionLootPart).FirstOrDefault();
		if (interactionLootPart != null && interactionLootPart.Enabled)
		{
			value = true;
			m_IsNotInCombat = interactionLootPart.Settings.NotInCombat;
			m_IsLoot = true;
			Name.Value = ((!StringUtility.IsNullOrInvisible(interactionLootPart.GetName())) ? interactionLootPart.GetName() : UIStrings.Instance.LootWindow.GetLootName(interactionLootPart.Settings.LootContainerType));
		}
		InteractionBarkPart interactionBarkPart = m_Interactions.Select((InteractionPart interaction) => interaction as InteractionBarkPart).FirstOrDefault();
		if (interactionBarkPart != null && interactionBarkPart.Enabled)
		{
			m_IsNotInCombat = interactionBarkPart.Settings.NotInCombat;
			value = true;
		}
		InteractionDialogPart interactionDialogPart = m_Interactions.Select((InteractionPart interaction) => interaction as InteractionDialogPart).FirstOrDefault();
		if (interactionDialogPart != null && interactionDialogPart.Enabled)
		{
			m_IsNotInCombat = interactionDialogPart.Settings.NotInCombat;
			value = true;
		}
		InteractionStairsPart interactionStairsPart = m_Interactions.Select((InteractionPart interaction) => interaction as InteractionStairsPart).FirstOrDefault();
		if (interactionStairsPart != null && interactionStairsPart.Enabled)
		{
			m_IsNotInCombat = interactionStairsPart.Settings.NotInCombat;
			Name.Value = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.Ladder;
			value = Game.Instance.IsControllerGamepad;
		}
		UpdateCanInteract();
		IsEnabled.Value = value;
		void UpdateResourceCount(BlueprintItem blueprintItem)
		{
			HasResourceCount.Value = Game.Instance.Player.Inventory.Items.Where((ItemEntity i) => i.Blueprint == blueprintItem).Sum((ItemEntity i) => i.Count);
			if (RequiredResourceCount.HasValue)
			{
				CanInteract.Value = HasResourceCount.Value >= RequiredResourceCount;
			}
		}
	}

	private static string GetItemName(BlueprintItem item)
	{
		if (item == Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MultikeyItem)
		{
			return item.Name;
		}
		if (item == Game.Instance.BlueprintRoot.SystemMechanics.Consumables.MeltaChargeItem)
		{
			return item.Name;
		}
		if (item == Game.Instance.BlueprintRoot.SystemMechanics.Consumables.RitualSetItem)
		{
			return item.Name;
		}
		return UIStrings.Instance.Overtips.NeedUnknownKey.Text;
	}

	private void UpdateCanInteract()
	{
		NotAvailable = HasInteractionsWithOvertip && !FirstInteractionPart.CanInteract();
		CanInteract.Value = ClickMapObjectHandler.HasAvailableInteractions(MapObjectEntity.View.GO) && (!RequiredResourceCount.HasValue || HasResourceCount.Value >= RequiredResourceCount);
	}

	public void UpdateInteraction(bool active)
	{
		IsInteract = active;
	}

	private void UpdateUnitsNear()
	{
		ActiveCharacterIsNear = false;
		MapObjectEntity mapObjectEntity = MapObjectEntity;
		if (mapObjectEntity == null || !mapObjectEntity.IsInCameraFrustum)
		{
			return;
		}
		foreach (BaseUnitEntity selectedUnit in Game.Instance.SelectionCharacter.SelectedUnits)
		{
			if ((MapObjectEntity.Position - selectedUnit.Position).sqrMagnitude <= m_ProximityRadius * m_ProximityRadius)
			{
				ActiveCharacterIsNear = true;
				break;
			}
		}
		if (MapObjectEntity.IsVisibleForPlayer)
		{
			CameraDistance.Value = Position.Value - CameraRig.Instance.GetTargetPointPosition();
		}
	}

	private string GetString(SharedStringAsset stringAsset, string def = null)
	{
		if (stringAsset == null)
		{
			return def;
		}
		if (!StringUtility.IsNullOrInvisible(stringAsset.String))
		{
			return stringAsset.String;
		}
		return def;
	}

	protected override Vector3 GetEntityPosition()
	{
		if (MapObjectEntity?.View != null && !m_IsInSpace)
		{
			m_EntityPosition = MapObjectEntity.Position;
		}
		else if (MapObjectEntity?.View != null && m_IsInSpace)
		{
			m_EntityPosition = ((m_Bone != null) ? m_Bone.transform.position : MapObjectEntity.Position);
		}
		else
		{
			m_EntityPosition = Vector3.zero;
		}
		return m_EntityPosition;
	}

	public void HandleHighlightChange(bool isOn)
	{
		ForceHotKeyPressed.Value = isOn;
	}

	public void HandleCombatStateChanged()
	{
		if (HasInteractions)
		{
			ForceHideInCombat.Value = TurnController.IsInTurnBasedCombat() && FirstInteractionPart.Settings.NotInCombat;
			UpdateCanInteract();
		}
	}

	public static bool CheckNeedOvertip(MapObjectEntity mapObject)
	{
		InteractionSkillCheckSettings interactionSkillCheckSettings = mapObject.GetOptional<InteractionSkillCheckPart>()?.Settings;
		if (interactionSkillCheckSettings != null && !interactionSkillCheckSettings.ShowOvertip && interactionSkillCheckSettings.DisplayName == null && interactionSkillCheckSettings.DisplayNameAfterUse == null && interactionSkillCheckSettings.CheckFailedActions == null && interactionSkillCheckSettings.CheckPassedActions == null && interactionSkillCheckSettings.CheckFailBark == null && interactionSkillCheckSettings.CheckPassedBark == null)
		{
			return false;
		}
		if (mapObject.View.GetComponent<AreaTransition>() != null)
		{
			return false;
		}
		return !(mapObject is DestructibleEntity);
	}
}
