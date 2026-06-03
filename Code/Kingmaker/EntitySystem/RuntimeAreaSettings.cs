using System.Collections.Generic;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.FlagCountable;

namespace Kingmaker.EntitySystem;

public class RuntimeAreaSettings
{
	public readonly CountableFlag Peaceful = new CountableFlag();

	public readonly CountableFlag IgnorePartyEncumbrance = new CountableFlag();

	public readonly CountableFlag IgnorePersonalEncumbrance = new CountableFlag();

	public readonly CountableFlag CannotJumpToWarp = new CountableFlag();

	public readonly CountableFlag AugmentsViewOnly = new CountableFlag();

	private static bool m_IgnoreViewOnly;

	private readonly CountingGuard m_CapitalPartyMode = new CountingGuard(canGoNegative: true);

	private readonly List<EtudeBracketForceInitiativeOrder> m_EtudeBracketForceInitiativeOrders = new List<EtudeBracketForceInitiativeOrder>();

	public bool CapitalModeTemporaryDisabled_Hack { get; set; }

	public bool CapitalPartyMode
	{
		get
		{
			if (!CapitalModeTemporaryDisabled_Hack)
			{
				return m_CapitalPartyMode;
			}
			return false;
		}
	}

	[CanBeNull]
	public EtudeBracketForceInitiativeOrder CurrentEtudeBracketForceInitiativeOrder => m_EtudeBracketForceInitiativeOrders.LastOrDefault();

	public RuntimeAreaSettings()
	{
		AugmentsViewOnly.Retain();
	}

	public bool SetCapitalMode(bool value)
	{
		if (m_CapitalPartyMode.SetValue(value))
		{
			Game.Instance.Player.InvalidateCharacterLists();
			EventBus.RaiseEvent(delegate(IPartyHandler h)
			{
				h.HandleCapitalModeChanged();
			});
			return true;
		}
		return false;
	}

	public void SetEtudeBracketForceInitiativeOrder([NotNull] EtudeBracketForceInitiativeOrder value)
	{
		if (m_EtudeBracketForceInitiativeOrders.Contains(value))
		{
			PFLog.Default.Error("EtudeBracketForceInitiativeOrders list already contains the order instance! Ignoring...");
		}
		else
		{
			m_EtudeBracketForceInitiativeOrders.Add(value);
		}
	}

	public void RemoveEtudeBracketForceInitiativeOrder([NotNull] EtudeBracketForceInitiativeOrder value)
	{
		if (!m_EtudeBracketForceInitiativeOrders.Remove(value))
		{
			PFLog.Default.Error("EtudeBracketForceInitiativeOrders list didn't contain the order instance!");
		}
	}

	[Cheat(Name = "augments_ignore_viewonly")]
	public static void CheatViewOnlyIgnoreHandler()
	{
		m_IgnoreViewOnly = !m_IgnoreViewOnly;
	}

	public bool IsAugmentsViewOnly()
	{
		if (m_IgnoreViewOnly)
		{
			return false;
		}
		BlueprintArea blueprintArea = Game.Instance.LoadedAreaState?.Blueprint;
		bool num = Game.Instance.IsModeActive(GameModeType.StarSystem) || Game.Instance.IsModeActive(GameModeType.GlobalMap);
		bool flag = blueprintArea != null && UIConfig.Instance.VoidshipBridgeAreaReference.Get() == blueprintArea;
		bool flag2 = blueprintArea != null && UIConfig.Instance.MedicareAreaReference.Get() == blueprintArea;
		bool flag3 = Game.Instance.Player.PartyAugmentManager.CurrentAvailableTier == AugmentTier.None;
		if ((num || flag || flag2) && !flag3)
		{
			return false;
		}
		return AugmentsViewOnly;
	}
}
