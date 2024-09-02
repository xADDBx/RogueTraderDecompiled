namespace UnityEngine.UI.Extensions;

[ExecuteInEditMode]
[AddComponentMenu("UI/Effects/Extensions/SoftMaskScript")]
public class SoftMaskScript : MonoBehaviour
{
	private Material mat;

	private Canvas canvas;

	[Tooltip("The area that is to be used as the container.")]
	public RectTransform MaskArea;

	private RectTransform myRect;

	[Tooltip("A Rect Transform that can be used to scale and move the mask - Does not apply to Text UI Components being masked")]
	public RectTransform maskScalingRect;

	[Tooltip("Texture to be used to do the soft alpha")]
	public Texture AlphaMask;

	[Tooltip("At what point to apply the alpha min range 0-1")]
	[Range(0f, 1f)]
	public float CutOff;

	[Tooltip("Implement a hard blend based on the Cutoff")]
	public bool HardBlend;

	[Tooltip("Flip the masks alpha value")]
	public bool FlipAlphaMask;

	[Tooltip("If Mask Scaling Rect is given and this value is true, the area around the mask will not be clipped")]
	public bool DontClipMaskScalingRect;

	[Tooltip("If set to true, this mask is applied to all child Text and Graphic objects belonging to this object.")]
	public bool CascadeToALLChildren;

	private Vector3[] worldCorners;

	private Vector2 AlphaUV;

	private Vector2 min;

	private Vector2 max = Vector2.one;

	private Vector2 p;

	private Vector2 siz;

	private Vector2 tp = new Vector2(0.5f, 0.5f);

	private bool MaterialNotSupported;

	private Rect maskRect;

	private Rect contentRect;

	private Vector2 centre;

	private bool isText;

	private void Start()
	{
		myRect = GetComponent<RectTransform>();
		if (!MaskArea)
		{
			MaskArea = myRect;
		}
		if (GetComponent<Graphic>() != null)
		{
			mat = new Material(Shader.Find("UI Extensions/SoftMaskShader"));
			GetComponent<Graphic>().material = mat;
		}
		if ((bool)GetComponent<Text>())
		{
			isText = true;
			mat = new Material(Shader.Find("UI Extensions/SoftMaskShaderText"));
			GetComponent<Text>().material = mat;
			GetCanvas();
			if (base.transform.parent.GetComponent<Button>() == null && base.transform.parent.GetComponent<Mask>() == null)
			{
				base.transform.parent.gameObject.AddComponent<Mask>();
			}
			if (base.transform.parent.GetComponent<Mask>() != null)
			{
				base.transform.parent.GetComponent<Mask>().enabled = false;
			}
		}
		if (CascadeToALLChildren)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				SetSAM(base.transform.GetChild(i));
			}
		}
		MaterialNotSupported = mat == null;
	}

	private void SetSAM(Transform t)
	{
		SoftMaskScript softMaskScript = t.gameObject.GetComponent<SoftMaskScript>();
		if (softMaskScript == null)
		{
			softMaskScript = t.gameObject.AddComponent<SoftMaskScript>();
		}
		softMaskScript.MaskArea = MaskArea;
		softMaskScript.AlphaMask = AlphaMask;
		softMaskScript.CutOff = CutOff;
		softMaskScript.HardBlend = HardBlend;
		softMaskScript.FlipAlphaMask = FlipAlphaMask;
		softMaskScript.maskScalingRect = maskScalingRect;
		softMaskScript.DontClipMaskScalingRect = DontClipMaskScalingRect;
		softMaskScript.CascadeToALLChildren = CascadeToALLChildren;
	}

	private void GetCanvas()
	{
		Transform parent = base.transform;
		int num = 100;
		int num2 = 0;
		while (canvas == null && num2 < num)
		{
			canvas = parent.gameObject.GetComponent<Canvas>();
			if (canvas == null)
			{
				parent = parent.parent;
			}
			num2++;
		}
	}

	private void LateUpdate()
	{
		SetMask();
	}

	public void Refresh()
	{
		if (GetComponent<Graphic>() != null)
		{
			mat = (mat ? mat : new Material(Shader.Find("UI Extensions/SoftMaskShader")));
			GetComponent<Graphic>().material = mat;
		}
	}

	private void SetMask()
	{
		Refresh();
		if (MaterialNotSupported)
		{
			return;
		}
		maskRect = MaskArea.rect;
		contentRect = myRect.rect;
		if (isText)
		{
			maskScalingRect = null;
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && Application.isPlaying)
			{
				p = canvas.transform.InverseTransformPoint(MaskArea.transform.position);
				siz = new Vector2(maskRect.width, maskRect.height);
			}
			else
			{
				worldCorners = new Vector3[4];
				MaskArea.GetWorldCorners(worldCorners);
				siz = worldCorners[2] - worldCorners[0];
				p = MaskArea.transform.position;
			}
			min = p - new Vector2(siz.x, siz.y) * 0.5f;
			max = p + new Vector2(siz.x, siz.y) * 0.5f;
		}
		else
		{
			if (maskScalingRect != null)
			{
				maskRect = maskScalingRect.rect;
			}
			if (maskScalingRect != null)
			{
				centre = myRect.transform.InverseTransformPoint(maskScalingRect.transform.TransformPoint(maskScalingRect.rect.center));
			}
			else
			{
				centre = myRect.transform.InverseTransformPoint(MaskArea.transform.TransformPoint(MaskArea.rect.center));
			}
			centre += (Vector2)myRect.transform.InverseTransformPoint(myRect.transform.position) - myRect.rect.center;
			AlphaUV = new Vector2(maskRect.width / contentRect.width, maskRect.height / contentRect.height);
			min = centre;
			max = min;
			siz = new Vector2(maskRect.width, maskRect.height) * 0.5f;
			min -= siz;
			max += siz;
			min = new Vector2(min.x / contentRect.width, min.y / contentRect.height) + tp;
			max = new Vector2(max.x / contentRect.width, max.y / contentRect.height) + tp;
		}
		mat.SetFloat("_HardBlend", HardBlend ? 1 : 0);
		mat.SetVector("_Min", min);
		mat.SetVector("_Max", max);
		mat.SetInt("_FlipAlphaMask", FlipAlphaMask ? 1 : 0);
		mat.SetTexture("_AlphaMask", AlphaMask);
		mat.SetInt("_NoOuterClip", (DontClipMaskScalingRect && maskScalingRect != null) ? 1 : 0);
		if (!isText)
		{
			mat.SetVector("_AlphaUV", AlphaUV);
		}
		mat.SetFloat("_CutOff", CutOff);
	}
}
