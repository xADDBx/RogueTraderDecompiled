using Kingmaker.UI.DollRoom;
using Owlcat.Runtime.Visual.SceneHelpers;
using UnityEngine;

namespace Kingmaker;

public class PlanetDollRoom : DollRoomBase
{
	private GameObject m_SimpleAvatar;

	public void SetupPlanet(GameObject planet)
	{
		Cleanup();
		CreateSimpleAvatar(planet);
	}

	private void CreateSimpleAvatar(GameObject planet)
	{
		MeshRenderer componentInChildren = planet.GetComponentInChildren<MeshRenderer>();
		if (componentInChildren == null)
		{
			PFLog.TechArt.Error("Can't find MeshRenderer component in {0}", planet.name);
			return;
		}
		m_SimpleAvatar = Object.Instantiate(componentInChildren.transform.parent.gameObject, m_TargetPlaceholder);
		Object.Destroy(m_SimpleAvatar.GetComponent<TransformFreezer>());
		m_SimpleAvatar.hideFlags = HideFlags.None;
		m_SimpleAvatar.transform.position = m_TargetPlaceholder.position;
		m_SimpleAvatar.transform.rotation = m_TargetPlaceholder.rotation;
		m_SimpleAvatar.transform.localScale = m_TargetPlaceholder.localScale;
		ChangeLayer(m_SimpleAvatar);
		MeshRenderer[] componentsInChildren = m_SimpleAvatar.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		foreach (MeshRenderer obj in componentsInChildren)
		{
			obj.material.SetInt("_Rim_light", 0);
			obj.material.SetInt("_IsPlanetDollRoom", 1);
		}
	}

	public override void Hide()
	{
		base.Hide();
		if (!(m_SimpleAvatar == null))
		{
			MeshRenderer[] componentsInChildren = m_SimpleAvatar.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i].material);
			}
			Object.Destroy(m_SimpleAvatar);
		}
	}

	private void ChangeLayer(GameObject avatar)
	{
		Transform[] componentsInChildren = avatar.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = 15;
		}
	}
}
