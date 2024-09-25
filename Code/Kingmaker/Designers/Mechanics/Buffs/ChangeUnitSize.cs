using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Buffs;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("bd6e075c1c773114ab6e34c72f5d8d94")]
public class ChangeUnitSize : UnitFactComponentDelegate, IHashable
{
	private enum ChangeType
	{
		Delta,
		Value
	}

	[SerializeField]
	private ChangeType m_Type;

	[ShowIf("IsTypeDelta")]
	public int SizeDelta;

	[ShowIf("IsTypeValue")]
	public Size Size;

	[UsedImplicitly]
	private bool IsTypeValue => m_Type == ChangeType.Value;

	[UsedImplicitly]
	private bool IsTypeDelta => m_Type == ChangeType.Delta;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartSizeModifier>().Add(base.Fact);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOrCreate<UnitPartSizeModifier>().Remove(base.Fact);
	}

	public Size GetUnitSize(EntityFactComponent runtime)
	{
		using (runtime.RequestEventContext())
		{
			return m_Type switch
			{
				ChangeType.Delta => base.Owner.OriginalSize.Shift(SizeDelta), 
				ChangeType.Value => Size, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
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
