using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Formations;

[TypeId("3ab6b83e262279f4daf64a622a656750")]
public class BlueprintPartyFormation : BlueprintScriptableObject, IPartyFormation, IImmutablePartyFormation
{
	[NotNull]
	public Vector2[] Positions = new Vector2[0];

	public PartyFormationType Type;

	public LocalizedString Name;

	public float Length => GetLength(Positions);

	public AbstractUnitEntity Tank => Game.Instance.Player.PartyAndPets.Get(GetTankIndex(Positions));

	public Vector2 GetOffset(int index, AbstractUnitEntity unit)
	{
		return PartyFormationHelper.GetOffset(Positions, index);
	}

	public void SetOffset(int index, AbstractUnitEntity unit, Vector2 pos)
	{
	}

	public static int GetTankIndex(IList<Vector2> position)
	{
		int num = 0;
		for (int i = 1; i < position.Count; i++)
		{
			float x = position[i].x;
			float y = position[i].y;
			float x2 = position[num].x;
			float y2 = position[num].y;
			if (0f - y + Math.Abs(x) < 0f - y2 + Math.Abs(x2))
			{
				num = i;
			}
		}
		return num;
	}

	public static float GetLength(IList<Vector2> position)
	{
		int tankIndex = GetTankIndex(position);
		Vector2 vector = position.Get(tankIndex);
		float num = 0f;
		for (int i = 0; i < position.Count; i++)
		{
			if (i != tankIndex)
			{
				float sqrMagnitude = (vector - position[i]).sqrMagnitude;
				num = Math.Max(num, sqrMagnitude);
			}
		}
		return num;
	}
}
