using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Localization;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("ecfbdb148a4d48e398acc9e5d1461419")]
public class PetNamingComponent : UnitFactComponentDelegate, IHashable
{
	[Serializable]
	private struct PetName
	{
		public PetType Type;

		public SharedStringAsset Name;
	}

	[SerializeField]
	private PetName[] m_PetNames;

	public SharedStringAsset GetPetNameByType(PetType petType)
	{
		for (int i = 0; i < m_PetNames.Length; i++)
		{
			if (m_PetNames[i].Type == petType)
			{
				return m_PetNames[i].Name;
			}
		}
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
