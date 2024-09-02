using System;

namespace UnityEngine.UI.Extensions;

public class ObjectIdentifier : MonoBehaviour
{
	public string prefabName;

	public string id;

	public string idParent;

	public bool dontSave;

	public void SetID()
	{
		id = Guid.NewGuid().ToString();
		CheckForRelatives();
	}

	private void CheckForRelatives()
	{
		if (base.transform.parent == null)
		{
			idParent = null;
			return;
		}
		ObjectIdentifier[] componentsInChildren = GetComponentsInChildren<ObjectIdentifier>();
		foreach (ObjectIdentifier objectIdentifier in componentsInChildren)
		{
			if (objectIdentifier.transform.gameObject != base.gameObject)
			{
				objectIdentifier.idParent = id;
				objectIdentifier.SetID();
			}
		}
	}
}
