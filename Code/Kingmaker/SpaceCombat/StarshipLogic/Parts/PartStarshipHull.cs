using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic.Equipment;
using Warhammer.SpaceCombat.StarshipLogic.Posts;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.SpaceCombat.StarshipLogic.Parts;

public class PartStarshipHull : StarshipPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStarshipHull>, IEntityPartOwner
	{
		PartStarshipHull Hull { get; }
	}

	[JsonProperty]
	public readonly List<Post> Posts = new List<Post>();

	[JsonProperty]
	public HullSlots HullSlots { get; private set; }

	[JsonProperty]
	public int CurrentMilitaryRating { get; set; }

	[JsonProperty]
	public InternalStructure InternalStructure { get; private set; }

	[JsonProperty]
	public ProwRam ProwRam { get; private set; }

	public List<WeaponSlot> WeaponSlots => HullSlots.WeaponSlots;

	public IEnumerable<ItemEntityStarshipWeapon> Weapons => from x in WeaponSlots
		where x.HasItem
		select x.Weapon;

	public List<Ability> WeaponAbilities => (from slot in WeaponSlots
		where slot.ActiveAbility != null
		select slot.ActiveAbility).ToList();

	public int InitialMilitaryRating => base.Owner.Starship.MilitaryRating;

	[JsonConstructor]
	public PartStarshipHull()
	{
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		HullSlots = new HullSlots(base.Owner);
		if (!ContextData<UnitHelper.PreviewUnit>.Current && !base.Owner.IsPreviewUnit)
		{
			HullSlots.Initialize();
		}
		List<PostData> list = base.Blueprint?.Posts;
		if (list != null && Posts.Empty())
		{
			foreach (PostData item in list)
			{
				AddPost(item);
			}
		}
		CurrentMilitaryRating = InitialMilitaryRating;
		InternalStructure = new InternalStructure(base.Owner);
		ProwRam = new ProwRam(base.Owner);
	}

	protected override void OnDetach()
	{
		HullSlots.Dispose();
	}

	public void AddPost(PostData postData)
	{
		Post item = new Post(base.Owner, postData.type);
		Posts.Add(item);
	}

	protected override void OnSubscribe()
	{
		HullSlots.Subscribe();
	}

	protected override void OnUnsubscribe()
	{
		HullSlots.Unsubscribe();
	}

	protected override void OnPreSave()
	{
		HullSlots.PreSave();
	}

	protected override void OnPrePostLoad()
	{
		HullSlots.PrePostLoad(base.Owner);
		foreach (WeaponSlot weaponSlot in WeaponSlots)
		{
			weaponSlot.PrePostLoad(base.Owner);
			weaponSlot.AmmoSlot.PrePostLoad(base.Owner);
		}
		FixStarshipPosts();
	}

	protected override void OnPostLoad()
	{
		HullSlots.PostLoad();
		foreach (WeaponSlot weaponSlot in WeaponSlots)
		{
			weaponSlot.PostLoad();
			weaponSlot.AmmoSlot.PostLoad();
		}
		InternalStructure internalStructure = InternalStructure;
		if (internalStructure.Owner == null)
		{
			internalStructure.Owner = base.Owner;
		}
		InternalStructure.RecalculateHealthBonus();
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		foreach (Post post in Posts)
		{
			post.Initialize();
		}
	}

	public int GetLocationDeflection(StarshipHitLocation hitLocation)
	{
		return hitLocation switch
		{
			StarshipHitLocation.Fore => AggregateArmorSources(StatType.ArmourFore, (BlueprintItemArmorPlating ap) => ap?.ArmourFore, (StarshipArmorBonus ab) => ab.ArmourFore), 
			StarshipHitLocation.Port => AggregateArmorSources(StatType.ArmourPort, (BlueprintItemArmorPlating ap) => ap?.ArmourPort, (StarshipArmorBonus ab) => ab.ArmourPort), 
			StarshipHitLocation.Starboard => AggregateArmorSources(StatType.ArmourStarboard, (BlueprintItemArmorPlating ap) => ap?.ArmourStarboard, (StarshipArmorBonus ab) => ab.ArmourStarboard), 
			StarshipHitLocation.Aft => AggregateArmorSources(StatType.ArmourAft, (BlueprintItemArmorPlating ap) => ap?.ArmourAft, (StarshipArmorBonus ab) => ab.ArmourAft), 
			_ => 0, 
		};
		int AggregateArmorSources(StatType statType, Func<BlueprintItemArmorPlating, int?> fItemArmor, Func<StarshipArmorBonus, int> fBonusArmor)
		{
			int num = base.Owner.Stats.GetStat(statType);
			int num2 = base.Owner.Facts.GetComponents<StarshipArmorBonus>().Select(fBonusArmor).Aggregate(fItemArmor(base.Owner.ArmorPlatings).GetValueOrDefault(), (int a, int b) => a + b);
			return num + num2;
		}
	}

	private void FixStarshipPosts()
	{
		if (Posts.Select((Post p) => p.PostType).Distinct().Count() == Posts.Count)
		{
			return;
		}
		int i;
		for (i = Posts.Count - 1; i >= 0; i--)
		{
			if (Posts.Count((Post p) => p.PostType == Posts[i].PostType) > 1)
			{
				Posts.RemoveAt(i);
			}
		}
		foreach (PostData postData in base.Owner.Blueprint?.Posts?.EmptyIfNull())
		{
			if (Posts.FirstOrDefault((Post p) => p.PostType == postData.type) == null)
			{
				AddPost(postData);
			}
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		List<Post> posts = Posts;
		if (posts != null)
		{
			for (int i = 0; i < posts.Count; i++)
			{
				Hash128 val2 = ClassHasher<Post>.GetHash128(posts[i]);
				result.Append(ref val2);
			}
		}
		Hash128 val3 = ClassHasher<HullSlots>.GetHash128(HullSlots);
		result.Append(ref val3);
		int val4 = CurrentMilitaryRating;
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<InternalStructure>.GetHash128(InternalStructure);
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<ProwRam>.GetHash128(ProwRam);
		result.Append(ref val6);
		return result;
	}
}
