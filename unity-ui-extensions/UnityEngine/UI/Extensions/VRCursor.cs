namespace UnityEngine.UI.Extensions;

[AddComponentMenu("UI/Extensions/VR Cursor")]
public class VRCursor : MonoBehaviour
{
	public float xSens;

	public float ySens;

	private Collider currentCollider;

	private void Update()
	{
		Vector3 position = default(Vector3);
		position.x = Input.mousePosition.x * xSens;
		position.y = Input.mousePosition.y * ySens - 1f;
		position.z = base.transform.position.z;
		base.transform.position = position;
		VRInputModule.cursorPosition = base.transform.position;
		if (Input.GetMouseButtonDown(0) && (bool)currentCollider)
		{
			VRInputModule.PointerSubmit(currentCollider.gameObject);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		VRInputModule.PointerEnter(other.gameObject);
		currentCollider = other;
	}

	private void OnTriggerExit(Collider other)
	{
		VRInputModule.PointerExit(other.gameObject);
		currentCollider = null;
	}
}
