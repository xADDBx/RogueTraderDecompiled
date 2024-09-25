using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Blueprints.Items.Weapons;

public class WeaponDamageScaleTableRow
{
	public List<WeaponDamageScaleTableItem> Items { get; private set; }

	public WeaponDamageScaleTableRow(IList<WeaponDamageScaleTableItem> items)
	{
		Items = new List<WeaponDamageScaleTableItem>();
		items.ForEach(delegate(WeaponDamageScaleTableItem i)
		{
			Items.Add(i);
		});
	}
}
