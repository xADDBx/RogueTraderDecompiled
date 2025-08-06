using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.Settings;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.Controllers;

public class FootprintsController : IControllerTick, IController, IControllerStop, IUnitFootstepAnimationEventHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	private class UnitFeet
	{
		[CanBeNull]
		public UnitFootLocator ActingFoot;

		public int FootprintIndex;

		public Vector3 PreviousFootPosition;

		public bool InParty;
	}

	private class UnitFootLocator
	{
		public Transform Transform;

		public bool LeftSided;
	}

	public class Footprint
	{
		[NotNull]
		public readonly List<Footprint> Pool;

		public readonly GameObject GameObject;

		public readonly Transform Transform;

		public readonly MeshRenderer MeshRenderer;

		public List<Footprint> UnitList;

		public float? TimeLeft;

		public float? FadeOutTime;

		public Footprint(List<Footprint> pool, MeshRenderer meshRenderer)
		{
			Pool = pool;
			GameObject = meshRenderer.gameObject;
			Transform = meshRenderer.transform.transform;
			MeshRenderer = meshRenderer;
		}
	}

	private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");

	private static readonly int BaseAlphaScalePropertyId = Shader.PropertyToID("_AlphaScale");

	private static readonly int BaseBumpScalePropertyId = Shader.PropertyToID("_BumpScale");

	private static readonly int BaseRoughnessPropertyId = Shader.PropertyToID("_Roughness");

	private static readonly Dictionary<AbstractUnitEntity, UnitFeet> UnitsFeet = new Dictionary<AbstractUnitEntity, UnitFeet>();

	private static readonly Dictionary<GameObject, List<Footprint>> PooledFootprints = new Dictionary<GameObject, List<Footprint>>();

	private readonly List<Footprint> m_Footprints = new List<Footprint>();

	private readonly List<KeyValuePair<EntityRef<AbstractUnitEntity>, List<Footprint>>> m_UnitFootprints = new List<KeyValuePair<EntityRef<AbstractUnitEntity>, List<Footprint>>>();

	private readonly HashSet<AbstractUnitEntity> m_UpdateList = new HashSet<AbstractUnitEntity>();

	public void OnStop()
	{
		foreach (Footprint footprint in m_Footprints)
		{
			Cleanup(footprint);
			footprint.Pool.Add(footprint);
		}
		PooledFootprints.Clear();
		m_UnitFootprints.Clear();
		m_Footprints.Clear();
		UnitsFeet.Clear();
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		float deltaTime = Game.Instance.TimeController.DeltaTime;
		FxRoot fxRoot = BlueprintRoot.Instance.FxRoot;
		List<Footprint> list = TempList.Get<Footprint>();
		foreach (KeyValuePair<EntityRef<AbstractUnitEntity>, List<Footprint>> unitFootprint in m_UnitFootprints)
		{
			unitFootprint.Deconstruct(out var key, out var value);
			EntityRef<AbstractUnitEntity> entityRef = key;
			List<Footprint> list2 = value;
			Footprint footprint = list2.FirstItem();
			if (footprint != null)
			{
				int num = ((entityRef.Entity?.IsInPlayerParty ?? false) ? fxRoot.MaxFootprintsCountPerPlayerUnit : Mathf.RoundToInt((float)fxRoot.MaxFootprintsCountPerPlayerUnit * fxRoot.MaxFootprintsCountModForNPC));
				if ((double)list2.Count > (double)num * 0.75 || footprint.TimeLeft <= 0f)
				{
					footprint.TimeLeft = null;
					float num2 = ((list2.Count > num) ? (fxRoot.FadeOutTimeSeconds * (1f - 1f / (float)(list2.Count - num))) : 0f);
					footprint.FadeOutTime = ((!footprint.FadeOutTime.HasValue) ? num2 : Math.Max(num2, footprint.FadeOutTime.Value));
					footprint.MeshRenderer.material.renderQueue = 2000;
				}
			}
		}
		foreach (Footprint footprint2 in m_Footprints)
		{
			if (footprint2.FadeOutTime.HasValue)
			{
				footprint2.FadeOutTime += deltaTime;
				float progress = footprint2.FadeOutTime.Value / fxRoot.FadeOutTimeSeconds;
				UpdateFadeOut(footprint2.MeshRenderer, progress);
				if (footprint2.FadeOutTime >= fxRoot.FadeOutTimeSeconds)
				{
					list.Add(footprint2);
				}
			}
			else
			{
				float? timeLeft = footprint2.TimeLeft;
				if (timeLeft.HasValue && timeLeft.GetValueOrDefault() > 0f)
				{
					footprint2.TimeLeft -= deltaTime;
				}
			}
		}
		foreach (Footprint item in list)
		{
			m_Footprints.Remove(item);
			item.UnitList.Remove(item);
			item.Pool.Add(item);
			Cleanup(item);
		}
		foreach (AbstractUnitEntity update in m_UpdateList)
		{
			try
			{
				Claim(update);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
		m_UpdateList.Clear();
	}

	public void HandleUnitFootstepAnimationEvent(string locator, int footIndex)
	{
		AbstractUnitEntity abstractUnitEntity = EventInvokerExtensions.AbstractUnitEntity;
		if (abstractUnitEntity == null || !abstractUnitEntity.IsVisibleForPlayer)
		{
			return;
		}
		FootprintsMode value = SettingsRoot.Graphics.FootprintsMode.GetValue();
		if (value == FootprintsMode.Off)
		{
			return;
		}
		if (locator == string.Empty)
		{
			PFLog.TechArt.Error("FootprintsController: locator feeld in animation event PlaceFootstep is empty! Check that all fields are filled in animation event! " + abstractUnitEntity.View.name + " animation: " + abstractUnitEntity.AnimationManager.CurrentAction.ActiveAnimation?.GetActiveClip()?.name);
			return;
		}
		UnitPartCompanion optional = abstractUnitEntity.GetOptional<UnitPartCompanion>();
		bool flag = optional != null && optional.State == CompanionState.InParty;
		if (value == FootprintsMode.Party && !flag)
		{
			return;
		}
		UnitFootLocator locatorTransform = GetLocatorTransform(abstractUnitEntity, locator);
		if (locatorTransform != null)
		{
			UnitFeet unitFeet = new UnitFeet();
			unitFeet.InParty = flag;
			unitFeet.FootprintIndex = footIndex;
			unitFeet.ActingFoot = locatorTransform;
			if (UnitsFeet.ContainsKey(abstractUnitEntity))
			{
				UnitsFeet.Remove(abstractUnitEntity);
			}
			UnitsFeet.Add(abstractUnitEntity, unitFeet);
			m_UpdateList.Add(abstractUnitEntity);
		}
	}

	[CanBeNull]
	private static GameObject GetFootprintPrefab(AbstractUnitEntity unit, int footIndex)
	{
		PrefabLink[] footprintsOverride = unit.Blueprint.VisualSettings.FootprintsOverride;
		GameObject gameObject;
		if (footprintsOverride.Length != 0)
		{
			if (footprintsOverride.Length - 1 < footIndex)
			{
				PFLog.TechArt.Error("FootprintsController: index in PlaceFootprint event from " + unit.AnimationManager.CurrentAction.ActiveAnimation?.GetActiveClip()?.name + " animation is out of range for FootprintsOverride array in blueprint: " + unit.Blueprint.name);
			}
			else
			{
				gameObject = footprintsOverride[footIndex].Load();
				if (gameObject != null)
				{
					return gameObject;
				}
			}
		}
		int num = unit.View.Footprints.Length;
		if (num == 0)
		{
			string name = unit.View.CharacterAvatar.Skeleton.name;
			if (name.Contains("human", StringComparison.InvariantCultureIgnoreCase))
			{
				gameObject = BlueprintRoot.Instance.FxRoot.DefaultHumanFootprint.Load();
			}
			else if (name.Contains("eldar", StringComparison.InvariantCultureIgnoreCase))
			{
				gameObject = BlueprintRoot.Instance.FxRoot.DefaultEldarFootprint.Load();
			}
			else
			{
				if (!name.Contains("marine", StringComparison.InvariantCultureIgnoreCase))
				{
					return null;
				}
				gameObject = BlueprintRoot.Instance.FxRoot.DefaultSpaceMarineFootprint.Load();
			}
		}
		else
		{
			if (num - 1 < footIndex)
			{
				PFLog.TechArt.Error("FootprintsController: index in PlaceFootprint event from " + unit.AnimationManager.CurrentAction.ActiveAnimation?.GetActiveClip()?.name + " animation is out of range for Footprints array in prefab: " + unit.View.name);
				return null;
			}
			gameObject = unit.View.Footprints[footIndex];
		}
		if (gameObject == null)
		{
			return null;
		}
		return gameObject;
	}

	[CanBeNull]
	private Footprint Claim(AbstractUnitEntity unit)
	{
		if (unit.View == null)
		{
			return null;
		}
		FxRoot fxRoot = BlueprintRoot.Instance.FxRoot;
		UnitFeet unitFeet = UnitsFeet.Get(unit);
		if (Vector3.Distance(unitFeet.PreviousFootPosition, unitFeet.ActingFoot.Transform.position) < fxRoot.MinDistanceBetweenFootprints)
		{
			return null;
		}
		unitFeet.PreviousFootPosition = new Vector3(unitFeet.ActingFoot.Transform.position.x, unitFeet.ActingFoot.Transform.position.y, unitFeet.ActingFoot.Transform.position.z);
		SurfaceType? groundType = SurfaceTypeObject.GetSurfaceSoundTypeSwitch(unitFeet.ActingFoot.Transform.position);
		if (!groundType.HasValue)
		{
			if (unitFeet.InParty)
			{
				PFLog.Audio.Error("Can not create footprints, Surface Type Object returns null. Check sound surfaces setup.");
			}
			return null;
		}
		if (!fxRoot.FootprintsSettings.TryFind((FxRoot.FootprintSurfaceSettings x) => x.GroundType == groundType, out var result))
		{
			return null;
		}
		GameObject footprintPrefab = GetFootprintPrefab(unit, unitFeet.FootprintIndex);
		if (footprintPrefab == null)
		{
			return null;
		}
		MeshRenderer componentNonAlloc = footprintPrefab.GetComponentNonAlloc<MeshRenderer>();
		if (componentNonAlloc == null)
		{
			return null;
		}
		List<Footprint> list = PooledFootprints.Get(footprintPrefab);
		if (list == null)
		{
			list = new List<Footprint>();
			PooledFootprints.Add(footprintPrefab, list);
		}
		Footprint footprint = list.LastItem();
		FootprintsModification(unit);
		if (footprint == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(footprintPrefab, FxHelper.FootprintsRoot);
			if (unitFeet.ActingFoot.LeftSided)
			{
				Vector3 localScale = gameObject.transform.localScale;
				localScale.x *= -1f;
				gameObject.transform.localScale = localScale;
			}
			footprint = new Footprint(list, gameObject.GetComponentNonAlloc<MeshRenderer>());
		}
		else
		{
			list.RemoveLast();
		}
		List<Footprint> list2 = m_UnitFootprints.FirstItem((KeyValuePair<EntityRef<AbstractUnitEntity>, List<Footprint>> i) => i.Key == unit).Value;
		if (list2 == null)
		{
			if (unitFeet.InParty)
			{
				list2 = new List<Footprint>
				{
					Capacity = fxRoot.MaxFootprintsCountPerPlayerUnit
				};
			}
			else
			{
				float num = (float)fxRoot.MaxFootprintsCountPerPlayerUnit * fxRoot.MaxFootprintsCountModForNPC;
				list2 = new List<Footprint>
				{
					Capacity = (int)num
				};
			}
			m_UnitFootprints.Add(new KeyValuePair<EntityRef<AbstractUnitEntity>, List<Footprint>>(unit, list2));
		}
		m_Footprints.Add(footprint);
		footprint.UnitList = list2;
		list2.Add(footprint);
		footprint.MeshRenderer.material = componentNonAlloc.sharedMaterial;
		footprint.MeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		footprint.MeshRenderer.material.SetFloat(BaseAlphaScalePropertyId, result.FootprintAlphaScale);
		footprint.MeshRenderer.material.SetFloat(BaseBumpScalePropertyId, result.FootprintBumpScale);
		footprint.MeshRenderer.material.SetFloat(BaseRoughnessPropertyId, result.FootprintRoughness);
		Color footprintTintColor = result.FootprintTintColor;
		footprintTintColor.a = 1f;
		footprint.MeshRenderer.material.SetColor(BaseColorPropertyId, footprintTintColor);
		if (unitFeet.ActingFoot != null)
		{
			footprint.Transform.position = unitFeet.ActingFoot.Transform.position;
			footprint.Transform.rotation = unitFeet.ActingFoot.Transform.rotation;
		}
		footprint.TimeLeft = fxRoot.DefaultLifetimeSeconds;
		footprint.GameObject.name = "footprint_" + unit.View.name + "_" + groundType.ToString();
		footprint.GameObject.SetActive(value: true);
		return footprint;
	}

	private static void Cleanup(Footprint footprint)
	{
		footprint.UnitList = null;
		footprint.TimeLeft = null;
		footprint.FadeOutTime = null;
		UnityEngine.Object.Destroy(footprint.MeshRenderer.material);
		footprint.GameObject.SetActive(value: false);
	}

	private static void UpdateFadeOut(MeshRenderer meshRenderer, float progress)
	{
		progress = Math.Max(0f, Math.Min(1f, progress));
		SetAlpha(meshRenderer, 1f - progress);
	}

	private static void SetAlpha(MeshRenderer meshRenderer, float alpha)
	{
		if ((bool)meshRenderer)
		{
			Color color = meshRenderer.material.GetColor(BaseColorPropertyId);
			color.a = alpha;
			meshRenderer.material.SetColor(BaseColorPropertyId, color);
		}
	}

	private static UnitFootLocator GetLocatorTransform(AbstractUnitEntity unit, string locator)
	{
		UnitFootLocator unitFootLocator = new UnitFootLocator();
		unitFootLocator.Transform = unit.View.ParticlesSnapMap.Bones.FirstOrDefault((FxBone b) => b.Name == locator)?.Transform;
		if (locator.Contains("left", StringComparison.InvariantCultureIgnoreCase))
		{
			unitFootLocator.LeftSided = true;
		}
		else
		{
			unitFootLocator.LeftSided = false;
		}
		if (unitFootLocator.Transform == null)
		{
			PFLog.TechArt.Error("FootprintsController: " + locator + " not found in " + unit.View.name + " for animation: " + unit.AnimationManager.CurrentAction.ActiveAnimation?.GetActiveClip()?.name);
			return null;
		}
		return unitFootLocator;
	}

	private static void FootprintsModification(AbstractUnitEntity unit)
	{
		foreach (Buff buff in unit.Buffs)
		{
			FootprintModification component = buff.GetComponent<FootprintModification>();
			if (component != null)
			{
				FxHelper.SpawnFxOnEntity(component.PrefabToAdd?.Load(), unit.View);
			}
		}
	}
}
