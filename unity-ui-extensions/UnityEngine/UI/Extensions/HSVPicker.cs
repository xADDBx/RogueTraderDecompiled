namespace UnityEngine.UI.Extensions;

public class HSVPicker : MonoBehaviour
{
	public HexRGB hexrgb;

	public Color currentColor;

	public Image colorImage;

	public Image pointer;

	public Image cursor;

	public RawImage hsvSlider;

	public RawImage hsvImage;

	public Slider sliderR;

	public Slider sliderG;

	public Slider sliderB;

	public Text sliderRText;

	public Text sliderGText;

	public Text sliderBText;

	public float pointerPos;

	public float cursorX;

	public float cursorY;

	public HSVSliderEvent onValueChanged = new HSVSliderEvent();

	private bool dontAssignUpdate;

	private void Awake()
	{
		hsvSlider.texture = HSVUtil.GenerateHSVTexture((int)hsvSlider.rectTransform.rect.width, (int)hsvSlider.rectTransform.rect.height);
		sliderR.onValueChanged.AddListener(delegate(float newValue)
		{
			currentColor.r = newValue;
			if (!dontAssignUpdate)
			{
				AssignColor(currentColor);
			}
			sliderRText.text = "R:" + (int)(currentColor.r * 255f);
			hexrgb.ManipulateViaRGB2Hex();
		});
		sliderG.onValueChanged.AddListener(delegate(float newValue)
		{
			currentColor.g = newValue;
			if (!dontAssignUpdate)
			{
				AssignColor(currentColor);
			}
			sliderGText.text = "G:" + (int)(currentColor.g * 255f);
			hexrgb.ManipulateViaRGB2Hex();
		});
		sliderB.onValueChanged.AddListener(delegate(float newValue)
		{
			currentColor.b = newValue;
			if (!dontAssignUpdate)
			{
				AssignColor(currentColor);
			}
			sliderBText.text = "B:" + (int)(currentColor.b * 255f);
			hexrgb.ManipulateViaRGB2Hex();
		});
		hsvImage.texture = HSVUtil.GenerateColorTexture((int)hsvImage.rectTransform.rect.width, (int)hsvImage.rectTransform.rect.height, ((Texture2D)hsvSlider.texture).GetPixelBilinear(0f, 0f));
		MoveCursor(cursorX, cursorY);
	}

	private void Update()
	{
	}

	public void AssignColor(Color color)
	{
		HsvColor hsvColor = HSVUtil.ConvertRgbToHsv(color);
		float newPos = (float)(hsvColor.H / 360.0);
		MovePointer(newPos, updateInputs: false);
		MoveCursor((float)hsvColor.S, (float)hsvColor.V, updateInputs: false);
		currentColor = color;
		colorImage.color = currentColor;
		onValueChanged.Invoke(currentColor);
	}

	public Color MoveCursor(float posX, float posY, bool updateInputs = true)
	{
		dontAssignUpdate = updateInputs;
		if (posX > 1f)
		{
			posX %= 1f;
		}
		if (posY > 1f)
		{
			posY %= 1f;
		}
		posY = Mathf.Clamp(posY, 0f, 0.9999f);
		posX = Mathf.Clamp(posX, 0f, 0.9999f);
		cursorX = posX;
		cursorY = posY;
		cursor.rectTransform.anchoredPosition = new Vector2(posX * hsvImage.rectTransform.rect.width, posY * hsvImage.rectTransform.rect.height - hsvImage.rectTransform.rect.height);
		currentColor = GetColor(cursorX, cursorY);
		colorImage.color = currentColor;
		if (updateInputs)
		{
			UpdateInputs();
			onValueChanged.Invoke(currentColor);
		}
		dontAssignUpdate = false;
		return currentColor;
	}

	public Color GetColor(float posX, float posY)
	{
		return ((Texture2D)hsvImage.texture).GetPixel((int)(cursorX * (float)hsvImage.texture.width), (int)(cursorY * (float)hsvImage.texture.height));
	}

	public Color MovePointer(float newPos, bool updateInputs = true)
	{
		dontAssignUpdate = updateInputs;
		if (newPos > 1f)
		{
			newPos %= 1f;
		}
		pointerPos = newPos;
		Color pixelBilinear = ((Texture2D)hsvSlider.texture).GetPixelBilinear(0f, pointerPos);
		if (hsvImage.texture != null)
		{
			if ((int)hsvImage.rectTransform.rect.width != hsvImage.texture.width || (int)hsvImage.rectTransform.rect.height != hsvImage.texture.height)
			{
				Object.Destroy(hsvImage.texture);
				hsvImage.texture = null;
				hsvImage.texture = HSVUtil.GenerateColorTexture((int)hsvImage.rectTransform.rect.width, (int)hsvImage.rectTransform.rect.height, pixelBilinear);
			}
			else
			{
				HSVUtil.GenerateColorTexture(pixelBilinear, (Texture2D)hsvImage.texture);
			}
		}
		else
		{
			hsvImage.texture = HSVUtil.GenerateColorTexture((int)hsvImage.rectTransform.rect.width, (int)hsvImage.rectTransform.rect.height, pixelBilinear);
		}
		pointer.rectTransform.anchoredPosition = new Vector2(0f, (0f - pointerPos) * hsvSlider.rectTransform.rect.height);
		currentColor = GetColor(cursorX, cursorY);
		colorImage.color = currentColor;
		if (updateInputs)
		{
			UpdateInputs();
			onValueChanged.Invoke(currentColor);
		}
		dontAssignUpdate = false;
		return currentColor;
	}

	public void UpdateInputs()
	{
		sliderR.value = currentColor.r;
		sliderG.value = currentColor.g;
		sliderB.value = currentColor.b;
		sliderRText.text = "R:" + currentColor.r * 255f;
		sliderGText.text = "G:" + currentColor.g * 255f;
		sliderBText.text = "B:" + currentColor.b * 255f;
	}

	private void OnDestroy()
	{
		if (hsvSlider.texture != null)
		{
			Object.Destroy(hsvSlider.texture);
		}
		if (hsvImage.texture != null)
		{
			Object.Destroy(hsvImage.texture);
		}
	}
}
