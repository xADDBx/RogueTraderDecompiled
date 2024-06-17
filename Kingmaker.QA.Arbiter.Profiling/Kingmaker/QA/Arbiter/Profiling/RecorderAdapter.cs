using UnityEngine.Profiling;

namespace Kingmaker.QA.Arbiter.Profiling;

public class RecorderAdapter
{
	public Recorder BehaviourUpdate = Recorder.Get("BehaviourUpdate");

	public Recorder BehaviourLateUpdate = Recorder.Get("LateBehaviourUpdate");

	public Recorder Probes = Recorder.Get("ReflectionProbes.Update");

	public Recorder Particles = Recorder.Get("ParticleSystem.Update");

	public Recorder Skinning = Recorder.Get("MeshSkinning.Update");

	public Recorder Canvases = Recorder.Get("PostLateUpdate.PlayerUpdateCanvases");

	public Recorder DirectorBegin = Recorder.Get("PreLateUpdate.DirectorUpdateAnimationBegin");

	public Recorder DirectorUpdate = Recorder.Get("Update.DirectorUpdate");

	public Recorder DirectorEnd = Recorder.Get("PreLateUpdate.DirectorUpdateAnimationEnd");

	public void AddMeasurements()
	{
	}
}
