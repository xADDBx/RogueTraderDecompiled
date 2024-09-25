using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintEtude))]
[TypeId("34ed37943b30447a8d15bde729df81ee")]
public class OverrideAreaCR : EntityFactComponentDelegate, IHashable
{
	[SerializeField]
	private BlueprintAreaPartReference m_LinkedAreaPart;

	[SerializeField]
	private int m_NewCR;

	public int NewCR => m_NewCR;

	public BlueprintAreaPart LinkedAreaPart => m_LinkedAreaPart.Get();

	protected override void OnActivateOrPostLoad()
	{
		if (LinkedAreaPart != null)
		{
			Game.Instance.Player.GetOrCreate<AreaCROverrideManager>().Add(this);
		}
	}

	protected override void OnDeactivate()
	{
		if (LinkedAreaPart != null)
		{
			Game.Instance.Player.GetOptional<AreaCROverrideManager>()?.Remove(this);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
