using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[Serializable]
public class ItemToActions
{
	[SerializeField]
	private BlueprintItemReference m_Item;

	public ActionList Actions;

	public BlueprintItem Item => m_Item?.Get();
}
