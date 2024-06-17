using Kingmaker.View.Covers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.CoverVisualiserSystem;

[ExecuteInEditMode]
public class CoverCellController : MonoBehaviour
{
	[FormerlySerializedAs("redColor")]
	public Color halfCoverColor;

	[FormerlySerializedAs("blueColor")]
	public Color fullCoverColor;

	[FormerlySerializedAs("halfShield")]
	public Material halfCoverMaterial;

	[FormerlySerializedAs("fullShield")]
	public Material fullCoverMaterial;

	private CoverSideGenerator[] m_CoverSideGenerators;

	public bool IsCentral { get; set; }

	private void Awake()
	{
		m_CoverSideGenerators = GetComponentsInChildren<CoverSideGenerator>();
	}

	public void ChangeCoverIndicator(LosCalculations.CoverType front, LosCalculations.CoverType right, LosCalculations.CoverType back, LosCalculations.CoverType left)
	{
		CoverSideGenerator[] coverSideGenerators = m_CoverSideGenerators;
		foreach (CoverSideGenerator coverSideGenerator in coverSideGenerators)
		{
			switch (coverSideGenerator.side)
			{
			case Side.Front:
				ChangeStatusAndColorOnSide(coverSideGenerator, front);
				break;
			case Side.Right:
				ChangeStatusAndColorOnSide(coverSideGenerator, right);
				break;
			case Side.Back:
				ChangeStatusAndColorOnSide(coverSideGenerator, back);
				break;
			case Side.Left:
				ChangeStatusAndColorOnSide(coverSideGenerator, left);
				break;
			case Side.Bottom:
			case Side.Top:
				ChangeStatusAndColorOnSide(coverSideGenerator, LosCalculations.CoverType.None);
				break;
			}
		}
	}

	private void ChangeStatusAndColorOnSide(CoverSideGenerator coverSideGenerator, LosCalculations.CoverType status)
	{
		switch (status)
		{
		case LosCalculations.CoverType.Full:
			coverSideGenerator.gameObject.SetActive(value: true);
			coverSideGenerator.ChangeMaterial(fullCoverMaterial, fullCoverColor);
			break;
		case LosCalculations.CoverType.Half:
			coverSideGenerator.gameObject.SetActive(value: true);
			coverSideGenerator.ChangeMaterial(halfCoverMaterial, halfCoverColor);
			break;
		default:
			coverSideGenerator.gameObject.SetActive(value: false);
			break;
		}
	}
}
