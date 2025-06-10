using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutsceneControlledUnit
{
	[NotNull]
	public readonly IAbstractUnitEntity Unit;

	[CanBeNull]
	private CutsceneEntry m_Active;

	[NotNull]
	private readonly List<CutsceneEntry> m_Entries = new List<CutsceneEntry>();

	private bool CanPlayCutscene
	{
		get
		{
			if (Game.Instance.CurrentMode == GameModeType.Cutscene || Game.Instance.GameCommandQueue.ContainsCommand((SwitchCutsceneLockCommand i) => i.Lock))
			{
				return true;
			}
			if ((bool)Unit.ToAbstractUnitEntity().Features.IsIgnoredByCombat)
			{
				return true;
			}
			if ((bool)Unit.ToAbstractUnitEntity().Passive)
			{
				return true;
			}
			return !Unit.IsInCombat;
		}
	}

	public CutsceneControlledUnit([NotNull] IAbstractUnitEntity unit)
	{
		Unit = unit;
	}

	private bool Mark(CutscenePlayerData cutscene)
	{
		bool flag = true;
		foreach (CutsceneEntry entry in m_Entries)
		{
			if (entry.Cutscene == cutscene)
			{
				entry.OwnersCount++;
				flag = false;
				break;
			}
		}
		CutsceneEntry cutsceneEntry = null;
		if (flag)
		{
			cutsceneEntry = new CutsceneEntry(cutscene);
			m_Entries.Add(cutsceneEntry);
		}
		if (!cutscene.Anchors.HasItem(Unit.Ref))
		{
			cutscene.Anchors.Add(Unit.Ref);
		}
		UpdateActiveCutscene();
		if (cutsceneEntry != null && cutsceneEntry != m_Active)
		{
			cutsceneEntry.PauseOrStop();
		}
		if (m_Active != null)
		{
			return m_Active.Cutscene == cutscene;
		}
		return false;
	}

	public void Release(CutscenePlayerData cutscene)
	{
		if (m_Entries.Count <= 0)
		{
			return;
		}
		foreach (CutsceneEntry entry in m_Entries)
		{
			if (entry.Cutscene == cutscene)
			{
				entry.OwnersCount--;
				if (entry.OwnersCount <= 0)
				{
					m_Entries.Remove(entry);
					cutscene.Anchors.Remove(Unit.Ref);
				}
				break;
			}
		}
		UpdateActiveCutscene();
	}

	private void UpdateActiveCutscene()
	{
		CutsceneEntry cutsceneEntry = null;
		if (CanPlayCutscene)
		{
			foreach (CutsceneEntry entry in m_Entries)
			{
				if (cutsceneEntry == null || cutsceneEntry?.Cutscene?.Cutscene?.Priority < entry?.Cutscene?.Cutscene?.Priority)
				{
					cutsceneEntry = entry;
				}
			}
		}
		if (m_Active == cutsceneEntry)
		{
			return;
		}
		if (m_Active != null)
		{
			CutsceneEntry active = m_Active;
			if (active != null && active.OwnersCount > 0)
			{
				m_Active?.PauseOrStop();
			}
		}
		cutsceneEntry?.Resume();
		m_Active = cutsceneEntry;
	}

	public static bool MarkUnit(IAbstractUnitEntity unit, CutscenePlayerData player)
	{
		if (unit.ToAbstractUnitEntity().CutsceneControlledUnit == null)
		{
			unit.ToAbstractUnitEntity().CutsceneControlledUnit = new CutsceneControlledUnit(unit);
		}
		return unit.ToAbstractUnitEntity().CutsceneControlledUnit.Mark(player);
	}

	public static void ReleaseUnit(IAbstractUnitEntity unit, CutscenePlayerData player)
	{
		unit.ToAbstractUnitEntity().CutsceneControlledUnit?.Release(player);
	}

	public static void UpdateActiveCutscene(IAbstractUnitEntity unit)
	{
		unit.ToAbstractUnitEntity().CutsceneControlledUnit?.UpdateActiveCutscene();
	}

	public static CutscenePlayerData GetControllingPlayer(IAbstractUnitEntity unit)
	{
		return unit.ToAbstractUnitEntity().CutsceneControlledUnit?.m_Active?.Cutscene;
	}

	public bool IsMarkedBy(CutscenePlayerData data)
	{
		return m_Entries.HasItem((CutsceneEntry e) => e.Cutscene == data);
	}

	public Cutscene GetCurrentlyActive()
	{
		return m_Active?.Cutscene?.Cutscene;
	}

	public static bool IsFreezingAllowed(IAbstractUnitEntity unit)
	{
		CutscenePlayerData cutscenePlayerData = unit.ToAbstractUnitEntity().CutsceneControlledUnit?.m_Active?.Cutscene;
		if (cutscenePlayerData == null)
		{
			return true;
		}
		return !cutscenePlayerData.Cutscene.Freezeless;
	}

	public static bool IsSleepingAllowed(IAbstractUnitEntity unit)
	{
		return (unit.ToAbstractUnitEntity().CutsceneControlledUnit?.m_Active?.Cutscene)?.Paused ?? true;
	}
}
