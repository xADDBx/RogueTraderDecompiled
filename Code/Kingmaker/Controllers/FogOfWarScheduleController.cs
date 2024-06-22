using System;
using System.Collections.Generic;
using Kingmaker.Controllers.FogOfWar.Culling;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.SceneHelpers;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Controllers;

public class FogOfWarScheduleController : IControllerTick, IController, IControllerStop, IControllerStart
{
	private bool? m_IsFogOfWarPreviouslyEnabled;

	private static PerFrameVar<FogOfWarFeature> m_FowFeature = new PerFrameVar<FogOfWarFeature>();

	private static FogOfWarFeature FowFeature
	{
		get
		{
			if (m_FowFeature.UpToDate)
			{
				return m_FowFeature.Value;
			}
			m_FowFeature.Value = FogOfWarControllerData.GetFogOfWarFeature();
			return m_FowFeature.Value;
		}
	}

	public static bool FowIsActive
	{
		get
		{
			if (FowFeature != null && FogOfWarArea.Active != null)
			{
				return FogOfWarArea.Active.isActiveAndEnabled;
			}
			return false;
		}
	}

	public bool FowStateIsSet => m_IsFogOfWarPreviouslyEnabled.HasValue;

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	public FogOfWarScheduleController()
	{
		FogOfWarCullingBlocker.IsStatic = (FogOfWarBlocker blocker) => IsBlockerStatic(blocker);
	}

	public void OnStart()
	{
		((IControllerTick)this).Tick();
		((IControllerTick)Game.Instance.FogOfWarComplete).Tick();
	}

	private bool IsBlockerStatic(FogOfWarBlocker blocker)
	{
		if (blocker.gameObject.isStatic)
		{
			return true;
		}
		StaticPrefab staticPrefab = blocker.transform.parent?.parent?.GetComponent<StaticPrefab>();
		if (staticPrefab == null || staticPrefab.VisualRoot == null)
		{
			return blocker.gameObject.isStatic;
		}
		foreach (Transform item in staticPrefab.VisualRoot.transform)
		{
			if (!item.gameObject.isStatic)
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsRevealer(BaseUnitEntity unit)
	{
		using (ProfileScope.New("IsRevealer"))
		{
			return unit.Faction.IsPlayer && (Game.Instance.Player.PartyAndPets.HasItem(unit) || Game.Instance.Player.PlayerShip == unit) && unit.View != null && unit.IsViewActive && !unit.LifeState.IsDead;
		}
	}

	void IControllerTick.Tick()
	{
		FogOfWarFeature fogOfWarFeature = FogOfWarControllerData.GetFogOfWarFeature();
		if ((fogOfWarFeature == null && Application.isPlaying) || (bool)FogOfWarControllerData.Suppressed)
		{
			return;
		}
		if (!FowIsActive)
		{
			if (m_IsFogOfWarPreviouslyEnabled.HasValue && !m_IsFogOfWarPreviouslyEnabled.Value)
			{
				return;
			}
			foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
			{
				allUnit.IsInFogOfWar = false;
				allUnit.IsRevealed = true;
			}
			foreach (MapObjectEntity mapObject in Game.Instance.State.MapObjects)
			{
				mapObject.IsInFogOfWar = false;
				mapObject.IsRevealed = true;
			}
			m_IsFogOfWarPreviouslyEnabled = false;
			return;
		}
		m_IsFogOfWarPreviouslyEnabled = true;
		FogOfWarControllerData.CleanupRevealers();
		NativeList<RevealerProperties> revealers = new NativeList<RevealerProperties>(16, AllocatorManager.Temp);
		try
		{
			FogOfWarRevealer.All.Clear();
			CollectRevealers(fogOfWarFeature, revealers, FogOfWarControllerData.GetAdditionalRevealers());
			using (ProfileScope.New("ScheduleUpdate"))
			{
				FogOfWarCulling.ScheduleUpdate(in revealers);
			}
			using (ProfileScope.New("ScheduleBatchedJobs"))
			{
				JobHandle.ScheduleBatchedJobs();
			}
		}
		finally
		{
			((IDisposable)revealers).Dispose();
		}
	}

	public void OnStop()
	{
		m_IsFogOfWarPreviouslyEnabled = null;
	}

	private static void CollectRevealers(FogOfWarFeature fowFeature, NativeList<RevealerProperties> results, IEnumerable<Transform> additionalRevealers)
	{
		using (ProfileScope.New("Collect Revealers"))
		{
			using (ProfileScope.New("Units"))
			{
				foreach (BaseUnitEntity allBaseUnit in Game.Instance.State.AllBaseUnits)
				{
					if (allBaseUnit.View == null)
					{
						continue;
					}
					FogOfWarRevealerSettings fogOfWarRevealerSettings = allBaseUnit.View.FogOfWarRevealer;
					bool flag = fogOfWarRevealerSettings != null && fogOfWarRevealerSettings.enabled;
					if (IsRevealer(allBaseUnit) || (allBaseUnit.IsInCombat && !allBaseUnit.LifeState.IsDead))
					{
						if (!flag)
						{
							using (ProfileScope.New("Enable Revealer"))
							{
								fogOfWarRevealerSettings = allBaseUnit.View.SureFogOfWarRevealer();
								fogOfWarRevealerSettings.Enable();
								fogOfWarRevealerSettings.DefaultRadius = allBaseUnit.Faction.IsPlayer;
								fogOfWarRevealerSettings.Radius = 1f;
							}
						}
						RevealerProperties revealerProperties = default(RevealerProperties);
						revealerProperties.Center = new float2(allBaseUnit.Position.x, allBaseUnit.Position.z);
						revealerProperties.Radius = CalculateFullRevealerRadius(fogOfWarRevealerSettings);
						revealerProperties.HeightMin = fogOfWarRevealerSettings.Revealer.HeightMinMax.x;
						revealerProperties.HeightMax = fogOfWarRevealerSettings.Revealer.HeightMinMax.y;
						RevealerProperties value = revealerProperties;
						results.Add(in value);
						if (fowFeature != null)
						{
							FogOfWarRevealer.All.Add(fogOfWarRevealerSettings.Revealer);
						}
					}
					else if (flag)
					{
						fogOfWarRevealerSettings.Disable();
					}
				}
			}
			using (ProfileScope.New("Additional"))
			{
				List<RevealerProperties> value2;
				using (CollectionPool<List<RevealerProperties>, RevealerProperties>.Get(out value2))
				{
					foreach (Transform additionalRevealer in additionalRevealers)
					{
						bool alreadyExists;
						FogOfWarRevealerSettings fogOfWarRevealerSettings2 = additionalRevealer.EnsureComponent<FogOfWarRevealerSettings>(out alreadyExists);
						if (!alreadyExists)
						{
							fogOfWarRevealerSettings2.Radius = 22f;
						}
						RevealerProperties revealerProperties = default(RevealerProperties);
						revealerProperties.Center = new float2(fogOfWarRevealerSettings2.Revealer.Position.x, fogOfWarRevealerSettings2.Revealer.Position.z);
						revealerProperties.Radius = CalculateFullRevealerRadius(fogOfWarRevealerSettings2);
						revealerProperties.HeightMin = fogOfWarRevealerSettings2.Revealer.HeightMinMax.x;
						revealerProperties.HeightMax = fogOfWarRevealerSettings2.Revealer.HeightMinMax.y;
						RevealerProperties item = revealerProperties;
						value2.Add(item);
						if (fowFeature != null)
						{
							FogOfWarRevealer.All.Add(fogOfWarRevealerSettings2.Revealer);
						}
					}
					using (ProfileScope.New("Sort"))
					{
						value2.Sort(RevealerProperties.Comparison);
					}
					using (ProfileScope.New("Copy"))
					{
						foreach (RevealerProperties item2 in value2)
						{
							RevealerProperties value3 = item2;
							results.Add(in value3);
						}
					}
				}
			}
		}
	}

	private static float CalculateFullRevealerRadius(FogOfWarRevealerSettings revealer)
	{
		Transform transform = revealer.transform;
		if (!revealer.DefaultRadius)
		{
			if (!(revealer.Radius < 0.01f))
			{
				return revealer.Radius;
			}
			return transform.localScale.x / 2f;
		}
		return 22f;
	}
}
