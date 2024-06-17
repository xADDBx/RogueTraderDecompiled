using System;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Cargo;

[TypeId("b2f7c40becfa4e79af7d701025025429")]
public class BlueprintCargoRoot : BlueprintScriptableObject
{
	[Serializable]
	public class CargoTemplate
	{
		[SerializeField]
		private ItemsItemOrigin m_ItemOrigin;

		[SerializeField]
		[ValidateNotNull]
		private BlueprintCargoReference m_Template;

		[SerializeField]
		private int m_ReputationPointsCost;

		public ItemsItemOrigin ItemOrigin => m_ItemOrigin;

		public BlueprintCargo Template => m_Template?.Get();

		public int ReputationPointsCost => m_ReputationPointsCost;
	}

	[Serializable]
	public class Reference : BlueprintReference<BlueprintCargoRoot>
	{
	}

	[SerializeField]
	[ValidateNotNull]
	private CargoTemplate[] m_CargoTemplates;

	[SerializeField]
	private int m_MaxFilledVolumePercentToAddItem = 100;

	public int MaxFilledVolumePercentToAddItem => m_MaxFilledVolumePercentToAddItem;

	public bool IsTemplate(BlueprintCargo blueprintCargo)
	{
		return m_CargoTemplates.Any((CargoTemplate x) => x.Template == blueprintCargo);
	}

	public CargoTemplate GetTemplate(ItemsItemOrigin itemOrigin)
	{
		CargoTemplate cargoTemplate = m_CargoTemplates.FirstOrDefault((CargoTemplate x) => x.ItemOrigin == itemOrigin);
		if (cargoTemplate != null)
		{
			return cargoTemplate;
		}
		PFLog.Default.Error($"Can not find template for ItemOrigin {itemOrigin}");
		return null;
	}
}
