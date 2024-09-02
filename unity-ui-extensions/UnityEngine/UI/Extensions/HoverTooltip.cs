namespace UnityEngine.UI.Extensions;

public class HoverTooltip : MonoBehaviour
{
	public int horizontalPadding;

	public int verticalPadding;

	public Text thisText;

	public HorizontalLayoutGroup hlG;

	public RectTransform bgImage;

	private Image bgImageSource;

	private bool firstUpdate;

	private bool inside;

	private RenderMode GUIMode;

	private Camera GUICamera;

	private Vector3 lowerLeft;

	private Vector3 upperRight;

	private float currentYScaleFactor;

	private float currentXScaleFactor;

	private float defaultYOffset;

	private float defaultXOffset;

	private float tooltipRealHeight;

	private float tooltipRealWidth;

	private void Start()
	{
		GUICamera = GameObject.Find("GUICamera").GetComponent<Camera>();
		GUIMode = base.transform.parent.parent.GetComponent<Canvas>().renderMode;
		bgImageSource = bgImage.GetComponent<Image>();
		inside = false;
		HideTooltipVisibility();
		base.transform.parent.gameObject.SetActive(value: false);
	}

	public void SetTooltip(string text)
	{
		NewTooltip();
		thisText.text = text;
		OnScreenSpaceCamera();
	}

	public void SetTooltip(string[] texts)
	{
		NewTooltip();
		string text = "";
		int num = 0;
		foreach (string text2 in texts)
		{
			text = ((num != 0) ? (text + "\n" + text2) : (text + text2));
			num++;
		}
		thisText.text = text;
		OnScreenSpaceCamera();
	}

	public void SetTooltip(string text, bool test)
	{
		NewTooltip();
		thisText.text = text;
		OnScreenSpaceCamera();
	}

	public void OnScreenSpaceCamera()
	{
		Vector3 position = GUICamera.ScreenToViewportPoint(Input.mousePosition);
		float num = 0f;
		float num2 = 0f;
		float num3 = GUICamera.ViewportToScreenPoint(position).x + tooltipRealWidth * bgImage.pivot.x;
		if (num3 > upperRight.x)
		{
			float num4 = upperRight.x - num3;
			num2 = ((!((double)num4 > (double)defaultXOffset * 0.75)) ? (defaultXOffset - tooltipRealWidth * 2f) : num4);
			Vector3 position2 = new Vector3(GUICamera.ViewportToScreenPoint(position).x + num2, 0f, 0f);
			position.x = GUICamera.ScreenToViewportPoint(position2).x;
		}
		num3 = GUICamera.ViewportToScreenPoint(position).x - tooltipRealWidth * bgImage.pivot.x;
		if (num3 < lowerLeft.x)
		{
			float num5 = lowerLeft.x - num3;
			num2 = ((!((double)num5 < (double)defaultXOffset * 0.75 - (double)tooltipRealWidth)) ? (tooltipRealWidth * 2f) : (0f - num5));
			Vector3 position3 = new Vector3(GUICamera.ViewportToScreenPoint(position).x - num2, 0f, 0f);
			position.x = GUICamera.ScreenToViewportPoint(position3).x;
		}
		num3 = GUICamera.ViewportToScreenPoint(position).y - (bgImage.sizeDelta.y * currentYScaleFactor * bgImage.pivot.y - tooltipRealHeight);
		if (num3 > upperRight.y)
		{
			float num6 = upperRight.y - num3;
			num = bgImage.sizeDelta.y * currentYScaleFactor * bgImage.pivot.y;
			num = ((!((double)num6 > (double)defaultYOffset * 0.75)) ? (defaultYOffset - tooltipRealHeight * 2f) : num6);
			Vector3 position4 = new Vector3(position.x, GUICamera.ViewportToScreenPoint(position).y + num, 0f);
			position.y = GUICamera.ScreenToViewportPoint(position4).y;
		}
		num3 = GUICamera.ViewportToScreenPoint(position).y - bgImage.sizeDelta.y * currentYScaleFactor * bgImage.pivot.y;
		if (num3 < lowerLeft.y)
		{
			float num7 = lowerLeft.y - num3;
			num = bgImage.sizeDelta.y * currentYScaleFactor * bgImage.pivot.y;
			num = ((!((double)num7 < (double)defaultYOffset * 0.75 - (double)tooltipRealHeight)) ? (tooltipRealHeight * 2f) : num7);
			Vector3 position5 = new Vector3(position.x, GUICamera.ViewportToScreenPoint(position).y + num, 0f);
			position.y = GUICamera.ScreenToViewportPoint(position5).y;
		}
		base.transform.parent.transform.position = new Vector3(GUICamera.ViewportToWorldPoint(position).x, GUICamera.ViewportToWorldPoint(position).y, 0f);
		base.transform.parent.gameObject.SetActive(value: true);
		inside = true;
	}

	public void HideTooltip()
	{
		if (GUIMode == RenderMode.ScreenSpaceCamera && this != null)
		{
			base.transform.parent.gameObject.SetActive(value: false);
			inside = false;
			HideTooltipVisibility();
		}
	}

	private void Update()
	{
		LayoutInit();
		if (inside && GUIMode == RenderMode.ScreenSpaceCamera)
		{
			OnScreenSpaceCamera();
		}
	}

	private void LayoutInit()
	{
		if (firstUpdate)
		{
			firstUpdate = false;
			bgImage.sizeDelta = new Vector2(hlG.preferredWidth + (float)horizontalPadding, hlG.preferredHeight + (float)verticalPadding);
			defaultYOffset = bgImage.sizeDelta.y * currentYScaleFactor * bgImage.pivot.y;
			defaultXOffset = bgImage.sizeDelta.x * currentXScaleFactor * bgImage.pivot.x;
			tooltipRealHeight = bgImage.sizeDelta.y * currentYScaleFactor;
			tooltipRealWidth = bgImage.sizeDelta.x * currentXScaleFactor;
			ActivateTooltipVisibility();
		}
	}

	private void NewTooltip()
	{
		firstUpdate = true;
		lowerLeft = GUICamera.ViewportToScreenPoint(new Vector3(0f, 0f, 0f));
		upperRight = GUICamera.ViewportToScreenPoint(new Vector3(1f, 1f, 0f));
		currentYScaleFactor = (float)Screen.height / base.transform.root.GetComponent<CanvasScaler>().referenceResolution.y;
		currentXScaleFactor = (float)Screen.width / base.transform.root.GetComponent<CanvasScaler>().referenceResolution.x;
	}

	public void ActivateTooltipVisibility()
	{
		Color color = thisText.color;
		thisText.color = new Color(color.r, color.g, color.b, 1f);
		bgImageSource.color = new Color(bgImageSource.color.r, bgImageSource.color.g, bgImageSource.color.b, 0.8f);
	}

	public void HideTooltipVisibility()
	{
		Color color = thisText.color;
		thisText.color = new Color(color.r, color.g, color.b, 0f);
		bgImageSource.color = new Color(bgImageSource.color.r, bgImageSource.color.g, bgImageSource.color.b, 0f);
	}
}
