using KSPBuildTools;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutiny
{
	public class ScenePatch
	{
		// might want to abstract this out at some point
		public static ScenePatch Create(UrlDir.UrlConfig urlConfig)
		{
			List<Type> scenePatchTypes = AssemblyLoader.GetSubclassesOfParentClass(typeof(ScenePatch));

			ConfigNode configNode = urlConfig.config;
			string typeName = configNode.GetValue("type") ?? "";
			Type type = scenePatchTypes.FirstOrDefault(t => t.Name == typeName);
			if (type == null)
			{
				Log.Error($"{typeName} is not a valid ScenePatch type: {urlConfig.url}");
				return null;
			}

			try
			{
				var result = (ScenePatch)Activator.CreateInstance(type);
				result.Load(configNode);
				return result;
			}
			catch (Exception ex)
			{
				Log.Error($"failed to create scenepatch {typeName} : {urlConfig.url}");
				Log.Exception(ex);
			}

			return null;
		}

		[Persistent]
		public string name = "";

		public virtual void Load(ConfigNode configNode)
		{
			ConfigNode.LoadObjectFromConfig(this, configNode);
		}

		public virtual void Execute() { }
	}
}
