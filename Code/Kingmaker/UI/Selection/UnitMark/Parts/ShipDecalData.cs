using System.Collections.Generic;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark.Parts;

public class ShipDecalData : MonoBehaviour
{
	public ShipShieldsUnitMarkPart[] ShipShields;

	public ShipDecalMeshData[] ShipDecals;

	public ShipDecalSize Size;

	public List<MeshRenderer> PingChangeColorRenderers;

	public List<Material> PingChangeColorMaterials;

	private bool m_ShieldsInitialized;

	public void InitializeShields(PartStarshipShields starshipShields, ShipDecalConfig shipDecalConfig)
	{
		if (!m_ShieldsInitialized)
		{
			ShipShieldsUnitMarkPart[] shipShields = ShipShields;
			for (int i = 0; i < shipShields.Length; i++)
			{
				shipShields[i].Initialize(starshipShields, shipDecalConfig);
			}
			m_ShieldsInitialized = true;
		}
	}

	public void SetShieldsVisible(bool isVisible, bool isDirect)
	{
		ShipShieldsUnitMarkPart[] shipShields = ShipShields;
		foreach (ShipShieldsUnitMarkPart shipShieldsUnitMarkPart in shipShields)
		{
			shipShieldsUnitMarkPart.gameObject.SetActive(isVisible && shipShieldsUnitMarkPart.IsDirect == isDirect);
		}
	}

	public void MarkHighlightShieldsHit(StarshipSectorShields shieldSector)
	{
		ShipShieldsUnitMarkPart[] shipShields = ShipShields;
		for (int i = 0; i < shipShields.Length; i++)
		{
			shipShields[i].MarkHighlightShieldHit(shieldSector);
		}
	}

	public void ClearHighlightShieldsHit()
	{
		ShipShieldsUnitMarkPart[] shipShields = ShipShields;
		for (int i = 0; i < shipShields.Length; i++)
		{
			shipShields[i].ClearHighlightShieldHit();
		}
	}

	public void SetShieldsHitHighlightVisible(bool isVisible)
	{
		ShipShieldsUnitMarkPart[] shipShields = ShipShields;
		for (int i = 0; i < shipShields.Length; i++)
		{
			shipShields[i].SetShieldHitHighlightVisible(isVisible);
		}
	}

	public void ShowShieldValues(bool isVisible)
	{
		ShipShieldsUnitMarkPart[] shipShields = ShipShields;
		for (int i = 0; i < shipShields.Length; i++)
		{
			shipShields[i].ShowShieldValues(isVisible);
		}
	}

	public void DisposeShields()
	{
		ShipShieldsUnitMarkPart[] shipShields = ShipShields;
		for (int i = 0; i < shipShields.Length; i++)
		{
			shipShields[i].Dispose();
		}
		m_ShieldsInitialized = false;
	}

	public void SwitchShipDecal(bool isDirect)
	{
		ShipDecalMeshData[] shipDecals = ShipDecals;
		foreach (ShipDecalMeshData shipDecalMeshData in shipDecals)
		{
			shipDecalMeshData.Renderer.gameObject.SetActive(shipDecalMeshData.IsDirect == isDirect);
		}
	}

	public void SetShipDecalsSelected(bool isSelected)
	{
		ShipDecalMeshData[] shipDecals = ShipDecals;
		for (int i = 0; i < shipDecals.Length; i++)
		{
			shipDecals[i].SetSelected(isSelected);
		}
	}

	public void SetCoopColorPingMaterial(int index)
	{
		if (PingChangeColorRenderers != null && PingChangeColorRenderers.Any() && PingChangeColorMaterials != null && PingChangeColorMaterials.Any())
		{
			PingChangeColorRenderers.ForEach(delegate(MeshRenderer p)
			{
				p.material = PingChangeColorMaterials[index];
			});
		}
	}
}
