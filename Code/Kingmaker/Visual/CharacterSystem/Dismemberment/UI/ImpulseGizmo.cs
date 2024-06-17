using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem.Dismemberment.UI;

public class ImpulseGizmo : MonoBehaviour
{
	private DismembermentPieceDescriptor m_Piece;

	internal void Init(DismembermentPieceDescriptor piece)
	{
		m_Piece = piece;
	}

	private void Update()
	{
		if (m_Piece.ImpulseRigidBodies[0] != null)
		{
			Transform transform = m_Piece.ImpulseRigidBodies[0].transform;
			base.transform.position = transform.position;
			Vector3 toDirection = transform.TransformDirection(m_Piece.Impulse);
			base.transform.rotation = Quaternion.FromToRotation(Vector3.forward, toDirection);
		}
	}
}
