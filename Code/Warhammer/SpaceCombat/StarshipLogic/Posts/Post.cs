using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.SpaceCombat.Blueprints;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Posts;

public class Post : IHashable
{
	[Serializable]
	public class UnitToAbility : IHashable
	{
		[JsonProperty]
		public BlueprintUnit Unit;

		[JsonProperty]
		public BlueprintAbility Ability;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Unit);
			result.Append(ref val);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Ability);
			result.Append(ref val2);
			return result;
		}
	}

	[Serializable]
	public class UnitToAttunedAbility : IHashable
	{
		[JsonProperty]
		public BlueprintUnit Unit;

		[JsonProperty]
		public BlueprintAbility DefaultAbility;

		[JsonProperty]
		public BlueprintAbility AttunedAbility;

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Unit);
			result.Append(ref val);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(DefaultAbility);
			result.Append(ref val2);
			Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(AttunedAbility);
			result.Append(ref val3);
			return result;
		}
	}

	[JsonProperty]
	private readonly EntityRef<StarshipEntity> Starship;

	[JsonProperty]
	public readonly PostType PostType;

	[JsonProperty]
	private EntityRef<BaseUnitEntity> m_CurrentUnit;

	[JsonProperty]
	private List<UnitToAbility> m_UnitsUseAbilities = new List<UnitToAbility>();

	[JsonProperty]
	private List<UnitToAttunedAbility> m_UnitAttunedAbilities = new List<UnitToAttunedAbility>();

	[JsonProperty]
	private bool m_Initialized;

	[JsonProperty]
	public readonly List<Buff> AppliedBuffs = new List<Buff>();

	public StarshipEntity Ship => Starship.Entity;

	public EntityRef<StarshipEntity> ShipRef => Starship;

	public IEnumerable<BlueprintAbility> DefaultAbilities => PostData.DefaultAbilities;

	[JsonProperty(PropertyName = "CurrentUnit")]
	private BaseUnitEntity CurrentUnit_Obsolete
	{
		set
		{
			m_CurrentUnit = value;
		}
	}

	public BaseUnitEntity CurrentUnit => m_CurrentUnit;

	public bool HasPenalty => CurrentSkillValue < 1;

	public int CurrentSkillValue => (CurrentUnit?.GetStatOptional(PostData.AssociatedSkill)?.ModifiedValue).GetValueOrDefault();

	public BlueprintPortrait Portrait => CurrentUnit?.Blueprint?.PortraitSafe ?? BlueprintRoot.Instance.UIConfig.Portraits.LeaderPlaceholderPortrait;

	public Buff BlockingBuff => Enumerable.FirstOrDefault(AppliedBuffs, (Buff b) => b.GetComponent<WarhammerBlockStarshipPost>());

	public bool IsBlocked => BlockingBuff != null;

	public PostData PostData => Ship.Blueprint.Posts.Find((PostData x) => x.type == PostType);

	public IEnumerable<Ability> CurrentAbilities()
	{
		List<Ability> list = new List<Ability>();
		foreach (BlueprintAbility blueprintAbility in DefaultAbilities)
		{
			BlueprintAbility blueprintAbility2 = m_UnitAttunedAbilities.FirstOrDefault((UnitToAttunedAbility a) => a.Unit == CurrentUnit?.Blueprint && blueprintAbility == a.DefaultAbility)?.AttunedAbility;
			Ability ability = Ship.Abilities.GetAbility(blueprintAbility2 ?? blueprintAbility);
			if (ability != null)
			{
				list.Add(ability);
			}
		}
		return list;
	}

	public IEnumerable<BlueprintShipPostExpertise> UnitExpertises(BaseUnitEntity unit)
	{
		return unit?.Facts?.GetAll((Feature f) => f.Blueprint is BlueprintShipPostExpertise)?.Select((Feature f) => f.Blueprint as BlueprintShipPostExpertise).EmptyIfNull();
	}

	public IEnumerable<Ability> UnlockedAbilities()
	{
		if (Ship?.StarshipProgression == null)
		{
			return Enumerable.Empty<Ability>();
		}
		IEnumerable<Ability> unlocked = Ship.StarshipProgression.GetUnlockedAbilities();
		return from ability in CurrentAbilities()
			where unlocked.Contains(ability)
			select ability;
	}

	public Post(StarshipEntity starship, PostType type)
	{
		Starship = starship;
		PostType = type;
	}

	public void Initialize()
	{
		if (!m_Initialized)
		{
			m_Initialized = true;
			AbstractUnitEntity value = null;
			PostData.DefaultUnit?.TryGetValue(out value);
			SetUnitOnPost(value as BaseUnitEntity);
		}
	}

	[JsonConstructor]
	public Post(JsonConstructorMark _)
	{
	}

	public void Subscribe()
	{
		EventBus.Subscribe(this);
	}

	public void Unsubscribe()
	{
		EventBus.Unsubscribe(this);
	}

	public void BlockBy(Buff buff)
	{
		bool isBlocked = IsBlocked;
		PFLog.Default.Log($"{buff.Name} blocks {PostType} post");
		AppliedBuffs.Add(buff);
		if (!isBlocked && IsBlocked)
		{
			EventBus.RaiseEvent(delegate(IStarshipPostHandler h)
			{
				h.HandlePostBlocked(this);
			});
		}
		EventBus.RaiseEvent(delegate(IStarshipPostHandler h)
		{
			h.HandleBuffDidAdded(this, buff);
		});
	}

	public void Unblock(Buff buff)
	{
		if (AppliedBuffs.Contains(buff))
		{
			PFLog.Default.Log($"{BlockingBuff.Name} was removed from {PostType} post");
			AppliedBuffs.Remove(buff);
			PFLog.Default.Log(string.Format("{0} post is {1}", PostType, IsBlocked ? ("blocked by " + BlockingBuff.Name) : "working well"));
			EventBus.RaiseEvent(delegate(IStarshipPostHandler h)
			{
				h.HandleBuffDidRemoved(this, buff);
			});
		}
	}

	public void SetUnitOnPost([CanBeNull] BaseUnitEntity unit)
	{
		foreach (UnitToAttunedAbility item in m_UnitAttunedAbilities.Where((UnitToAttunedAbility a) => a.Unit == CurrentUnit?.Blueprint).EmptyIfNull())
		{
			Ship.Abilities.Remove(item.AttunedAbility);
		}
		m_CurrentUnit = unit;
		foreach (UnitToAttunedAbility item2 in m_UnitAttunedAbilities.Where((UnitToAttunedAbility a) => a.Unit == CurrentUnit?.Blueprint).EmptyIfNull())
		{
			Ship.Abilities.Add(item2.AttunedAbility);
		}
		EventBus.RaiseEvent(delegate(IOnNewUnitOnPostHandler h)
		{
			h.HandleNewUnit();
		});
	}

	public bool IsEnoughScrapToAttune()
	{
		int scrapToAttunePostAbility = BlueprintWarhammerRoot.Instance.BlueprintScrapRoot.ScrapToAttunePostAbility;
		return (int)Game.Instance.Player.Scrap >= scrapToAttunePostAbility;
	}

	public bool IsAbilityUsed(BlueprintAbility ability)
	{
		return m_UnitsUseAbilities.Contains((UnitToAbility data) => data.Unit == CurrentUnit?.Blueprint && data.Ability == ability);
	}

	public bool NoUnit()
	{
		return CurrentUnit == null;
	}

	public bool ShipHasFullHealth()
	{
		return Ship.Health.HitPointsLeft >= (int)Ship.Health.HitPoints;
	}

	public bool IsAlreadyAttuned(BlueprintAbility ability)
	{
		return m_UnitAttunedAbilities.FirstOrDefault((UnitToAttunedAbility a) => a.Unit == CurrentUnit?.Blueprint && a.AttunedAbility == ability) != null;
	}

	public bool TryAttuneAbility(BlueprintAbility ability)
	{
		if (NoUnit())
		{
			return false;
		}
		if (IsAlreadyAttuned(ability))
		{
			return false;
		}
		BlueprintShipPostExpertise blueprintShipPostExpertise = UnitExpertises(CurrentUnit).FirstOrDefault((BlueprintShipPostExpertise exp) => exp.DefaultPostAbility == ability);
		if (blueprintShipPostExpertise == null)
		{
			return false;
		}
		if (!IsEnoughScrapToAttune())
		{
			return false;
		}
		if (!ShipHasFullHealth())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ShipCustomization.AttuneFailedNotFullHP, addToLog: false, WarningNotificationFormat.Short);
			});
			return false;
		}
		if (!IsAbilityUsed(ability))
		{
			return false;
		}
		int scrapToAttunePostAbility = BlueprintWarhammerRoot.Instance.BlueprintScrapRoot.ScrapToAttunePostAbility;
		Game.Instance.Player.Scrap.Spend(scrapToAttunePostAbility);
		m_UnitAttunedAbilities.Add(new UnitToAttunedAbility
		{
			Unit = CurrentUnit?.Blueprint,
			DefaultAbility = ability,
			AttunedAbility = blueprintShipPostExpertise.ChangedPostAbility
		});
		Starship.Entity.Abilities.Add(blueprintShipPostExpertise.ChangedPostAbility);
		EventBus.RaiseEvent(delegate(IOnNewUnitOnPostHandler h)
		{
			h.HandleNewUnit();
		});
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(UIStrings.Instance.ShipCustomization.AttuneSuccess, addToLog: false, WarningNotificationFormat.Attention);
		});
		return true;
	}

	public void UnitUseAbilityFirstTime(BlueprintAbility ability)
	{
		if (CurrentUnit != null && !m_UnitsUseAbilities.Contains((UnitToAbility data) => data.Unit == CurrentUnit.Blueprint && data.Ability == ability))
		{
			m_UnitsUseAbilities.Add(new UnitToAbility
			{
				Unit = CurrentUnit.Blueprint,
				Ability = ability
			});
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef<StarshipEntity> obj = Starship;
		Hash128 val = StructHasher<EntityRef<StarshipEntity>>.GetHash128(ref obj);
		result.Append(ref val);
		PostType val2 = PostType;
		result.Append(ref val2);
		EntityRef<BaseUnitEntity> obj2 = m_CurrentUnit;
		Hash128 val3 = StructHasher<EntityRef<BaseUnitEntity>>.GetHash128(ref obj2);
		result.Append(ref val3);
		List<UnitToAbility> unitsUseAbilities = m_UnitsUseAbilities;
		if (unitsUseAbilities != null)
		{
			for (int i = 0; i < unitsUseAbilities.Count; i++)
			{
				Hash128 val4 = ClassHasher<UnitToAbility>.GetHash128(unitsUseAbilities[i]);
				result.Append(ref val4);
			}
		}
		List<UnitToAttunedAbility> unitAttunedAbilities = m_UnitAttunedAbilities;
		if (unitAttunedAbilities != null)
		{
			for (int j = 0; j < unitAttunedAbilities.Count; j++)
			{
				Hash128 val5 = ClassHasher<UnitToAttunedAbility>.GetHash128(unitAttunedAbilities[j]);
				result.Append(ref val5);
			}
		}
		result.Append(ref m_Initialized);
		List<Buff> appliedBuffs = AppliedBuffs;
		if (appliedBuffs != null)
		{
			for (int k = 0; k < appliedBuffs.Count; k++)
			{
				Hash128 val6 = ClassHasher<Buff>.GetHash128(appliedBuffs[k]);
				result.Append(ref val6);
			}
		}
		return result;
	}
}
