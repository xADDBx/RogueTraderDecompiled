using UnityEngine;

public class FX_PatentDeviceController : MonoBehaviour
{
	public Material[] _materials;

	public string _materialProperty = "_MainTex_Offset_Scroll";

	[Range(-1f, 1f)]
	public float _scrollSpeed;

	public float _scrollSpeedValueCurrent;

	[Header("Rotation Settings")]
	public Transform[] _objectsToRotateForward;

	public Vector3 _rotationSpeedForward = new Vector3(75f, 0f, 0f);

	public Transform[] _objectsToRotateBackrward;

	public Vector3 _rotationSpeedBackward = new Vector3(-150f, 0f, 0f);

	private void Update()
	{
		_scrollSpeedValueCurrent += _scrollSpeed * Time.deltaTime;
		Material[] materials = _materials;
		for (int i = 0; i < materials.Length; i++)
		{
			materials[i].SetFloat(_materialProperty, _scrollSpeedValueCurrent);
		}
		Transform[] objectsToRotateForward = _objectsToRotateForward;
		for (int i = 0; i < objectsToRotateForward.Length; i++)
		{
			objectsToRotateForward[i].Rotate(_rotationSpeedForward * _scrollSpeed * Time.deltaTime);
		}
		objectsToRotateForward = _objectsToRotateBackrward;
		for (int i = 0; i < objectsToRotateForward.Length; i++)
		{
			objectsToRotateForward[i].Rotate(_rotationSpeedBackward * _scrollSpeed * Time.deltaTime);
		}
	}
}
