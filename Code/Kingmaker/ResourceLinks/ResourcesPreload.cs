using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.ResourceLinks.BaseInterfaces;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.ResourceLinks;

public static class ResourcesPreload
{
	[NotNull]
	private static readonly HashSet<IResourceIdsHolder> s_Preloaded = new HashSet<IResourceIdsHolder>();

	[NotNull]
	private static readonly HashSet<BlueprintUnit> s_PreloadedUnits = new HashSet<BlueprintUnit>();

	public static void PreloadMapObjectViews()
	{
	}

	public static void PreloadUnitViews()
	{
		PreloadUnitViews(Game.Instance.Player.CrossSceneState);
		foreach (SceneEntitiesState allSceneState in Game.Instance.State.LoadedAreaState.GetAllSceneStates())
		{
			PreloadUnitViews(allSceneState);
		}
	}

	private static void PreloadUnitViews(SceneEntitiesState state)
	{
		foreach (Entity allEntityDatum in state.AllEntityData)
		{
			if (allEntityDatum is BaseUnitEntity baseUnitEntity)
			{
				baseUnitEntity.UISettings.Portrait.Preload();
			}
		}
	}

	public static void PreloadUnitResources()
	{
		try
		{
			foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
			{
				PreloadUnitBlueprint(allUnit.Blueprint);
			}
		}
		finally
		{
		}
	}

	private static void Preload(IResourceIdsHolder holder)
	{
		if (holder != null && !s_Preloaded.Contains(holder))
		{
			s_Preloaded.Add(holder);
			PreloadResources(holder);
		}
	}

	private static void PreloadUnitBlueprint(BlueprintUnit unit)
	{
		if (!s_PreloadedUnits.Contains(unit))
		{
			unit.PreloadResources();
			s_PreloadedUnits.Add(unit);
		}
	}

	public static void PreloadResources(IResourceIdsHolder holder)
	{
		string[] array = holder.GetResourceIds().EmptyIfNull();
		for (int i = 0; i < array.Length; i++)
		{
			ResourcesLibrary.PreloadResource<GameObject>(array[i]);
		}
	}
}
