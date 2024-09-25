using System.Collections.Generic;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;

public class TooltipBrickNonStackVm : TooltipBaseBrickVM
{
	public class NonStackEntity
	{
		public Sprite Icon;

		public string Name;
	}

	public readonly List<NonStackEntity> Entities = new List<NonStackEntity>();

	public TooltipBrickNonStackVm(UnitPartNonStackBonuses bonus)
	{
		foreach (ItemSlot items in bonus.GetItemsList())
		{
			Entities.Add(new NonStackEntity
			{
				Icon = items.Item.Icon,
				Name = items.Item.Name
			});
		}
		foreach (Buff buff in bonus.GetBuffList())
		{
			Entities.Add(new NonStackEntity
			{
				Icon = buff.Icon,
				Name = buff.Name
			});
		}
	}
}
