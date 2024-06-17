using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Mechanics.Entities;

public class SimpleCaster : SimpleMechanicEntity, IHashable
{
	private const int Count = 10;

	private static readonly List<SimpleCaster> Pool = new List<SimpleCaster>();

	public bool IsTrap;

	public GameObject TrapParentObject;

	public string NameInLog;

	public override bool ForbidFactsAndPartsModifications => true;

	public static void WarmupPool()
	{
		if (!Pool.Any())
		{
			Enumerable.Range(0, 10).ForEach(delegate(int i)
			{
				Entity.Initialize(new SimpleCaster(i));
			});
		}
	}

	public static SimpleCaster GetFree()
	{
		foreach (SimpleCaster item in Pool)
		{
			bool flag = true;
			foreach (AbilityExecutionProcess ability in Game.Instance.AbilityExecutor.Abilities)
			{
				if (ability.Context.Caster == item)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return item;
			}
		}
		throw new Exception("All SimpleCasters are busy");
	}

	public SimpleCaster(int index)
		: base("simple_caster_" + index, isInGame: true, BlueprintWarhammerRoot.Instance.DefaultMapObjectBlueprint)
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		Pool.Add(this);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Pool.Remove(this);
	}

	protected override IEntityViewBase CreateViewForData()
	{
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
