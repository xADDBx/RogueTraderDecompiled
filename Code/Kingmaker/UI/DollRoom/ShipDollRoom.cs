using System.Collections;
using Kingmaker.Blueprints.Area;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Sound;
using Kingmaker.View;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace Kingmaker.UI.DollRoom;

public class ShipDollRoom : DollRoomBase, IDollCharacterDragUIHandler, ISubscriber
{
	private const float m_RotationSpeed = 0.045f;

	[FormerlySerializedAs("m_BackgroundTintColor")]
	[Header("Additional")]
	[SerializeField]
	private float m_ShipPlaceholderDefaultRotationY;

	[SerializeField]
	private Color m_BackgroundColor;

	[SerializeField]
	private GameObject m_Skydome;

	[SerializeField]
	private MaterialLink m_DefaultSkydomeMaterial;

	[SerializeField]
	private GameObject m_DefaultPlanet;

	[SerializeField]
	private GameObject m_PlanetRoot;

	private Quaternion m_ShipPlaceholderInitialRotationQuaternion;

	private GameObject m_Planet;

	private GameObject m_SimpleAvatar;

	private float m_RotationOffcet = 0.5f;

	public void SetupShip(BaseUnitEntity ship)
	{
		Cleanup();
		SetShipRotation();
		CreateSimpleAvatar(ship);
		SetupBackground();
	}

	protected override void Cleanup()
	{
		if ((bool)m_SimpleAvatar)
		{
			Object.Destroy(m_SimpleAvatar.gameObject);
			m_SimpleAvatar = null;
		}
		if ((bool)m_Planet)
		{
			Object.Destroy(m_Planet.gameObject);
			m_Planet = null;
		}
		base.Cleanup();
	}

	private void SetShipRotation()
	{
		m_ShipPlaceholderInitialRotationQuaternion.eulerAngles = new Vector3(0f, m_ShipPlaceholderDefaultRotationY, 0f);
		m_TargetPlaceholder.transform.rotation = m_ShipPlaceholderInitialRotationQuaternion;
	}

	private void CreateSimpleAvatar(BaseUnitEntity ship)
	{
		UnitEntityView unitEntityView = ship.View;
		if (unitEntityView == null)
		{
			unitEntityView = ship.CreateView();
			ship.AttachView(unitEntityView);
		}
		GameObject original = unitEntityView.GetComponentInChildren<StarshipView>().BaseRenderer.gameObject;
		m_SimpleAvatar = Object.Instantiate(original, m_TargetPlaceholder, worldPositionStays: false);
		m_SimpleAvatar.transform.localPosition = Vector3.zero;
		m_SimpleAvatar.transform.localRotation = Quaternion.identity;
		m_SimpleAvatar.transform.localScale = unitEntityView.transform.localScale;
	}

	public void UpdateStarshipRenderers()
	{
		if (m_SimpleAvatar != null)
		{
			MeshRenderer[] componentsInChildren = m_SimpleAvatar.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			foreach (MeshRenderer obj in componentsInChildren)
			{
				obj.enabled = true;
				obj.gameObject.layer = 15;
			}
			VisualEffect[] componentsInChildren2 = m_SimpleAvatar.GetComponentsInChildren<VisualEffect>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].gameObject.layer = 15;
			}
		}
		else
		{
			PFLog.TechArt.Error("m_SimpleAvatar is null");
		}
		PFLog.TechArt.Log("ShipDollRoom changed Layers to DollRoom");
	}

	private void SetupBackground()
	{
		if (!RootUIContext.Instance.IsShipInventoryShown)
		{
			return;
		}
		SpaceCombatBackgroundComposerConfigs spaceCombatBackgroundComposerConfigs = (Game.Instance.CurrentlyLoadedArea as BlueprintStarSystemMap)?.BackgroundComposerConfig;
		if (spaceCombatBackgroundComposerConfigs == null)
		{
			if (m_Skydome != null)
			{
				MaterialLink defaultSkydomeMaterial = m_DefaultSkydomeMaterial;
				if ((object)defaultSkydomeMaterial != null && defaultSkydomeMaterial.Exists())
				{
					m_Skydome.GetComponent<MeshRenderer>().material = Object.Instantiate(m_DefaultSkydomeMaterial.Load());
					m_Planet = Object.Instantiate(m_DefaultPlanet, m_PlanetRoot.transform);
				}
			}
		}
		else
		{
			m_Skydome.GetComponent<MeshRenderer>().material = spaceCombatBackgroundComposerConfigs.SkyDomeMaterial.Load();
			int num = 0;
			foreach (SpaceCombatBackgroundComposerConfigs.PlanetObjectProperties planet in spaceCombatBackgroundComposerConfigs.Planets)
			{
				GameObject gameObject = planet.Prefab.Load();
				if (gameObject != null && (bool)gameObject.GetComponentInChildren<MeshRenderer>())
				{
					break;
				}
				num++;
			}
			m_Planet = Object.Instantiate(spaceCombatBackgroundComposerConfigs.Planets[num].Prefab.Load(), m_PlanetRoot.transform);
		}
		m_Planet.AddComponent<ObjectRotator>().AngularSpeed.y = 0.2f;
		Transform[] componentsInChildren = m_Planet.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 15;
		}
		ColorizeBackground();
	}

	private void ColorizeBackground()
	{
		if (m_Skydome != null && m_Planet != null)
		{
			Material material = m_Skydome.GetComponent<MeshRenderer>().material;
			material.SetColor("_BackgroundColor", m_BackgroundColor);
			material.SetInt("_IsShipDollRoom", 1);
			MeshRenderer[] componentsInChildren = m_Planet.GetComponentsInChildren<MeshRenderer>();
			componentsInChildren[0].material.SetColor("_BackgroundColor", m_BackgroundColor);
			componentsInChildren[0].material.SetInt("_IsShipDollRoom", 1);
			componentsInChildren[0].material.SetInt("_IsPlanetDollRoom", 1);
			for (int i = 1; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].material.SetColor("_BaseColor", m_BackgroundColor);
			}
		}
		else
		{
			PFLog.TechArt.Error("m_Skydome or m_Planet is null");
		}
	}

	public override void Show()
	{
		base.Show();
		SetupDollPostProcessAndAnimation(isCharGen: false);
		UISounds.Instance.Sounds.Ship.ShipDollAnimationShow.Play();
	}

	public override void Hide()
	{
		base.Hide();
		Object.Destroy(m_SimpleAvatar);
		Object.Destroy(m_Planet);
	}

	public void StartDrag()
	{
		StopCoroutine(RotateBack());
		EventBus.RaiseEvent(delegate(IVoidShipRotationHandler h)
		{
			h.HandleOnRotationStart();
		});
	}

	public void EndDrag()
	{
		StopCoroutine(RotateBack());
	}

	public void RotateToDefaultPosition()
	{
		if (m_TargetPlaceholder.rotation.eulerAngles.y >= m_ShipPlaceholderInitialRotationQuaternion.eulerAngles.y + m_RotationOffcet || m_TargetPlaceholder.rotation.eulerAngles.y <= m_ShipPlaceholderInitialRotationQuaternion.eulerAngles.y - m_RotationOffcet)
		{
			StopCoroutine(RotateBack());
			StartCoroutine(RotateBack());
		}
	}

	private IEnumerator RotateBack()
	{
		while (m_TargetPlaceholder.rotation.eulerAngles.y >= m_ShipPlaceholderInitialRotationQuaternion.eulerAngles.y + m_RotationOffcet || m_TargetPlaceholder.rotation.eulerAngles.y <= m_ShipPlaceholderInitialRotationQuaternion.eulerAngles.y - m_RotationOffcet)
		{
			m_TargetPlaceholder.rotation = Quaternion.Lerp(m_TargetPlaceholder.rotation, m_ShipPlaceholderInitialRotationQuaternion, 0.045f);
			yield return null;
		}
		EventBus.RaiseEvent(delegate(IVoidShipRotationHandler h)
		{
			h.HandleOnRotationStop();
		});
	}
}
