using System.Collections;
using System.IO;
using UnityEngine;

namespace Kingmaker.Utility.UnityExtensions;

public class VideoRecorder : MonoBehaviour
{
	public string Folder = "ScreenshotFolder";

	public int FrameRate = 60;

	public int Width = 4096;

	public int Height = 2160;

	public Camera Camera;

	public float RecordTime = 2f;

	public Animator Animator;

	private Texture2D m_ScreenShot;

	private RenderTexture m_Rt;

	public bool IsRecording { get; private set; }

	public void StartRecording()
	{
		Camera = (Camera ? Camera : Camera.main);
		if ((bool)Camera)
		{
			Time.captureFramerate = FrameRate;
			Directory.CreateDirectory(Folder);
			m_ScreenShot = new Texture2D(Width, Height, TextureFormat.RGB24, mipChain: false);
			m_Rt = new RenderTexture(Width, Height, 24);
			StartCoroutine(RecordRoutine());
			IsRecording = true;
		}
	}

	private IEnumerator RecordRoutine()
	{
		float start = Time.time;
		int frame = Time.frameCount - 1;
		if ((bool)Animator)
		{
			Animator.enabled = true;
		}
		while (Time.time <= start + RecordTime)
		{
			yield return new WaitForEndOfFrame();
			string text = $"{Folder}/{Time.frameCount - frame:D06}.png";
			ScreenShot(text);
		}
		IsRecording = false;
	}

	private void ScreenShot(string name)
	{
		Camera.targetTexture = m_Rt;
		Camera.Render();
		RenderTexture.active = m_Rt;
		m_ScreenShot.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
		Camera.targetTexture = null;
		RenderTexture.active = null;
		byte[] bytes = m_ScreenShot.EncodeToPNG();
		File.WriteAllBytes(name, bytes);
	}
}
