using JetBrains.Annotations;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Formations;

public sealed class PartyFormationCustom : IPartyFormation, IImmutablePartyFormation, IHashable
{
	[NotNull]
	[JsonProperty]
	private readonly Vector2[] m_Positions;

	public float Length => BlueprintPartyFormation.GetLength(m_Positions);

	public AbstractUnitEntity Tank => Game.Instance.Player.PartyAndPets.Get(BlueprintPartyFormation.GetTankIndex(m_Positions));

	[JsonConstructor]
	public PartyFormationCustom(Vector2[] positions)
	{
		m_Positions = positions;
	}

	public Vector2 GetOffset(int index, AbstractUnitEntity _)
	{
		return PartyFormationHelper.GetOffset(m_Positions, index);
	}

	public void SetOffset(int index, AbstractUnitEntity _, Vector2 pos)
	{
		PartyFormationHelper.SetOffset(m_Positions, index, pos);
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(m_Positions);
		return result;
	}
}
