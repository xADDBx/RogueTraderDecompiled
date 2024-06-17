using System.Collections.Generic;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Root;
using Kingmaker.GameModes;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.ResourceLinks;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.SpaceCombat.SpaceCombatBackgroundComposer;

public class SpaceCombatBackgroundComposer : MonoBehaviour
{
	[Space(30f)]
	public GameObject BackgroundCameraPrefab;

	public SpaceCombatBackgroundComposerConfigs DefaultBackGroundComposerConfig;

	private GameObject m_BackgroundCameraObject;

	private SpaceCombatBackgroundComposerConfigs m_BackGroundComposerConfig;

	private GameObject m_SkyDome;

	private GameObject m_SpaceObjectsRoot;

	private GameObject m_Star;

	private GameObject m_Planets;

	private GameObject m_SpaceObjects;

	private GameObject m_Fx;

	private GameObject m_UnitsLight;

	private List<GameObject> m_CreatedPlanets = new List<GameObject>();

	private List<GameObject> m_CreatedSpaceObjects = new List<GameObject>();

	public SpaceCombatBackgroundComposerConfigs CurrentAsset => m_BackGroundComposerConfig;

	private void OnDestroy()
	{
		if ((bool)m_UnitsLight)
		{
			Object.Destroy(m_UnitsLight);
		}
	}

	private void Awake()
	{
		if (BackgroundCameraPrefab == null)
		{
			PFLog.TechArt.Error("BackgroundCameraPrefab not found in SpaceCombatBackgroundComposer prefab");
			return;
		}
		GetBackgroundComposerConfig();
		if (m_BackGroundComposerConfig == null)
		{
			PFLog.TechArt.Error("BackgroundComposerConfig not found. Script work is terminated!");
			return;
		}
		if (Game.Instance.CurrentMode == GameModeType.StarSystem)
		{
			CreateSkyDomeStarSystem();
			return;
		}
		CreateBackgroundCamera();
		CreateRoots();
		CreateSkyDome(m_BackGroundComposerConfig.SystemRadius * 2f);
		CreateStar();
		CreatePlanets();
		CreateSpaceObject();
		ChangeLayer();
		ChangeLightLayer();
		CreateFx();
	}

	private void GetBackgroundComposerConfig()
	{
		BlueprintStarSystemMap blueprintStarSystemMap = ((!(Game.Instance.CurrentMode == GameModeType.SpaceCombat)) ? (Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap) : Game.Instance.Player.CurrentStarSystem);
		if (blueprintStarSystemMap != null)
		{
			m_BackGroundComposerConfig = blueprintStarSystemMap.BackgroundComposerConfig;
		}
		else if ((bool)DefaultBackGroundComposerConfig)
		{
			m_BackGroundComposerConfig = DefaultBackGroundComposerConfig;
		}
		else
		{
			PFLog.TechArt.Error("DefaultBackGroundComposerConfig not found");
		}
	}

	public void CreateBackgroundCamera()
	{
		m_BackgroundCameraObject = Object.Instantiate(BackgroundCameraPrefab, base.gameObject.transform);
	}

	private void CreateRoots()
	{
		m_SpaceObjectsRoot = new GameObject();
		m_SpaceObjectsRoot.name = "SpaceCombatSpaceObjects";
		m_SpaceObjectsRoot.transform.parent = base.gameObject.transform;
		m_Star = new GameObject();
		m_Star.name = "Star";
		m_Star.transform.parent = m_SpaceObjectsRoot.transform;
		m_Planets = new GameObject();
		m_Planets.name = "Planets";
		m_Planets.transform.parent = m_SpaceObjectsRoot.transform;
		m_SpaceObjects = new GameObject();
		m_SpaceObjects.name = "SpaceObjects";
		m_SpaceObjects.transform.parent = m_SpaceObjectsRoot.transform;
		m_Fx = new GameObject();
		m_Fx.name = "FX";
		m_Fx.transform.parent = base.gameObject.transform;
		m_SpaceObjectsRoot.transform.position = new Vector3(m_BackGroundComposerConfig.SystemOffset.x, m_BackGroundComposerConfig.SystemOffset.y, m_BackGroundComposerConfig.SystemOffset.z);
	}

	public void CreateSkyDome(float skyDomeScale)
	{
		if (BlueprintRoot.Instance.SkyDome != null)
		{
			m_SkyDome = Object.Instantiate(BlueprintRoot.Instance.SkyDome, base.gameObject.transform);
			m_SkyDome.transform.localScale = new Vector3(skyDomeScale, skyDomeScale, skyDomeScale);
			MaterialLink skyDomeMaterial = m_BackGroundComposerConfig.SkyDomeMaterial;
			if ((object)skyDomeMaterial != null && skyDomeMaterial.Exists())
			{
				m_SkyDome.GetComponent<MeshRenderer>().material = m_BackGroundComposerConfig.SkyDomeMaterial.Load();
			}
			else
			{
				PFLog.TechArt.Error("SkyDome material not found in " + m_BackGroundComposerConfig.name);
			}
		}
		else
		{
			PFLog.TechArt.Error("SkyDome prefab not found");
		}
	}

	public void CreateStar()
	{
		Object.Instantiate(m_BackGroundComposerConfig.Star.Prefab.Load(), m_Star.transform);
		m_Star.transform.localScale = new Vector3(m_BackGroundComposerConfig.Star.Scale, m_BackGroundComposerConfig.Star.Scale, m_BackGroundComposerConfig.Star.Scale);
		if (BlueprintRoot.Instance.FxRoot.StarLight != null)
		{
			Object.Instantiate(BlueprintRoot.Instance.FxRoot.StarLight, m_Star.transform);
		}
		else
		{
			PFLog.TechArt.Error("StarLight prefab in FxRoot-> SpaceCombat is empty");
		}
		if (BlueprintRoot.Instance.FxRoot.UnitsLight != null)
		{
			m_UnitsLight = Object.Instantiate(BlueprintRoot.Instance.FxRoot.UnitsLight, CameraRig.Instance.CameraAttachPoint);
		}
		else
		{
			PFLog.TechArt.Error("UnitsLight prefab in FxRoot-> SpaceCombat is empty");
		}
	}

	private void CreatePlanets()
	{
		float num = m_BackGroundComposerConfig.SystemRadius / (float)(m_BackGroundComposerConfig.Planets.Count + 1);
		int num2 = 1;
		foreach (SpaceCombatBackgroundComposerConfigs.PlanetObjectProperties planet in m_BackGroundComposerConfig.Planets)
		{
			GameObject gameObject = Object.Instantiate(planet.Prefab.Load(), m_Planets.transform);
			gameObject.transform.localPosition = new Vector3(0f, planet.OffsetY, num * (float)num2);
			gameObject.transform.localScale = new Vector3(planet.Scale, planet.Scale, planet.Scale);
			gameObject.transform.RotateAround(m_SpaceObjectsRoot.transform.position, Vector3.up, planet.AngleOnOrbit);
			num2++;
			m_CreatedPlanets.Add(gameObject);
			gameObject.AddComponent<ObjectRotator>().AngularSpeed.y = 0.4f;
		}
	}

	private void CreateSpaceObject()
	{
		if (m_BackGroundComposerConfig.SpaceObject.Prefab.Exists())
		{
			GameObject gameObject = Object.Instantiate(m_BackGroundComposerConfig.SpaceObject.Prefab.Load(), m_SpaceObjects.transform);
			gameObject.transform.localPosition = m_BackGroundComposerConfig.SpaceObject.Position;
			gameObject.transform.localScale = new Vector3(m_BackGroundComposerConfig.SpaceObject.Scale, m_BackGroundComposerConfig.SpaceObject.Scale, m_BackGroundComposerConfig.SpaceObject.Scale);
			gameObject.transform.localRotation = m_BackGroundComposerConfig.SpaceObject.Rotation;
			m_CreatedSpaceObjects.Add(gameObject);
		}
	}

	public void CreateFx()
	{
		if (BlueprintRoot.Instance.FxRoot.StarLight != null)
		{
			Object.Instantiate(BlueprintRoot.Instance.FxRoot.FxSpaceCombat, m_Fx.transform);
		}
		else
		{
			PFLog.TechArt.Error("FxSpaceCombat prefab in FxRoot-> SpaceCombat is empty");
		}
	}

	public void ChangeLayer()
	{
		Transform[] componentsInChildren = base.gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 25;
		}
	}

	public void ChangeLightLayer()
	{
		MeshRenderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].renderingLayerMask = 4u;
		}
	}

	private void CreateSkyDomeStarSystem()
	{
		CreateSkyDome(150f);
		if (BlueprintRoot.Instance.SkyDomeStarSystemFader != null)
		{
			m_SkyDome = Object.Instantiate(BlueprintRoot.Instance.SkyDomeStarSystemFader, base.gameObject.transform);
		}
		else
		{
			PFLog.TechArt.Error("SkyDomeStarSystemFader prefab in BlueprintRoot -> SpaceCombat not found");
		}
	}
}
