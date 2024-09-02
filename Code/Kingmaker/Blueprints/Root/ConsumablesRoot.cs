using System;
using Kingmaker.Blueprints.Items;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class ConsumablesRoot
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_MeltaChargeItem;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_MultikeyItem;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_RitualSetItem;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_VoidCryptKeyItem;

	public BlueprintItem MeltaChargeItem => m_MeltaChargeItem.Get();

	public BlueprintItem MultikeyItem => m_MultikeyItem.Get();

	public BlueprintItem RitualSetItem => m_RitualSetItem.Get();

	public BlueprintItem VoidCryptKeyItem => m_VoidCryptKeyItem.Get();
}
