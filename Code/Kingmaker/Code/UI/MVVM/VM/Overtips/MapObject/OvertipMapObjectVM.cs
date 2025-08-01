using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Overtips.CommonOvertipParts;
using Kingmaker.Code.UI.MVVM.VM.VariativeInteraction;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
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
using Kingmaker.Twitch;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionComponentBase;
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

	public readonly ReactiveProperty<int?> HasResourceCount = new ReactiveProperty<int?>();

	public readonly ReactiveProperty<bool> CanInteract = new ReactiveProperty<bool>(initialValue: true);

	public readonly ReactiveProperty<TwitchDropsLinkStatus> TwitchStatus = new ReactiveProperty<TwitchDropsLinkStatus>();

	public int? RequiredResourceCount;

	public string ResourceName;

	public readonly ReactiveCommand InventoryChanged = new ReactiveCommand();

	private IDisposable m_SelectedUnitsSubscription;

	protected override bool UpdateEnabled => MapObjectEntity.IsVisibleForPlayer;

	public bool IsTwitchDrops { get; private set; }

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
		InteractionTwitchDropsPart interactionTwitchDropsPart = m_Interactions.Select((InteractionPart interaction) => interaction as InteractionTwitchDropsPart).FirstOrDefault();
		IsTwitchDrops = interactionTwitchDropsPart != null;
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
				BaseUnitEntity[] units = FirstInteractionPart.SelectAllUnits(Game.Instance.SelectionCharacter.SelectedUnits.ToList()).ToArray();
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

	private void TryApproachAndInteract(BaseUnitEntity[] units)
	{
		InteractionPart firstInteractionPart = FirstInteractionPart;
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			TryApproachAndInteractTB(units, firstInteractionPart);
		}
		else
		{
			TryApproachAndInteractRT(units, firstInteractionPart);
		}
	}

	private static void TryApproachAndInteractTB(BaseUnitEntity[] units, InteractionPart interactionPart)
	{
		BaseUnitEntity baseUnitEntity = units.FirstItem();
		if (baseUnitEntity != null)
		{
			ClickMapObjectHandler.ShowWarningIfNeeded(baseUnitEntity, interactionPart);
			UnitCommandsRunner.TryApproachAndInteract(baseUnitEntity, interactionPart);
		}
	}

	private static async void TryApproachAndInteractRT(BaseUnitEntity[] units, InteractionPart interactionPart)
	{
		Task<ForcedPath>[] findPathTasks = units.Select((BaseUnitEntity i) => PathfindingService.Instance.FindPathRTAsync(i.MovementAgent, interactionPart.Owner.Position, interactionPart.ApproachRadius)).ToArray();
		await Task.WhenAll(findPathTasks);
		using PooledList<BaseUnitEntity> pooledList = PooledList<BaseUnitEntity>.Get();
		for (int j = 0; j < units.Length; j++)
		{
			BaseUnitEntity baseUnitEntity = units[j];
			ForcedPath result = findPathTasks[j].Result;
			if (!result.error && !baseUnitEntity.IsMovementLockedByGameModeOrCombat())
			{
				InteractionPart interactionPart2 = interactionPart;
				List<Vector3> vectorPath = result.vectorPath;
				if (interactionPart2.IsEnoughCloseForInteraction(vectorPath[vectorPath.Count - 1]))
				{
					pooledList.Add(baseUnitEntity);
				}
			}
		}
		BaseUnitEntity unit = ((!pooledList.Empty()) ? interactionPart.SelectUnit(pooledList) : interactionPart.SelectUnit(units));
		ClickMapObjectHandler.ShowWarningIfNeeded(unit, interactionPart);
		UnitCommandsRunner.TryApproachAndInteract(unit, interactionPart);
	}

	public void InteractTwitchDrops()
	{
		if (TwitchStatus.Value == TwitchDropsLinkStatus.ConnectionError)
		{
			CheckTwitchLinkedStatus();
		}
		else if (TwitchStatus.Value != TwitchDropsLinkStatus.Linked)
		{
			TwitchDropsManager.Instance.OpenTwitchLinkPage();
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(LocalizedTexts.Instance.Reasons.RedirectionToWeb, addToLog: false, WarningNotificationFormat.Attention);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ITwitchDropsRewardsUIHandler h)
			{
				h.HandleItemRewardsShow();
			});
		}
	}

	public async void CheckTwitchLinkedStatus()
	{
		ReactiveProperty<TwitchDropsLinkStatus> twitchStatus = TwitchStatus;
		twitchStatus.Value = await TwitchDropsManager.Instance.GetTwitchLinkedStatus();
		SetTwitchHint(TwitchStatus.Value);
	}

	private void SetTwitchHint(TwitchDropsLinkStatus status)
	{
		if (status == TwitchDropsLinkStatus.ConnectionError)
		{
			Name.Value = LocalizedTexts.Instance.Reasons.NoInternetConnection;
			return;
		}
		UIPopupWindows popUps = UIStrings.Instance.PopUps;
		Name.Value = ((status == TwitchDropsLinkStatus.Linked) ? popUps.GetRewards : popUps.LinkAccount);
	}

	public void UpdateObjectData()
	{
		m_SelectedUnitsSubscription?.Dispose();
		if (MapObjectEntity.View == null || m_Interactions.Empty())
		{
			return;
		}
		InteractionPart interactionPart = m_Interactions.FirstOrDefault();
		if (interactionPart == null)
		{
			OvertipVerticalCorrection = 0f;
			return;
		}
		OvertipVerticalCorrection = interactionPart.Settings.OvertipVerticalCorrection;
		bool flag = interactionPart.Enabled;
		if (flag)
		{
			InteractionSkillCheckPart interactionSkillCheckPart = interactionPart as InteractionSkillCheckPart;
			if (interactionSkillCheckPart == null)
			{
				DisableTrapInteractionPart disableTrapInteractionPart = interactionPart as DisableTrapInteractionPart;
				if (disableTrapInteractionPart == null)
				{
					if (!(interactionPart is InteractionDoorPart interactionDoorPart))
					{
						if (!(interactionPart is InteractionLootPart interactionLootPart))
						{
							if (!(interactionPart is InteractionStairsPart))
							{
								if (!(interactionPart is InteractionTwitchDropsPart))
								{
									if (interactionPart is InteractionActionPart interactionActionPart)
									{
										Name.Value = GetString(interactionActionPart.Settings.DisplayName, string.Empty);
									}
								}
								else
								{
									IsTwitchDrops = true;
									Name.Value = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.Trap;
								}
							}
							else
							{
								Name.Value = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.Ladder;
								flag = Game.Instance.IsControllerGamepad;
							}
						}
						else
						{
							m_IsLoot = true;
							Name.Value = ((!StringUtility.IsNullOrInvisible(interactionLootPart.GetName())) ? interactionLootPart.GetName() : UIStrings.Instance.LootWindow.GetLootName(interactionLootPart.Settings.LootContainerType));
						}
					}
					else
					{
						Name.Value = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.Door;
						ObjectDescription.Value = (interactionDoorPart.GetState() ? Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.DoorClose : Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.DoorOpen);
						ObjectSkillCheckText.Value = string.Empty;
					}
				}
				else
				{
					Name.Value = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.Trap;
					if (disableTrapInteractionPart.Owner.TrapActive)
					{
						ObjectDescription.Value = Game.Instance.BlueprintRoot.LocalizedTexts.UserInterfacesText.Tooltips.TrapNeutralize;
						ObjectSkillCheckText.Value = UIUtility.GetTrapSkillCheckText(disableTrapInteractionPart, Game.Instance.SelectionCharacter.SelectedUnits.ToList());
						m_SelectedUnitsSubscription = UniRxExtensionMethods.Subscribe(Game.Instance.SelectionCharacter.ActualGroupUpdated, delegate
						{
							ObjectSkillCheckText.Value = UIUtility.GetTrapSkillCheckText(disableTrapInteractionPart, Game.Instance.SelectionCharacter.SelectedUnits.ToList());
						});
					}
					else
					{
						ObjectDescription.Value = string.Empty;
						ObjectSkillCheckText.Value = string.Empty;
					}
				}
			}
			else
			{
				InteractionSkillCheckSettings settings = interactionSkillCheckPart.Settings;
				if (interactionSkillCheckPart.AlreadyUsed && settings.OnlyCheckOnce)
				{
					ObjectDescription.Value = (interactionSkillCheckPart.CheckPassed ? GetString(settings.ShortDescriptionPassed, string.Empty) : GetString(settings.ShortDescriptionFailed, string.Empty));
					ObjectSkillCheckText.Value = string.Empty;
					Name.Value = GetString(settings.DisplayNameAfterUse, string.Empty);
				}
				else
				{
					Name.Value = GetString(settings.DisplayName, string.Empty);
					ObjectDescription.Value = GetString(settings.ShortDescription, string.Empty);
					ObjectSkillCheckText.Value = UIUtility.GetOvertipSkillCheckText(interactionSkillCheckPart, Game.Instance.SelectionCharacter.SelectedUnits.ToList(), out var needChance);
					if (needChance)
					{
						m_SelectedUnitsSubscription = UniRxExtensionMethods.Subscribe(Game.Instance.SelectionCharacter.ActualGroupUpdated, delegate
						{
							ObjectSkillCheckText.Value = UIUtility.GetOvertipSkillCheckText(interactionSkillCheckPart, Game.Instance.SelectionCharacter.SelectedUnits.ToList(), out needChance);
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
		}
		UpdateCanInteract();
		IsEnabled.Value = flag;
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
		ReactiveCollection<BaseUnitEntity> selectedUnits = Game.Instance.SelectionCharacter.SelectedUnits;
		for (int i = 0; i < selectedUnits.Count; i++)
		{
			BaseUnitEntity baseUnitEntity = selectedUnits[i];
			if ((MapObjectEntity.Position - baseUnitEntity.Position).sqrMagnitude <= m_ProximityRadius * m_ProximityRadius)
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
