using Kingmaker.Visual.CharacterSystem;
using UnityEngine;

public class DollRoomFootOcclusion : MonoBehaviour
{
	[SerializeField]
	public GameObject[] m_FootOccluders;

	private Character m_Character;

	private void Start()
	{
		m_Character = GetComponentInParent<Character>();
		if (m_Character != null && m_Character.IsInDollRoom)
		{
			ActivateOccluders();
		}
	}

	public void ActivateOccluders()
	{
		if (m_FootOccluders != null)
		{
			GameObject[] footOccluders = m_FootOccluders;
			for (int i = 0; i < footOccluders.Length; i++)
			{
				footOccluders[i].SetActive(value: true);
			}
		}
	}
}
