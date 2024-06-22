using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IUIHighlighter : ISubscriber
{
	RectTransform RectTransform { get; }

	void StartHighlight(string key);

	void StopHighlight(string key);

	void Highlight(string key);

	void HighlightOnce(string key);
}
