using Kingmaker.View;
using UnityEngine;

namespace Kingmaker;

public class SolarSystemArCell : MonoBehaviour
{
	public Animator CellAnimator;

	private MeshRenderer m_Renderer;

	private bool m_Occupied;

	private static readonly int In = Animator.StringToHash("In");

	private static readonly int Out = Animator.StringToHash("Out");

	private void Start()
	{
		if (CellAnimator == null)
		{
			CellAnimator = GetComponent<Animator>();
		}
		if (m_Renderer == null)
		{
			m_Renderer = GetComponent<MeshRenderer>();
		}
		if (CellAnimator != null)
		{
			CellAnimator.enabled = false;
		}
		if (m_Renderer != null)
		{
			m_Renderer.enabled = false;
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if ((bool)collider.transform.parent && (bool)collider.transform.parent.transform.parent && !(collider.transform.parent.transform.parent.GetComponent<UnitEntityView>() == null) && !m_Occupied)
		{
			CellAnimator.enabled = true;
			m_Renderer.enabled = true;
			m_Occupied = true;
			CellAnimator.SetTrigger(In);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (!(collider.transform.parent.transform.parent.GetComponent<UnitEntityView>() == null) && m_Occupied)
		{
			m_Occupied = false;
			CellAnimator.SetTrigger(Out);
		}
	}

	private void OutAnimationEnded()
	{
		if ((bool)CellAnimator)
		{
			CellAnimator.enabled = false;
		}
		if ((bool)m_Renderer)
		{
			m_Renderer.enabled = false;
		}
	}
}
