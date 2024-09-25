using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;

namespace Kingmaker.Blueprints.Items.Ecnchantments;

public class ItemEnchantmentCollection : EntityFactsProcessor<ItemEnchantment>
{
	public ItemEntity Owner => base.Manager?.Owner as ItemEntity;

	public IEnumerable<ItemEnchantment> Enumerable => base.RawFacts;

	public int EnchantmentValue => Enumerable.Aggregate(0, (int r, ItemEnchantment e) => r + e.Blueprint.EnchantmentCost);

	public int RuntimeVersion { get; private set; } = 1;


	public IEnumerator<ItemEnchantment> GetEnumerator()
	{
		return base.RawFacts.GetEnumerator();
	}

	public ItemEnchantment AddEnchantment(BlueprintItemEnchantment blueprint, MechanicsContext parentContext = null, Rounds? duration = null)
	{
		ItemEnchantment itemEnchantment = base.Manager.Add(new ItemEnchantment(blueprint, parentContext));
		if (itemEnchantment != null && duration.HasValue)
		{
			itemEnchantment.EndTime = Game.Instance.TimeController.GameTime + duration.Value.Seconds;
		}
		return itemEnchantment;
	}

	protected override ItemEnchantment PrepareFactForAttach(ItemEnchantment fact)
	{
		fact.SuppressActivationOnAttach = Owner.Wielder == null;
		return fact;
	}

	protected override ItemEnchantment PrepareFactForDetach(ItemEnchantment fact)
	{
		return fact;
	}

	protected override void OnFactDidAttach(ItemEnchantment fact)
	{
		RuntimeVersion++;
	}

	protected override void OnFactWillDetach(ItemEnchantment fact)
	{
	}

	protected override void OnFactDidDetached(ItemEnchantment fact)
	{
		RuntimeVersion++;
	}
}
