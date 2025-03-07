using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.AccessControl;
using UnityEngine;
using Log = KSPBuildTools.Log;

namespace Mutiny
{
	internal class ModifyObject : ScenePatch
	{
		Dictionary<string, Action<object>[]> m_gameObjectMutators = new Dictionary<string, Action<object>[]>();
		List<Action> m_staticMutators = new List<Action>();

		public override void Load(ConfigNode configNode)
		{
			base.Load(configNode);

			foreach (var childNode in configNode.nodes.nodes)
			{
				if (childNode.name == "GameObject")
				{
					string path = childNode.GetValue("path");
					childNode.RemoveValue("path");

					if (path == null)
					{
						Log.Error("ModifyObject patch must have a `path` field for each GameObject node");
					}

					m_gameObjectMutators.Add(path, CreateMutators(childNode, typeof(GameObject)));
				}
				else if (TypeUtilities.FindTypeNamed(childNode.name, out Type type))
				{
					var mutators = CreateMutators(childNode, type);
					m_staticMutators.Add(() =>
					{
						foreach (var mutator in mutators)
						{
							mutator.Invoke(null);
						}
					});
				}
			}
		}

		#region Reflection

		private static MemberInfo GetMemberInfo(Type objectType, string memberName, out Type memberType)
		{
			FieldInfo fieldInfo = objectType.GetField(memberName);
			if (fieldInfo != null)
			{
				memberType = fieldInfo.FieldType;
				return fieldInfo;
			}
			
			PropertyInfo propertyInfo = objectType.GetProperty(memberName);
			if (propertyInfo != null)
			{
				memberType = propertyInfo.PropertyType;
				return propertyInfo;
			}

			Log.Error($"Member {memberName} not found in type {objectType.FullName}");

			memberType = null;
			return null;
		}

		private static Action<object> CreateMutator(ConfigNode.Value configValue, Type objectType)
		{
			var memberInfo = GetMemberInfo(objectType, configValue.name, out Type memberType);

			if (memberInfo != null)
			{
				object newValue = ConfigNode.ReadValue(memberType, configValue.value);

				if (memberInfo is FieldInfo fieldInfo)
				{
					return obj => fieldInfo.SetValue(obj, newValue);
				}
				else if (memberInfo is PropertyInfo propertyInfo)
				{
					return obj => propertyInfo.SetValue(obj, newValue);
				}
			}

			return null;
		}

		private static void CreateObjectMutators(ConfigNode configNode, Type objectType, List<Action<object>> mutators)
		{
			foreach (ConfigNode.Value configValue in configNode.values.values)
			{
				var mutator = CreateMutator(configValue, objectType);
				if (mutator != null)
				{
					mutators.Add(mutator);
				}
			}

			var customNodeHandlers = x_customNodeHandlers.GetValueOrDefault(objectType);

			foreach (ConfigNode childNode in configNode.nodes.nodes)
			{
				// TODO: support arbitrary nested objects

				if (customNodeHandlers != null && customNodeHandlers.TryGetValue(childNode.name, out var customNodeHandler))
				{
					customNodeHandler.Invoke(childNode, objectType, mutators);
				}
			}
		}

		// these should probably be wrapped up in a factory class or something
		static Dictionary<Type, Dictionary<string, Action<ConfigNode, Type, List<Action<object>>>>> x_customNodeHandlers = new()
		{
			{
				typeof(GameObject), new ()
				{
					{ "Components", CreateComponentMutators },
				}
			},
		};

		private static void CreateComponentMutators(ConfigNode configNode, Type type, List<Action<object>> mutators)
		{
			foreach (ConfigNode componentNode in configNode.nodes.nodes)
			{
				string componentTypeName = componentNode.name;
				Type componentType = typeof(Component).GetSubclassNamed(componentTypeName);

				if (componentType == null) continue;

				bool createNew = false;
				if (componentNode.TryGetValue("create", ref createNew))
				{
					componentNode.RemoveValue("create");
				}

				Action<object>[] componentMutators = CreateMutators(componentNode, componentType);

				mutators.Add(obj =>
				{
					GameObject gameObject = (GameObject)obj;
					var component = gameObject.GetComponent(componentType);
					if (component == null)
					{
						if (createNew)
						{
							component = gameObject.AddComponent(componentType);
						}
						else
						{
							Log.Error($"Component {componentTypeName} not found on GameObject {gameObject.name}");
							return;
						}
					}
					else if (createNew)
					{
						// maybe we should still add a new one?
						Log.Warning($"Component {componentTypeName} already existed on Gamebject {gameObject.name}");
					}

					foreach (var mutator in componentMutators)
					{
						mutator.Invoke(component);
					}
				});
			}
		}

		private static Action<object>[] CreateMutators(ConfigNode configNode, Type objectType)
		{
			List<Action<object>> mutators = new List<Action<object>>();
			CreateObjectMutators(configNode, objectType, mutators);
			return mutators.ToArray();
		}

		#endregion

		public override void Execute()
		{
			foreach (var objectMutator in m_gameObjectMutators)
			{
				GameObject gameObject = GameObject.Find(objectMutator.Key);

				if (gameObject == null)
				{
					Log.Error("GameObject {objectName} not found");
					continue;
				}

				foreach (var mutator in objectMutator.Value)
				{
					mutator.Invoke(gameObject);
				}
			}

			foreach (var staticMutator in m_staticMutators)
			{
				staticMutator.Invoke();
			}
		}
	}
}
