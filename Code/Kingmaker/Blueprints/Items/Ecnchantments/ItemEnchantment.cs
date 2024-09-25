using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Particles;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Ecnchantments;

public class ItemEnchantment : MechanicEntityFact<ItemEntity>, IHashable
{
	public class Data : ContextData<Data>
	{
		public ItemEnchantment ItemEnchantment { get; private set; }

		public Data Setup(ItemEnchantment enchantment)
		{
			ItemEnchantment = enchantment;
			return this;
		}

		protected override void Reset()
		{
			ItemEnchantment = null;
		}
	}

	[JsonProperty]
	[CanBeNull]
	private MechanicsContext m_CurrentContext;

	public override Type RequiredEntityType => EntityInterfacesHelper.ItemEntityInterface;

	[CanBeNull]
	[JsonProperty]
	public MechanicsContext ParentContext { get; private set; }

	[JsonProperty]
	public TimeSpan EndTime { get; set; }

	[JsonProperty]
	public bool RemoveOnUnequipItem { get; set; }

	public MechanicsContext Context => m_CurrentContext ?? throw new Exception("You can't use Context while ItemEnchantment is not active");

	public override MechanicsContext MaybeContext => Context;

	public bool IsTemporary => EndTime != TimeSpan.Zero;

	public bool IsEnded
	{
		get
		{
			if (IsTemporary)
			{
				return EndTime - Game.Instance.TimeController.GameTime <= 0.Seconds();
			}
			return false;
		}
	}

	public new BlueprintItemEnchantment Blueprint => (BlueprintItemEnchantment)base.Blueprint;

	public GameObject FxObject { get; private set; }

	private bool WielderVisibleInInventory
	{
		get
		{
			if (base.Owner?.Wielder != null)
			{
				return UIDollRooms.Instance?.CharacterDollRoom?.Unit == base.Owner?.Wielder;
			}
			return false;
		}
	}

	public ItemEnchantment(BlueprintItemEnchantment blueprint, MechanicsContext parentContext = null)
		: base((BlueprintMechanicEntityFact)blueprint)
	{
		ParentContext = parentContext;
	}

	protected ItemEnchantment(JsonConstructorMark _)
	{
	}

	public void RespawnFx()
	{
		GameObject gameObject = (Blueprint as BlueprintWeaponEnchantment)?.WeaponFxPrefab;
		if ((bool)gameObject)
		{
			DestroyFx();
			WeaponParticlesSnapMap weaponSnap = (base.Owner.HoldingSlot as WeaponSlot)?.FxSnapMap;
			FxObject = FxHelper.SpawnFxOnWeapon(gameObject, base.Owner.Wielder?.View, weaponSnap);
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		m_CurrentContext = ((ParentContext != null) ? ParentContext.CloneFor(Blueprint, base.Owner.Wielder) : new MechanicsContext(null, base.Owner.Wielder, Blueprint));
		if (!WielderVisibleInInventory)
		{
			RespawnFx();
		}
	}

	protected override void OnDeactivate()
	{
		base.OnDeactivate();
		m_CurrentContext = null;
		if (!WielderVisibleInInventory)
		{
			DestroyFx();
		}
	}

	public void DestroyFx()
	{
		if ((bool)FxObject)
		{
			FxHelper.Destroy(FxObject);
			FxObject = null;
		}
	}

	public override void RunActionInContext(ActionList action, ITargetWrapper target = null)
	{
		if (m_CurrentContext == null)
		{
			throw new Exception("You can't RunActionInContext while ItemEnchantment is not active");
		}
		using (ContextData<Data>.Request().Setup(this))
		{
			using (m_CurrentContext.GetDataScope(target))
			{
				base.RunActionInContext(action, target);
			}
		}
	}

	protected override void OnDetach()
	{
		base.OnDetach();
		m_CurrentContext = null;
		ParentContext = null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicsContext>.GetHash128(m_CurrentContext);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<MechanicsContext>.GetHash128(ParentContext);
		result.Append(ref val3);
		TimeSpan val4 = EndTime;
		result.Append(ref val4);
		bool val5 = RemoveOnUnequipItem;
		result.Append(ref val5);
		return result;
	}
}
