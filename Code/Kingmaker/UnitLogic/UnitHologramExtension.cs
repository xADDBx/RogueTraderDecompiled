using System;
using System.Diagnostics.CodeAnalysis;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public static class UnitHologramExtension
{
	public static UnitHologram CreateHologram([NotNull] this BaseUnitEntity unit)
	{
		UnitHologram unitHologram = null;
		UnitEntityView unitEntityView = null;
		if (unit.View.CharacterAvatar == null && unit.View.GetComponentInChildren<StarshipView>() != null)
		{
			LogChannel.Default.Warning(unit.View, "This unit is not a Character nor a Starship, creating a hologram for this thing isn't possible.");
			return null;
		}
		Skeleton skeleton = unit.View.CharacterAvatar.Skeleton;
		HologramRaceFx[] hologramPrefabs = BlueprintRoot.Instance.FxRoot.HologramPrefabs;
		foreach (HologramRaceFx hologramRaceFx in hologramPrefabs)
		{
			if (hologramRaceFx.Race == null)
			{
				LogChannel.Default.Error(unit.View, "No Skeletal config in HologramFx list in BlueprintFxRoot was found.");
			}
			else if (hologramRaceFx.HologramPrefab == null)
			{
				LogChannel.Default.Error(unit.View, "No HologramPrefab in HologramFx list in BlueprintFxRoot was found.");
			}
			else if (hologramRaceFx.Race == skeleton)
			{
				unitEntityView = SetupHologramPrefab(hologramRaceFx.HologramPrefab, unit);
				break;
			}
		}
		if (unitEntityView == null && BlueprintRoot.Instance.FxRoot.DefaultHologramPrefab != null)
		{
			unitEntityView = SetupHologramPrefab(BlueprintRoot.Instance.FxRoot.DefaultHologramPrefab, unit);
		}
		if (unitEntityView == null)
		{
			LogChannel.Default.Error(unit.View, "Cannot create hologram for the unit. Check BlueprintFxRoot so appropriate data was set in HologramPrefabArray.");
			return null;
		}
		try
		{
			unitEntityView.gameObject.name = unit.View.gameObject.name + " (UnitHologram)";
			unitEntityView.GetComponentsInChildren<IKController>().ForEach(UnityEngine.Object.Destroy);
			unitEntityView.GetComponentsInChildren<UnitMovementAgent>().ForEach(UnityEngine.Object.Destroy);
			unitHologram = unitEntityView.gameObject.EnsureComponent<UnitHologram>();
			unitEntityView.CharacterAvatar = unitEntityView.GetComponent<Character>();
			unitHologram.Setup(unitEntityView, unit.View);
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
			UnityEngine.Object.Destroy(unitEntityView.gameObject);
		}
		return unitHologram;
	}

	public static UnitHologram CreateHologramSpaceship([NotNull] this BaseUnitEntity unit)
	{
		UnitHologram unitHologram = null;
		UnitEntityView unitEntityView = null;
		if (BlueprintRoot.Instance.FxRoot.DefaultStarshipHologramPrefab != null)
		{
			unitEntityView = SetupHologramPrefab(BlueprintRoot.Instance.FxRoot.DefaultStarshipHologramPrefab, unit);
		}
		if (unitEntityView == null)
		{
			LogChannel.Default.Error(unit.View, "Cannot create hologram for the unit. Check BlueprintFxRoot so appropriate data was set in HologramPrefabArray.");
			return null;
		}
		try
		{
			unitEntityView.gameObject.name = unit.View.gameObject.name + " (UnitHologram)";
			unitHologram = unitEntityView.gameObject.EnsureComponent<UnitHologram>();
			unitHologram.SetupStarship(unitEntityView, unit.View);
		}
		catch (Exception ex)
		{
			PFLog.Entity.Exception(ex);
			UnityEngine.Object.Destroy(unitEntityView.gameObject);
		}
		return unitHologram;
	}

	private static UnitEntityView SetupHologramPrefab(PrefabLink holoPrefabLink, BaseUnitEntity unit)
	{
		UnitEntityView component = UnityEngine.Object.Instantiate(holoPrefabLink.Load()).GetComponent<UnitEntityView>();
		if (component == null)
		{
			LogChannel.Default.Error(unit.View, "No UnitEntityView in HologramPrefab. No Hologram will be created. Check BlueprintFxRoot.");
			return null;
		}
		return component;
	}
}
