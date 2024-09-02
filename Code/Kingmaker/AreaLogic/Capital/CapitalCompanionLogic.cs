using System;
using System.Linq;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Capital;

[AllowedOn(typeof(BlueprintEtude))]
[TypeId("1862eb457738a6a4d908e20172213f3a")]
public class CapitalCompanionLogic : EtudeBracketTrigger, IHashable
{
	[Obsolete]
	[HideInInspector]
	[SerializeField]
	private bool m_RestAllRemoteCompanions;

	public override bool RequireLinkedArea => true;

	protected override void OnEnter()
	{
		foreach (BaseUnitEntity allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
		{
			UnitPartCompanion optional = allCrossSceneUnit.GetOptional<UnitPartCompanion>();
			if (optional != null && optional.State == CompanionState.InPartyDetached)
			{
				Game.Instance.Player.AttachPartyMember(allCrossSceneUnit);
			}
		}
		if (!Game.Instance.LoadedAreaState.Settings.SetCapitalMode(value: true))
		{
			return;
		}
		PlaceAllCompanions();
		foreach (ItemEntity item in Game.Instance.Player.Inventory)
		{
			item.TryIdentify();
		}
	}

	protected override void OnExit()
	{
		if (!Game.Instance.LoadedAreaState.Settings.SetCapitalMode(value: false))
		{
			return;
		}
		foreach (BaseUnitEntity item in Game.Instance.Player.AllCrossSceneUnits.Where((BaseUnitEntity u) => !(u is StarshipEntity)))
		{
			BaseUnitEntity baseUnitEntity = item;
			baseUnitEntity.IsInGame = item.GetCompanionOptional()?.State switch
			{
				CompanionState.InParty => true, 
				CompanionState.Remote => false, 
				_ => item.IsInGame, 
			};
		}
	}

	protected override void OnResume()
	{
		if (Game.Instance.LoadedAreaState.Settings.SetCapitalMode(value: true))
		{
			PlaceAllCompanions();
		}
	}

	private static void PlaceAllCompanions()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.RemoteCompanions.ToTempList())
		{
			if (item != null && item != Game.Instance.Player.MainCharacterEntity)
			{
				BaseUnitEntity master = item.Master;
				if ((master == null || (!master.IsMainCharacter && !(master.GetOptional<UnitPartCompanion>()?.GetCurrentSpawner()))) && !(item.GetOptional<UnitPartCompanion>()?.GetCurrentSpawner()))
				{
					item.IsInGame = false;
				}
			}
		}
		if (UIAccess.SelectionManager != null)
		{
			UIAccess.SelectionManager.SelectUnit(Game.Instance.Player.MainCharacterEntity.View, single: true, sendSelectionEvent: true, ask: false);
		}
	}

	public static void ExitCapital(BlueprintAreaEnterPoint destination, AutoSaveMode autoSaveMode)
	{
		Game.Instance.LoadedAreaState.Settings.CapitalModeTemporaryDisabled_Hack = true;
		EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
		{
			h.HandleCall(delegate
			{
				Game.Instance.Player.FixPartyAfterChange();
				Game.Instance.LoadedAreaState.Settings.CapitalModeTemporaryDisabled_Hack = false;
				Game.Instance.LoadArea(destination, autoSaveMode);
			}, delegate
			{
				Game.Instance.LoadedAreaState.Settings.CapitalModeTemporaryDisabled_Hack = false;
			}, isCapital: true);
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
