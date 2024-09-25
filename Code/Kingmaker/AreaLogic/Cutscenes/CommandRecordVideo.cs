using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[TypeId("4aa837b3083b029449443e8ec39a2971")]
public class CommandRecordVideo : CommandBase
{
	public string Folder = "ScreenshotFolder";

	public int FrameRate = 60;

	public int Width = 4096;

	public int Height = 2160;

	public float RecordTime = 2f;

	private VideoRecorder m_Player;

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		m_Player = new GameObject("[Video recorder]").AddComponent<VideoRecorder>();
		m_Player.Folder = Folder;
		m_Player.FrameRate = FrameRate;
		m_Player.Width = Width;
		m_Player.Height = Height;
		m_Player.RecordTime = RecordTime;
		m_Player.StartRecording();
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return !m_Player.IsRecording;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
