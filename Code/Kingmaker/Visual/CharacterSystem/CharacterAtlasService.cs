using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility.Locator;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

public class CharacterAtlasService : IService
{
	private struct AtlasRebuildRequest
	{
		public List<CharacterAtlas> Atlases;

		public Character Character;

		public Action<CharacterAtlas, Texture2D> OnTextureCompressed;

		public Action<CharacterAtlas> OnTextureNotCompressed;

		public string ContextString;

		public int CurrentAtlasIndex;
	}

	private readonly List<AtlasRebuildRequest> m_Requests = new List<AtlasRebuildRequest>();

	public ServiceLifetimeType Lifetime => ServiceLifetimeType.Game;

	public int RequestsCount => m_Requests.Count;

	public void QueueAtlasRebuild(Character character, List<CharacterAtlas> atlases, Action<CharacterAtlas, Texture2D> onTextureCompressed, Action<CharacterAtlas> onTextureNotCompressed, string contextString)
	{
		if (m_Requests.FindIndex((AtlasRebuildRequest r) => r.Character.AtlasMaterial == character.AtlasMaterial) != -1)
		{
			foreach (CharacterAtlas atlase in atlases)
			{
				onTextureNotCompressed?.Invoke(atlase);
			}
			return;
		}
		if (character.AtlasMaterial == null)
		{
			PFLog.Default.Error("CharacterAtlasService.QueueAtlasRebuild: material is null for " + contextString);
			{
				foreach (CharacterAtlas atlase2 in atlases)
				{
					atlase2.ClearTempValues();
					onTextureNotCompressed?.Invoke(atlase2);
				}
				return;
			}
		}
		AtlasRebuildRequest atlasRebuildRequest = default(AtlasRebuildRequest);
		atlasRebuildRequest.Atlases = new List<CharacterAtlas>(atlases);
		atlasRebuildRequest.Character = character;
		atlasRebuildRequest.OnTextureCompressed = onTextureCompressed;
		atlasRebuildRequest.OnTextureNotCompressed = onTextureNotCompressed;
		atlasRebuildRequest.ContextString = contextString;
		AtlasRebuildRequest item = atlasRebuildRequest;
		m_Requests.RemoveAll((AtlasRebuildRequest r) => r.Character.AtlasMaterial == null);
		m_Requests.Add(item);
		Update();
	}

	public void Update()
	{
		if (m_Requests.Count == 0)
		{
			return;
		}
		AtlasRebuildRequest request = m_Requests[0];
		DxtCompressorServiceNew instance = Services.GetInstance<DxtCompressorServiceNew>();
		if (instance != null && instance.RequestsCount > 0)
		{
			return;
		}
		m_Requests.RemoveAt(0);
		if (request.Character.AtlasMaterial == null)
		{
			PFLog.Default.Error("CharacterAtlasService.Update: Material is null in request for " + request.ContextString);
			foreach (CharacterAtlas atlase in request.Atlases)
			{
				atlase.ClearTempValues();
				request.OnTextureNotCompressed?.Invoke(atlase);
			}
			request.Atlases.Clear();
		}
		else if (request.Atlases.Count != 0)
		{
			request.Atlases[0].CompressAsync(delegate(CharacterAtlas a, Texture2D t)
			{
				OnOneAtlasCompressed(request, a, t);
			}, delegate(CharacterAtlas a)
			{
				OnOneAtlasNotCompressed(request, a);
			});
		}
	}

	private void OnOneAtlasCompressed(AtlasRebuildRequest request, CharacterAtlas atlas, Texture2D texture)
	{
		request.OnTextureCompressed(atlas, texture);
		if (request.CurrentAtlasIndex < request.Atlases.Count - 1)
		{
			CharacterAtlas characterAtlas = request.Atlases[++request.CurrentAtlasIndex];
			if (characterAtlas.Destroyed)
			{
				ClearRequestBecauseAtlasDestroyed(request);
				return;
			}
			characterAtlas.CompressAsync(delegate(CharacterAtlas a, Texture2D t)
			{
				OnOneAtlasCompressed(request, a, t);
			}, delegate(CharacterAtlas a)
			{
				OnOneAtlasNotCompressed(request, a);
			});
		}
		else
		{
			request.Atlases.Clear();
		}
	}

	private void OnOneAtlasNotCompressed(AtlasRebuildRequest request, CharacterAtlas atlas)
	{
		request.OnTextureNotCompressed(atlas);
		if (request.CurrentAtlasIndex < request.Atlases.Count - 1)
		{
			CharacterAtlas characterAtlas = request.Atlases[++request.CurrentAtlasIndex];
			if (characterAtlas.Destroyed)
			{
				ClearRequestBecauseAtlasDestroyed(request);
				return;
			}
			characterAtlas.CompressAsync(delegate(CharacterAtlas a, Texture2D t)
			{
				OnOneAtlasCompressed(request, a, t);
			}, delegate(CharacterAtlas a)
			{
				OnOneAtlasNotCompressed(request, a);
			});
		}
		else
		{
			request.Atlases.Clear();
		}
	}

	private void ClearRequestBecauseAtlasDestroyed(AtlasRebuildRequest request)
	{
		foreach (CharacterAtlas atlase in request.Atlases)
		{
			atlase.ClearTempValues();
		}
		request.Atlases.Clear();
	}
}
