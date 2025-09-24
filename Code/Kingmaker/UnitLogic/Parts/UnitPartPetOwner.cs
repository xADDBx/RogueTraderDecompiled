using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartPetOwner : BaseUnitPart, IAreaHandler, ISubscriber, IUnitFactionHandler<EntitySubscriber>, IUnitFactionHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IUnitFactionHandler, EntitySubscriber>, IPartyHandler<EntitySubscriber>, IPartyHandler, IEventTag<IPartyHandler, EntitySubscriber>, IPreparationTurnBeginHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, IInGameHandler<EntitySubscriber>, IInGameHandler, ISubscriber<IEntity>, IEventTag<IInGameHandler, EntitySubscriber>, IHashable
{
	private static int PLACEMENT_RADIUS = 5;

	[JsonProperty]
	private bool m_Initialized;

	[JsonProperty]
	private bool m_PetInCombat;

	[JsonProperty]
	public BlueprintPet PetBlueprint;

	[JsonProperty]
	public EntityRef<BaseUnitEntity> m_PetRef;

	[JsonProperty]
	public bool ShouldUnhidePet;

	[JsonProperty]
	public bool? PetIsInGame;

	[JsonIgnore]
	public double LastReactionMoment;

	[JsonIgnore]
	public const int INTEREST_REACTION_COOLDOWN = 3;

	[JsonIgnore]
	private UnitPartNotMoveTrigger m_AfkAnimationsPart;

	public BaseUnitEntity PetUnit => m_PetRef;

	[JsonIgnore]
	public bool HasPet => !m_PetRef.IsNull;

	public PetType PetType => PetBlueprint.type;

	public bool IsPetFollowing { get; private set; }

	public bool PetIsDeactivated
	{
		get
		{
			if ((!PetIsInGame) ?? false)
			{
				UnitPartNotMoveTrigger afkAnimationsPart = m_AfkAnimationsPart;
				if (afkAnimationsPart == null || !afkAnimationsPart.Triggered)
				{
					return true;
				}
			}
			return !base.Owner.IsInGame;
		}
	}

	public void Setup(BlueprintPet pet)
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			PetIsInGame = true;
			PetBlueprint = pet;
			PreparePet();
			InitializePetColors();
			if (!base.Owner.IsPreview)
			{
				EventBus.RaiseEvent((IBaseUnitEntity)base.Owner, (Action<IPetInitializedHandle>)delegate(IPetInitializedHandle h)
				{
					h.HandlePetInitialized(pet);
				}, isCheckRuntime: true);
			}
		}
		StartFollowing();
		m_AfkAnimationsPart = base.Owner.Parts.GetOptional<UnitPartNotMoveTrigger>();
	}

	private void InitializePetColors()
	{
		if (PetUnit?.ViewSettings != null)
		{
			if (PetUnit.ViewSettings.Doll == null)
			{
				PetUnit.ViewSettings.SetDoll(new DollData());
			}
			if (PetUnit.View != null)
			{
				PetUnit.ViewSettings.Doll.ApplyPetRamps(PetUnit.View);
			}
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		if (!Game.Instance.TurnController.InCombat && IsPetFollowing)
		{
			PlacePet();
		}
		SyncPetInGameToOwner();
	}

	public void HandleUnitSpawned()
	{
		if (!m_PetRef.IsNull && EventInvokerExtensions.BaseUnitEntity == m_PetRef.Entity)
		{
			Game.Instance.Player.InvalidateCharacterLists();
		}
	}

	private void PreparePet()
	{
		BaseUnitEntity baseUnitEntity;
		if (!base.Owner.IsPreview)
		{
			baseUnitEntity = Game.Instance.EntitySpawner.SpawnUnit(PetBlueprint.unit, base.Owner.Position, Quaternion.identity, Game.Instance.State.PlayerState.CrossSceneState);
			baseUnitEntity.IsInGame = base.Owner.IsInGame;
		}
		else
		{
			baseUnitEntity = ((BlueprintUnit)PetBlueprint.unit).CreateEntity();
			baseUnitEntity.Inventory.EnsureOwn();
		}
		m_PetRef = baseUnitEntity;
		baseUnitEntity.Faction.Set(base.Owner.Faction.Blueprint);
		baseUnitEntity.InitAsPet(base.Owner);
		SharedStringAsset sharedStringAsset = base.Owner.Facts.GetComponents<PetNamingComponent>().FirstOrDefault()?.GetPetNameByType(PetBlueprint.type);
		if (sharedStringAsset != null)
		{
			baseUnitEntity.Description.SetName(sharedStringAsset.String);
		}
		else if (!string.IsNullOrEmpty(base.Owner.Description.CustomPetName))
		{
			baseUnitEntity.Description.SetName(base.Owner.Description.CustomPetName);
		}
		if (!base.Owner.IsPreview)
		{
			if (base.Owner.CurrentUnwalkableNode != null)
			{
				PlacePet();
			}
			SyncPetInGameToOwner();
		}
	}

	public void PlacePet()
	{
		CustomGridNode currentUnwalkableNode = base.Owner.CurrentUnwalkableNode;
		if (currentUnwalkableNode != null)
		{
			PetUnit.Position = GetFreeNodeAround(currentUnwalkableNode).Vector3Position;
			PetUnit.LookAt(currentUnwalkableNode.Vector3Position);
			PetUnit.MovementAgent.Blocker.BlockAt(PetUnit.Position);
		}
	}

	public void StartFollowing()
	{
		PetUnit.GetOrCreate<UnitPartFollowUnit>().Init(base.Owner, PetBlueprint.FollowSettings);
		IsPetFollowing = true;
	}

	public void StopFollowing()
	{
		PetUnit.Remove<UnitPartFollowUnit>();
		IsPetFollowing = false;
	}

	private CustomGridNodeBase GetFreeNodeAround(CustomGridNode centerNode)
	{
		foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(centerNode, base.Owner.SizeRect, PLACEMENT_RADIUS))
		{
			if (PetUnit.CanStandHere(item))
			{
				return item;
			}
		}
		return centerNode;
	}

	public void HandleFactionChanged()
	{
		if (PetUnit != null && PetUnit.Master == base.Owner)
		{
			PetUnit.CombatGroup.Id = base.Owner.CombatGroup.Id;
			PetUnit.Faction.Set(base.Owner.Faction.Blueprint);
			PetUnit.CombatGroup.ResetFactionSet();
		}
	}

	public void HandleAddCompanion()
	{
	}

	public void HandleCompanionActivated()
	{
		if (PetUnit != null && PetUnit.Master == base.Owner)
		{
			PetUnit.Inventory.MakeSharedInventory();
		}
	}

	public void HandleCompanionRemoved(bool stayInGame)
	{
	}

	public void HandleCapitalModeChanged()
	{
	}

	protected override void OnDetach()
	{
		if (PetUnit == null || PetUnit.Master != base.Owner)
		{
			return;
		}
		PetUnit.IsInGame = false;
		if (!PetUnit.Master.IsPreview)
		{
			if (!PetUnit.Body.PetProtocol.RemoveItem())
			{
				PFLog.Default.Error("Unable to unequip protocol while detaching pet");
			}
			BlueprintItemEquipmentPetProtocol petProtocol = PetUnit.Blueprint.Body.PetProtocol;
			if (petProtocol != null && Game.Instance.Player.Inventory.Contains(petProtocol))
			{
				Game.Instance.Player.Inventory.Remove(petProtocol);
			}
		}
		Game.Instance.EntityDestroyer.Destroy(PetUnit);
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		PetUnit.Commands.InterruptAllInterruptible();
		PetUnit.Commands.AddToQueue(new UnitTeleportParams(GetFreeNodeAround(base.Owner.CurrentUnwalkableNode).Vector3Position, isSynchronized: false, interruptible: false));
	}

	public void HandleObjectInGameChanged()
	{
		SyncPetInGameToOwner();
	}

	private void SyncPetInGameToOwner()
	{
		if (PetUnit != null)
		{
			if (!PetIsInGame.HasValue)
			{
				PetIsInGame = PetUnit.IsInGame || !base.Owner.IsInGame || ShouldUnhidePet;
			}
			PetUnit.IsInGame = base.Owner.IsInGame && PetIsInGame.Value;
		}
	}

	protected override void OnPostLoad()
	{
		base.OnPostLoad();
		if (m_AfkAnimationsPart == null)
		{
			m_AfkAnimationsPart = base.Owner.Parts.GetOptional<UnitPartNotMoveTrigger>();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_Initialized);
		result.Append(ref m_PetInCombat);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(PetBlueprint);
		result.Append(ref val2);
		EntityRef<BaseUnitEntity> obj = m_PetRef;
		Hash128 val3 = StructHasher<EntityRef<BaseUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val3);
		result.Append(ref ShouldUnhidePet);
		if (PetIsInGame.HasValue)
		{
			bool val4 = PetIsInGame.Value;
			result.Append(ref val4);
		}
		return result;
	}
}
