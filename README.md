Mutate + Unity = Mutiny

We are the captain now.  Plus, there's probably eyepatches involved somewhere.

Mutiny is a cfg-driven framework for modifying objects at runtime.  The main use is to patch objects that were created from KSP's stock assetbundles, but it's certainly not limited to that.

---

# `SCENE_PATCH`

Contains a set of patches to apply when a scene is loaded.

## Fields

- `name`  
  *optional*.  The name of the patch.  This is used for logging purposes.
- `type`  
  The type of patch to apply.  This can be one of the following (see below for details on each):
  - `DeleteObject`
  - `ModifyObject`
- `scene`  
  The name of the scene in which to apply the patch.  This field may be specified multiple times to apply the patch to several scenes.
  Note that this is the name of the unity scene, not the KSP scene.  Mutiny will emit logging messages for each unity scene load.

## `DeleteObject`

Deletes an object from the scene.  The syntax of this patch is likely to change so I'm not going to document it here.

## `ModifyObject`

Alters Gameobjects or static members of classes.

### Nodes

#### General Object Mutations

Within any object mutation node, the fields and sub-nodes will modify the object when the patch is executed.
Simple value-type fields and properties on the object (numbers, strings, colors, vectors, enums, etc) can be altered with a field in the cfg node.
Compound types (classes, structs) can be altered by adding a sub-node with the name of the field or property to mutate.
Containers (arrays, lists, dictionaries) are not yet suported.

#### `GameObject`

For each `GameObject` node, Mutiny will search for a GameObject with the specified path in the scene and apply the specified modifications.
The `GameObject` node must have a `path` field which is used with `GameObject.Find` to locate the object.

In addition to normal field and property mutators, `GameObject`nodes may contain a `Components` node.
Inside this node, each sub-node's name corresponds to a component type, and the values inside will mutate that component within the GameObject.
If `create = true` is specified, a new component of the given type will be added to the object, and the config node will be used to set its values.

```
SCENE_PATCH
{
	name = test
	type = ModifyObject
	scene = VABmodern
	
	GameObject // finds a GameObject with a given scene path
	{
		path = VABmodern/VAB_interior_modern/Day Lights/Shadow Light

		name = RedLight // sets the name of the GameObject (remember there's nothing special about "name" here; it's just a field like any other))

		Components
		{
			Light
			{
				color = 1,0,0
			}

			MyCoolComponent
			{
				create = true
				coolness = 9001
			}
		}
```

#### Static members of classes

If the name of the node is a class, then the contents of the node will be applied to the static members of that class.

```
SCENE_PATCH
{
	name = test
	type = ModifyObject
	scene = VABmodern

	UnityEngine.RenderSettings // this is a class.  Full qualification is necessary because there are other classes named RenderSettings
	{
		ambientLightColor = 0,0,1 // modifies RenderSettings.ambientLightColor

		sun
		{
			color = 1,1,0 // modifies RenderSettings.sun.color
		}
	}
}
```

