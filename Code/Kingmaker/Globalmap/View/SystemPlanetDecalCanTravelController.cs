using DG.Tweening;
using Kingmaker.Globalmap.SectorMap;
using UnityEngine;

namespace Kingmaker.Globalmap.View;

public class SystemPlanetDecalCanTravelController : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer m_Decal;

	[SerializeField]
	private Color m_SafePassageColor = new Color(0.5246991f, 0.7924528f, 0.2878248f, 0.8f);

	[SerializeField]
	private Color m_UnsafePassageColor = new Color(0.9245283f, 0.8604134f, 0.2136882f, 0.8f);

	[SerializeField]
	private Color m_DangerousPassageColor = new Color(0.8490566f, 0.6085173f, 0.2763439f, 0.8f);

	[SerializeField]
	private Color m_DeadlyPassageColor = new Color(0.8301887f, 0.2271487f, 0.1605553f, 0.8f);

	[SerializeField]
	private float m_DurationRotate = 100f;

	[SerializeField]
	private float m_ScaleEndValue = 0.9f;

	[SerializeField]
	private float m_DurationScale = 3f;

	private SectorMapObject m_SectorMapObject;

	private bool Visible
	{
		set
		{
			m_Decal.enabled = value;
		}
	}

	public void Initialize(SectorMapObject sectorMapObject)
	{
		m_SectorMapObject = sectorMapObject;
		base.name = sectorMapObject.name + "_SystemPlanetDecalCanTravel";
		m_Decal.gameObject.transform.DORotate(new Vector3(0f, 0f, 360f), m_DurationRotate).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear)
			.SetRelative();
		m_Decal.gameObject.transform.DOScale(m_ScaleEndValue, m_DurationScale).SetLoops(-1, LoopType.Yoyo);
	}

	public void SetDecalColor(SectorMapPassageEntity.PassageDifficulty dif)
	{
		SpriteRenderer decal = m_Decal;
		decal.color = dif switch
		{
			SectorMapPassageEntity.PassageDifficulty.Safe => m_SafePassageColor, 
			SectorMapPassageEntity.PassageDifficulty.Unsafe => m_UnsafePassageColor, 
			SectorMapPassageEntity.PassageDifficulty.Dangerous => m_DangerousPassageColor, 
			SectorMapPassageEntity.PassageDifficulty.Deadly => m_DeadlyPassageColor, 
			_ => m_Decal.color, 
		};
	}

	public void SetVisibility(bool visible)
	{
		Visible = visible;
	}
}
