using Kingmaker.Code.UI.MVVM.View.BugReport;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;
using UnityEngine;

namespace Kingmaker.UI.Legacy.BugReportDrawing;

public class BugReportDrawing
{
	private delegate void BrushFunction(Vector2 worldPosition);

	public BugReportDrawingView BugreportDrawingView;

	private static readonly Color32 ClearColor = new Color32(0, 0, 0, 0);

	private static readonly Color32 PenColor = Color.green;

	private const int PenWidth = 2;

	private Color32[] m_CurColors;

	private Color32[] m_CleanColoursArray;

	private bool m_MouseWasPreviouslyHeldDown;

	private bool m_NoDrawingOnCurrentDrag;

	private Vector2 m_PreviousDragPosition;

	private BrushFunction m_CurrentBrush;

	public void Awake()
	{
		m_CurrentBrush = PenBrush;
		int num = BugreportDrawingView.DrawingTexture.width * BugreportDrawingView.DrawingTexture.height;
		m_CleanColoursArray = new Color32[num];
		for (int i = 0; i < m_CleanColoursArray.Length; i++)
		{
			m_CleanColoursArray[i] = ClearColor;
		}
		ResetCanvas();
	}

	public void Update()
	{
		bool flag = (Game.Instance.IsControllerMouse ? Input.GetMouseButton(0) : GamePad.Instance.Player.GetButton(8));
		if (flag && !m_NoDrawingOnCurrentDrag)
		{
			Vector2 vector = (Game.Instance.IsControllerMouse ? Input.mousePosition : UICamera.Instance.WorldToScreenPoint(UIKitRewiredCursorController.Cursor.transform.position));
			if (CheckHitOnImage(vector))
			{
				m_CurrentBrush(vector);
			}
			else
			{
				m_PreviousDragPosition = Vector2.zero;
				if (!m_MouseWasPreviouslyHeldDown)
				{
					m_NoDrawingOnCurrentDrag = true;
				}
			}
		}
		else if (!flag)
		{
			m_PreviousDragPosition = Vector2.zero;
			m_NoDrawingOnCurrentDrag = false;
		}
		m_MouseWasPreviouslyHeldDown = flag;
	}

	private bool CheckHitOnImage(Vector2 mousePos)
	{
		Vector2 localPoint;
		return RectTransformUtility.ScreenPointToLocalPointInRectangle(BugreportDrawingView.DrawingImage.rectTransform, mousePos, UICamera.Instance, out localPoint);
	}

	private Vector2 WorldToPixelCoordinates(Vector2 worldPosition)
	{
		RectTransform rectTransform = BugreportDrawingView.DrawingImage.rectTransform;
		Vector2 b = new Vector2(BugreportDrawingView.DrawingTexture.width, BugreportDrawingView.DrawingTexture.height);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, worldPosition, UICamera.Instance, out var localPoint);
		return Vector2.Scale(Rect.PointToNormalized(rectTransform.rect, localPoint), b);
	}

	private void PenBrush(Vector2 worldPoint)
	{
		Vector2 vector = WorldToPixelCoordinates(worldPoint);
		m_CurColors = BugreportDrawingView.DrawingTexture.GetPixels32();
		if (m_PreviousDragPosition == Vector2.zero)
		{
			DrawPen(vector, 2, PenColor);
		}
		else
		{
			DrawLine(m_PreviousDragPosition, vector, 2, PenColor);
		}
		ApplyMarkedPixelChanges();
		m_PreviousDragPosition = vector;
	}

	private void ApplyMarkedPixelChanges()
	{
		BugreportDrawingView.DrawingTexture.SetPixels32(m_CurColors);
		BugreportDrawingView.DrawingTexture.Apply();
	}

	public void ResetCanvas()
	{
		BugreportDrawingView.DrawingTexture.SetPixels32(m_CleanColoursArray);
		BugreportDrawingView.DrawingTexture.Apply();
	}

	private void DrawPen(Vector2 centerPixel, int penThickness, Color32 colorOfPen)
	{
		int num = (int)centerPixel.x;
		int num2 = (int)centerPixel.y;
		for (int i = num - penThickness; i <= num + penThickness; i++)
		{
			if (i >= BugreportDrawingView.DrawingTexture.width || i < 0)
			{
				continue;
			}
			for (int j = num2 - penThickness; j <= num2 + penThickness; j++)
			{
				float num3 = Vector2.Distance(centerPixel, new Vector2(i, j)) / (float)penThickness;
				if (!(num3 > 1f))
				{
					byte b = (byte)(255f * Mathf.SmoothStep(1f, 0f, num3));
					if (!IsDrawn(i, j, b))
					{
						Color32 color = colorOfPen;
						color.a = b;
						DrawPixel(i, j, color);
					}
				}
			}
		}
	}

	private bool IsDrawn(int x, int y, byte threshold)
	{
		int num = y * BugreportDrawingView.DrawingTexture.width + x;
		if (num >= m_CurColors.Length || num < 0)
		{
			return true;
		}
		return m_CurColors[num].a > threshold;
	}

	private void DrawPixel(int x, int y, Color color)
	{
		int num = y * BugreportDrawingView.DrawingTexture.width + x;
		if (num < m_CurColors.Length && num >= 0)
		{
			m_CurColors[num] = color;
		}
	}

	private void DrawLine(Vector2 startPoint, Vector2 endPoint, int width, Color color)
	{
		float num = Vector2.Distance(startPoint, endPoint);
		float num2 = 1f / num;
		for (float num3 = 0f; num3 <= 1f; num3 += num2)
		{
			Vector2 centerPixel = Vector2.Lerp(startPoint, endPoint, num3);
			DrawPen(centerPixel, width, color);
		}
	}
}
