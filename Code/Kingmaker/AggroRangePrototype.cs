using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker;

public class AggroRangePrototype : MonoBehaviour
{
	public class AggroLineCharacter
	{
		public LineRenderer line;

		public LineRenderer decalLine;

		public UnitEntityView character;

		public Renderer decal;
	}

	public LineRenderer aggroLinePrefab;

	public LineRenderer aggroLineForDecalsPrefab;

	public List<UnitEntityView> playerCharacters = new List<UnitEntityView>();

	public List<AggroLineCharacter> aggroLines = new List<AggroLineCharacter>();

	public float maxDistance = 15f;

	public GameObject aggroDecal;

	public bool noDecal;

	public bool noLine;

	public bool movableDecal;

	public bool linesForDecals;

	public float decalLineOffset = 0.03f;

	private void Update()
	{
		if (Game.Instance.Player.ActiveCompanions.Count == 0)
		{
			return;
		}
		if (Game.Instance.Player.ActiveCompanions.Count > aggroLines.Count)
		{
			foreach (BaseUnitEntity activeCompanion in Game.Instance.Player.ActiveCompanions)
			{
				bool flag = false;
				foreach (AggroLineCharacter aggroLine in aggroLines)
				{
					if (aggroLine.character == activeCompanion.View)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					LineRenderer lineRenderer = Object.Instantiate(aggroLinePrefab, base.transform);
					lineRenderer.positionCount = 2;
					lineRenderer.SetPositions(new Vector3[2]
					{
						base.transform.FindRecursive("Head").position,
						activeCompanion.View.transform.FindRecursive("Pelvis").position
					});
					GameObject gameObject = Object.Instantiate(aggroDecal);
					LineRenderer lineRenderer2 = Object.Instantiate(aggroLineForDecalsPrefab, base.transform);
					lineRenderer2.positionCount = 2;
					lineRenderer.SetPositions(new Vector3[2]
					{
						base.transform.position,
						gameObject.transform.position
					});
					AggroLineCharacter item = new AggroLineCharacter
					{
						line = lineRenderer,
						decalLine = lineRenderer2,
						character = activeCompanion.View,
						decal = gameObject.GetComponent<Renderer>()
					};
					aggroLines.Add(item);
				}
			}
		}
		if (Game.Instance.Player.ActiveCompanions.Count < aggroLines.Count)
		{
			foreach (AggroLineCharacter aggroLine2 in aggroLines)
			{
				Object.Destroy(aggroLine2.line);
				Object.Destroy(aggroLine2.decal);
			}
			aggroLines.Clear();
		}
		foreach (AggroLineCharacter aggroLine3 in aggroLines)
		{
			aggroLine3.line.SetPosition(0, base.transform.FindRecursive("Head").position);
			aggroLine3.line.SetPosition(1, aggroLine3.character.transform.position);
			float num = Vector3.Distance(base.transform.position, aggroLine3.character.transform.position);
			Vector3 vector = Vector3.Lerp(base.transform.position, aggroLine3.character.transform.position, 1f / num * 8.5f);
			aggroLine3.decal.transform.position = vector;
			if (movableDecal)
			{
				float num2 = Vector3.Distance(vector, aggroLine3.character.transform.position);
				float num3 = maxDistance - 8.5f;
				float num4 = 1f / num3 * num2;
				Vector3 position = Vector3.Lerp(base.transform.position, vector, 1f - num4);
				aggroLine3.decal.transform.position = position;
				float num5 = Mathf.Lerp(0f, 1.495f, 1f - num4);
				aggroLine3.decal.transform.localScale = new Vector3(num5, 6.2f, num5);
				aggroLine3.decal.material.SetFloat("_AlphaScale", Mathf.Clamp(1f - num4, 0.15f, 0.75f));
			}
			aggroLine3.decal.transform.LookAt(aggroLine3.character.transform.position);
			float num6 = 1f / (maxDistance - 8.5f) * Vector3.Distance(aggroLine3.decal.transform.position, aggroLine3.character.transform.position);
			float num7 = 2f / (maxDistance - 8.5f) * Vector3.Distance(aggroLine3.decal.transform.position, aggroLine3.character.transform.position);
			if (!movableDecal)
			{
				aggroLine3.decal.material.SetFloat("_AlphaScale", Mathf.Clamp(1f - num6, 0f, 0.75f));
			}
			if (num > maxDistance)
			{
				aggroLine3.line.material.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0f));
				break;
			}
			float num8 = (num - 8.5f) / (maxDistance - 8.5f);
			aggroLine3.line.material.SetColor("_BaseColor", new Color(1f, num8, 0f, Mathf.Clamp(1f - num8, 0.05f, 1f)));
			aggroLine3.decal.material.SetColor("_BaseColor", new Color(1f, num8, 0f, 0.6392157f));
			aggroLine3.line.material.SetColor("_UV0Speed", new Color(3f - num7, 0f, 0f, 0f));
			if (linesForDecals)
			{
				Vector3 position2 = new Vector3(base.transform.position.x, base.transform.position.y + decalLineOffset, base.transform.position.z);
				new Vector3(aggroLine3.decal.transform.position.x, aggroLine3.decal.transform.position.y + decalLineOffset, aggroLine3.decal.transform.position.z);
				aggroLine3.decalLine.SetPosition(0, position2);
				aggroLine3.decalLine.SetPosition(1, aggroLine3.decal.transform.position);
				aggroLine3.decalLine.material.SetFloat("_AlphaScale", aggroLine3.decal.material.GetFloat("_AlphaScale"));
				aggroLine3.decalLine.material.SetColor("_BaseColor", new Color(1f, num8, 0f, 1f));
			}
			if (noDecal)
			{
				aggroLine3.decal.material.SetColor("_BaseColor", new Color(1f, num8, 0f, 0f));
			}
			if (noLine)
			{
				aggroLine3.line.material.SetColor("_BaseColor", new Color(1f, num8, 0f, 0f));
			}
		}
	}
}
