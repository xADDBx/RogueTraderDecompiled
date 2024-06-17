using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Warhammer.SpaceCombat.StarshipLogic.Posts;

namespace Kingmaker.Visual.Sound;

public class StarshipBarksManager : UnitBarksManager
{
	private class UnitOnPostBarksManagerWrapper
	{
		private readonly BlueprintUnitReference m_BlueprintRef;

		private readonly UnitReference m_EntityRef;

		private readonly UnitBarksManager m_OverrideBarksManager;

		public bool HasOverrideBarks => m_OverrideBarksManager != null;

		public UnitBarksManager BarksManager => m_OverrideBarksManager ?? m_EntityRef.Entity.ToBaseUnitEntity().View.Asks;

		public bool IsAvailable => Game.Instance.Player.PlayerShip.Hull.Posts.Any((Post x) => x.CurrentUnit != null && x.CurrentUnit.Blueprint.Equals(m_BlueprintRef.Get()));

		public UnitOnPostBarksManagerWrapper(BlueprintUnitReference blueprintRef, UnitAsksComponent asksComponent)
		{
			m_BlueprintRef = blueprintRef;
			m_EntityRef = default(UnitReference);
			m_OverrideBarksManager = new UnitBarksManager(null, asksComponent);
		}

		public UnitOnPostBarksManagerWrapper(UnitReference entityRef)
		{
			m_BlueprintRef = entityRef.Entity.ToBaseUnitEntity().Blueprint.ToReference<BlueprintUnitReference>();
			m_EntityRef = entityRef;
			m_OverrideBarksManager = null;
		}
	}

	private readonly List<UnitOnPostBarksManagerWrapper> m_UnitOnPostBarks = new List<UnitOnPostBarksManagerWrapper>();

	public StarshipBarksManager(StarshipEntity unit, UnitAsksComponent component)
		: base(unit, component)
	{
		InitializePosts(unit);
	}

	private void InitializePosts(StarshipEntity unit)
	{
		if (m_UnitOnPostBarks.Count > 0)
		{
			return;
		}
		m_UnitOnPostBarks.Clear();
		if (unit.Hull?.Posts == null)
		{
			return;
		}
		foreach (Post post in unit.Hull.Posts)
		{
			if (post.PostData != null && post.CurrentUnit != null && TryWrap(post.CurrentUnit.Blueprint.ToReference<BlueprintUnitReference>(), out var result))
			{
				m_UnitOnPostBarks.Add(result);
			}
		}
	}

	private static bool TryWrap(BlueprintUnitReference unitRef, out UnitOnPostBarksManagerWrapper result)
	{
		BlueprintUnit unitBlueprint = unitRef?.Get();
		if (unitBlueprint == null)
		{
			result = null;
			return false;
		}
		UnitReference unitReference = Game.Instance.Player.PartyCharacters.FirstOrDefault((UnitReference x) => x.Entity.ToBaseUnitEntity().Blueprint == unitBlueprint);
		if (unitReference != null && unitReference.Entity?.ToBaseUnitEntity().Asks.List?.GetComponent<UnitAsksComponent>() != null)
		{
			result = new UnitOnPostBarksManagerWrapper(unitReference);
			return true;
		}
		UnitAsksComponent unitAsksComponent = unitBlueprint.VisualSettings?.Barks?.GetComponent<UnitAsksComponent>();
		if (unitAsksComponent == null)
		{
			result = null;
			return false;
		}
		result = new UnitOnPostBarksManagerWrapper(unitRef, unitAsksComponent);
		return true;
	}

	protected override void OnLoadBanks()
	{
		foreach (UnitOnPostBarksManagerWrapper item in m_UnitOnPostBarks.Where((UnitOnPostBarksManagerWrapper i) => i.HasOverrideBarks))
		{
			item.BarksManager.LoadBanks();
		}
	}

	protected override void OnUnloadBanks()
	{
		foreach (UnitOnPostBarksManagerWrapper item in m_UnitOnPostBarks.Where((UnitOnPostBarksManagerWrapper i) => i.HasOverrideBarks))
		{
			item.BarksManager.UnloadBanks();
		}
	}

	public void ScheduleRandomUnitOnPostBark(Func<UnitBarksManager, BarkWrapper> selector)
	{
		InitializePosts(Unit as StarshipEntity);
		(from x in m_UnitOnPostBarks
			where x.IsAvailable
			let y = selector(x.BarksManager)
			where y.HasBarks && !y.IsOnCooldown
			select y).Random(PFStatefulRandom.Visuals.UnitAsks)?.Schedule(is2D: true);
	}
}
