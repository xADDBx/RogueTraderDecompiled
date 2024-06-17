using UnityEngine.Playables;

namespace Kingmaker.Visual.Animation.GraphVisualizerClient;

public class GraphVisualizerClient
{
	public delegate void UpdateGraphDelegate(Playable p, string title);

	public UpdateGraphDelegate updateGraph;

	private static GraphVisualizerClient s_Instance;

	public static GraphVisualizerClient instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new GraphVisualizerClient();
			}
			return s_Instance;
		}
	}

	public static void Show(Playable p, string title)
	{
		if (instance.updateGraph != null)
		{
			instance.updateGraph(p, title);
		}
	}
}
