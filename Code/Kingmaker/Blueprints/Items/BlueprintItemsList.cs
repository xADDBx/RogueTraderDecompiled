using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Items;

[TypeId("0e946708a6f74943970977ac69a9d2c2")]
public class BlueprintItemsList : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintItemsList>
	{
	}

	[SerializeField]
	private BlueprintItemReference[] m_Items = new BlueprintItemReference[0];

	public ReferenceArrayProxy<BlueprintItem> Items
	{
		get
		{
			BlueprintReference<BlueprintItem>[] items = m_Items;
			return items;
		}
	}

	public bool Contains(BlueprintItem item)
	{
		return Items.HasReference(item);
	}
}
