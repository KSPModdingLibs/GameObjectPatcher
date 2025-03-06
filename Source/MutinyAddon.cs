using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Log = KSPBuildTools.Log;

namespace Mutiny
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class MutinyAddon : MonoBehaviour
	{
		void Awake()
		{
			GameObject.DontDestroyOnLoad(this);

			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			Log.Message("OnSceneLoaded: " + scene.name);

			if (m_scenePatches.TryGetValue(scene.name, out var scenePatches))
			{
				foreach (var scenePatch in scenePatches)
				{
					Log.Debug($"executing patch {scenePatch.name}");
					scenePatch.Execute();
				}
			}
		}

		Dictionary<string, List<ScenePatch>> m_scenePatches = new Dictionary<string, List<ScenePatch>>();

		public void ModuleManagerPostLoad()
		{
			foreach (var urlConfig in GameDatabase.Instance.root.AllConfigs)
			{
				if (urlConfig.type == "SCENE_PATCH")
				{
					var scenes = urlConfig.config.GetValuesList("scene");

					if (scenes.Count == 0)
					{
						Log.Error($"SCENE_PATCH config {urlConfig.url} must have at least one 'scene' value");
						continue;
					}

					var scenePatch = ScenePatch.Create(urlConfig);

					if (scenePatch == null) continue;

					foreach (var scene in scenes)
					{
						if (!m_scenePatches.ContainsKey(scene))
						{
							m_scenePatches[scene] = new List<ScenePatch>();
						}
						m_scenePatches[scene].Add(scenePatch);
					}
				}
			}
		}
	}
}
