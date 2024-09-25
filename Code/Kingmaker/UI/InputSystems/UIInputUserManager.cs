using System;
using UnityEngine;

namespace Kingmaker.UI.InputSystems;

public class UIInputUserManager : MonoBehaviour
{
	private uint m_InputUserID;

	private void Start()
	{
	}

	private void OnUserServiceEvent(uint eventType, uint userid)
	{
		if (eventType != 0 && eventType == 1 && userid == m_InputUserID)
		{
			AssignPrimaryUserToUIInput();
		}
	}

	private int FindPrimaryUserSlot()
	{
		return -1;
	}

	private void SetInputUserSlot(int userSlot)
	{
	}

	private void AssignPrimaryUserToUIInput()
	{
		int num = FindPrimaryUserSlot();
		if (num < 0)
		{
			throw new Exception("Unable to find primary user slot for UI input.");
		}
		SetInputUserSlot(num);
	}

	private void Update()
	{
	}
}
