using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.Items.Equipment;

[Serializable]
[JsonObject]
public class WeaponAbilityContainer : IEnumerable<WeaponAbility>, IEnumerable, IReadOnlyList<WeaponAbility>, IReadOnlyCollection<WeaponAbility>
{
	public WeaponAbility Ability1;

	public WeaponAbility Ability2;

	public WeaponAbility Ability3;

	public WeaponAbility Ability4;

	public WeaponAbility Ability5;

	public int Count => 5;

	[NotNull]
	public WeaponAbility this[int index] => index switch
	{
		0 => Ability1, 
		1 => Ability2, 
		2 => Ability3, 
		3 => Ability4, 
		4 => Ability5, 
		_ => throw new IndexOutOfRangeException(), 
	};

	public IEnumerable<WeaponAbility> All
	{
		get
		{
			yield return Ability1 ?? (Ability1 = new WeaponAbility());
			yield return Ability2 ?? (Ability2 = new WeaponAbility());
			yield return Ability3 ?? (Ability3 = new WeaponAbility());
			yield return Ability4 ?? (Ability4 = new WeaponAbility());
			yield return Ability5 ?? (Ability5 = new WeaponAbility());
		}
	}

	public IEnumerable<(int Index, WeaponAbility Slot)> AllWithIndex => Enumerable.Range(0, Count).Zip(All, (int i, WeaponAbility a) => (i: i, a: a));

	public IEnumerator<WeaponAbility> GetEnumerator()
	{
		return All.Where((WeaponAbility i) => i.Ability != null).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
