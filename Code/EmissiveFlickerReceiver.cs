using UnityEngine;

public class EmissiveFlickerReceiver : MonoBehaviour
{
	[Tooltip("Уникальный ID — должен точно совпадать с ID в LightFlickerSimple")]
	public string flickerID = "";

	[Tooltip("Индекс эмиссивного материала (смотри в Inspector меша: Element 0, 1, 2...)")]
	public int materialIndex;

	private Material _instanceMaterial;

	private static readonly int EmissionColorScaleID = Shader.PropertyToID("_EmissionColorScale");

	private void Awake()
	{
		Renderer component = GetComponent<Renderer>();
		if (component == null)
		{
			Debug.LogError("[EmissiveFlickerReceiver] No Renderer on " + base.gameObject.name + "!");
			return;
		}
		Material[] materials = component.materials;
		if (materialIndex < 0 || materialIndex >= materials.Length)
		{
			Debug.LogError($"[EmissiveFlickerReceiver] materialIndex {materialIndex} " + $"out of range for {base.gameObject.name} (has {materials.Length} materials)");
			return;
		}
		_instanceMaterial = materials[materialIndex];
		EmissiveFlickerRegistry.Register(flickerID, this);
	}

	private void OnDestroy()
	{
		EmissiveFlickerRegistry.Unregister(flickerID, this);
		if (_instanceMaterial != null)
		{
			Object.Destroy(_instanceMaterial);
		}
	}

	public void SetEmissiveIntensity(float intensity)
	{
		if (_instanceMaterial != null)
		{
			_instanceMaterial.SetFloat(EmissionColorScaleID, intensity);
		}
	}
}
