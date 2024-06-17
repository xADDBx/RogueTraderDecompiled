using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("54b8118d35ef44847b10a125ed9d64f7")]
public class SuppressBuffs : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	[FormerlySerializedAs("Buffs")]
	private BlueprintBuffReference[] m_Buffs = new BlueprintBuffReference[0];

	public SpellSchool[] Schools;

	public SpellDescriptorWrapper Descriptor;

	public ReferenceArrayProxy<BlueprintBuff> Buffs
	{
		get
		{
			BlueprintReference<BlueprintBuff>[] buffs = m_Buffs;
			return buffs;
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		UnitPartBuffSuppress orCreate = base.Owner.GetOrCreate<UnitPartBuffSuppress>();
		if (Descriptor != SpellDescriptor.None)
		{
			orCreate.Suppress(Descriptor);
		}
		if (!Schools.Empty())
		{
			orCreate.Suppress(Schools);
		}
		foreach (BlueprintBuff buff in Buffs)
		{
			orCreate.Suppress(buff);
		}
	}

	protected override void OnDeactivate()
	{
		UnitPartBuffSuppress optional = base.Owner.GetOptional<UnitPartBuffSuppress>();
		if (optional == null)
		{
			PFLog.Default.Error("UnitPartSuppressBuff is missing");
			return;
		}
		if (Descriptor != SpellDescriptor.None)
		{
			optional.Release(Descriptor);
		}
		if (!Schools.Empty())
		{
			optional.Release(Schools);
		}
		foreach (BlueprintBuff buff in Buffs)
		{
			optional.Release(buff);
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
