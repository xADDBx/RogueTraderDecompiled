using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class RampGenerator : MonoBehaviour
{
	public Gradient targetGradient;

	[Tooltip("Renderer where material will be searched. Will try to find it in children if empty")]
	public Renderer targetRenderer;

	[Tooltip("Material where we will try to assign ramp")]
	public Material targetMaterial;

	[Tooltip("Height of the ramp")]
	public int resolutionHeight = 1;

	[Tooltip("Width of the ramp")]
	public int resolutionWidth = 256;

	[Tooltip("Update once on start")]
	public bool updateRampOnce;

	[Tooltip("Update every frame. Only in editor. VERY EXPENSIVE")]
	public bool updateRampEveryFrame;

	[Tooltip("Shader field where we want to assign our ramp")]
	public string shaderFieldName = "_FoamDensityRamp";

	[Tooltip("Ramp name for saving in file")]
	public string rampFileName = "_FoamDensityRamp";

	[Tooltip("Save ramp as .png file in Assets root folder")]
	public bool saveRampToFile;

	public Texture2D tex;

	private void OnEnable()
	{
		if (!targetRenderer)
		{
			targetRenderer = GetComponent<Renderer>();
		}
		if (!targetMaterial)
		{
			targetMaterial = targetRenderer.material;
		}
	}

	private void Update()
	{
		if ((bool)targetMaterial)
		{
			if (updateRampOnce)
			{
				SaveGradient();
				updateRampOnce = false;
				updateRampEveryFrame = false;
			}
			_ = updateRampEveryFrame;
			if (saveRampToFile)
			{
				SaveGradient(rampFileName);
				saveRampToFile = false;
			}
		}
	}

	public void SaveGradient()
	{
		tex = new Texture2D(resolutionWidth, resolutionHeight);
		for (int i = 0; i < resolutionWidth; i++)
		{
			for (int j = 0; j < resolutionHeight; j++)
			{
				tex.SetPixel(i, j, targetGradient.Evaluate((float)i / (float)resolutionWidth));
			}
		}
		tex.Apply();
		tex.EncodeToPNG();
		targetMaterial.SetTexture(shaderFieldName, tex);
	}

	public void SaveGradient(string fileName)
	{
		tex = new Texture2D(resolutionWidth, resolutionHeight);
		for (int i = 0; i < resolutionWidth; i++)
		{
			for (int j = 0; j < resolutionHeight; j++)
			{
				tex.SetPixel(i, j, targetGradient.Evaluate((float)i / (float)resolutionWidth));
			}
		}
		tex.Apply();
		byte[] bytes = tex.EncodeToPNG();
		File.WriteAllBytes($"{Application.dataPath}/{fileName}.png", bytes);
	}
}
