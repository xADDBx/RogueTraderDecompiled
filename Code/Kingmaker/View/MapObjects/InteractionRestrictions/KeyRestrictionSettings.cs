using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

[Serializable]
public class KeyRestrictionSettings
{
	[SerializeField]
	[FormerlySerializedAs("Key")]
	private BlueprintItemReference m_Key;

	public bool DontRemoveKey;

	public BlueprintItem Key => m_Key?.Get();
}
