using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NostraTools.Editor
{
    public static class RemapDllUtils
    {
        private static Dictionary<Component, Component> globalOldToNew = new();
        private static Dictionary<ScriptableObject, ScriptableObject> globalSoMap = new();
        private static Dictionary<GameObject, GameObject> globalPrefabMap = new();

        internal static void RemapAllAssets(string targetScriptsAssemblyName, string assetDir)
        {
            RemapAllSoToNewAssembly(targetScriptsAssemblyName, assetDir);
            Debug.Log("Remaped Scriptable objects..");
            RemapAllPrefabsToAssembly(targetScriptsAssemblyName, assetDir);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        internal static void RemapAllSoToNewAssembly(string targetScriptsAssemblyName, string targetSoDir)
        {
            var dllAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == targetScriptsAssemblyName);
            if (dllAssembly == null)
            {
                Debug.LogError($"Assembly '{targetScriptsAssemblyName}' not found.");
                return;
            }

            var dllTypes = dllAssembly.GetTypes()
                .Where(t => typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract)
                .ToDictionary(t => t.FullName, t => t);

            var soGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { targetSoDir });
            foreach (var guid in soGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var newPath = path;
                var oldAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

                if (oldAsset == null) continue;

                if (dllTypes.TryGetValue(oldAsset.GetType().FullName, out var newType))
                {
                    var serializedObject = new SerializedObject(oldAsset);
                    var scriptProperty = serializedObject.FindProperty("m_Script");
                    if(scriptProperty != null)
                    {
                        ScriptableObject replacement = ScriptableObject.CreateInstance(newType);
                        EditorUtility.CopySerialized(oldAsset, replacement);
                        AssetDatabase.CreateAsset(replacement, newPath);
                        globalSoMap[oldAsset] = replacement;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        internal static void RemapAllPrefabsToAssembly(string targetScriptsAssemblyName, string targetPrefabDir)
        {
            var dllAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == targetScriptsAssemblyName);
            if (dllAssembly == null)
            {
                Debug.LogError($"Failed to load assembly '{targetScriptsAssemblyName}'.");
                return;
            }

            var dllTypes = dllAssembly.GetTypes()
                .Where(t => typeof(MonoBehaviour).IsAssignableFrom(t) && !t.IsAbstract)
                .ToDictionary(t => t.FullName, t => t);

            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { targetPrefabDir });
            var processedPrefabs = new HashSet<string>();

            foreach (var guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (processedPrefabs.Contains(path)) continue;
                processedPrefabs.Add(path);

                var prefabRoot = PrefabUtility.LoadPrefabContents(path);
                var original = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                globalPrefabMap[original] = prefabRoot;

                try
                {
                    UnpackNestedPrefabs(prefabRoot);
                    var components = prefabRoot.GetComponentsInChildren<MonoBehaviour>(true);
                    var compMap = new Dictionary<GameObject, Dictionary<string, List<Component>>>();

                    foreach (var comp in components)
                    {
                        if (comp == null) continue;
                        var oldType = comp.GetType();
                        if (!dllTypes.TryGetValue(oldType.FullName, out var newType)) continue;

                        if (!compMap.TryGetValue(comp.gameObject, out var typeMap))
                            typeMap = compMap[comp.gameObject] = new();

                        if (!typeMap.TryGetValue(oldType.FullName, out var list))
                            list = typeMap[oldType.FullName] = new();

                        list.Add(comp);
                    }

                    foreach (var obj in compMap)
                    {
                        foreach (var list in obj.Value)
                        {
                            foreach (var oldComp in list.Value)
                            {
                                var newComp = obj.Key.AddComponent(dllTypes[oldComp.GetType().FullName]);
                                globalOldToNew[oldComp] = newComp;
                            }
                        }
                    }

                    foreach (var pair in globalOldToNew)
                    {
                        var oldSerialized = new SerializedObject(pair.Key);
                        var newSerialized = new SerializedObject(pair.Value);
                        CopySerializedProperties(oldSerialized, newSerialized);
                        Debug.Log($"✅ Remapped {pair.Key.GetType().FullName}");
                    }

                    foreach (var old in globalOldToNew.Keys)
                        UnityEngine.Object.DestroyImmediate(old);

                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                    globalOldToNew.Clear();
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CopySerializedProperties(SerializedObject source, SerializedObject destination)
        {
            source.Update();
            destination.Update();
            var iterator = source.GetIterator();
            if (!iterator.NextVisible(true)) return;

            do
            {
                if (iterator.name == "m_Script") continue;
                var destProp = destination.FindProperty(iterator.name);
                if (destProp == null) continue;
                CopyPropertyRecursive(iterator, destProp);
            } while (iterator.NextVisible(false));

            destination.ApplyModifiedProperties();
        }

        private static void CopyPropertyRecursive(SerializedProperty src, SerializedProperty dst)
        {
            switch (src.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    var obj = src.objectReferenceValue;
                    if (obj is Component c && globalOldToNew.TryGetValue(c, out var nc))
                        dst.objectReferenceValue = nc;
                    else if (obj is ScriptableObject so && globalSoMap.TryGetValue(so, out var nso))
                        dst.objectReferenceValue = nso;
                    else if (obj is GameObject go && globalPrefabMap.TryGetValue(go, out var ngo))
                        dst.objectReferenceValue = ngo;
                    else dst.objectReferenceValue = obj;
                    break;
                case SerializedPropertyType.Generic:
                    if (src.isArray)
                    {
                        dst.arraySize = src.arraySize;
                        for (int i = 0; i < src.arraySize; i++){
                            try{
                                CopyPropertyRecursive(src.GetArrayElementAtIndex(i), dst.GetArrayElementAtIndex(i));
                            }
                            catch(Exception e)
                            {
                                Debug.LogWarning($"Failed to copy propery in array: {e}");
                            }
                        }
                    }
                    else
                    {
                        var copy = src.Copy();
                        var depth = copy.depth;
                        while (copy.NextVisible(true) && copy.depth > depth)
                        {
                            var dp = dst.FindPropertyRelative(copy.name);
                            if (dp != null)
                                CopyPropertyRecursive(copy, dp);
                        }
                    }
                    break;
                default:
                    dst.serializedObject.CopyFromSerializedProperty(src);
                    break;
            }
        }

        private static void UnpackNestedPrefabs(GameObject root)
        {
            bool modified;
            do
            {
                modified = false;
                var transforms = root.GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                {
                    if (t.gameObject == root) continue;
                    if (PrefabUtility.IsPartOfPrefabInstance(t.gameObject) &&
                        PrefabUtility.IsOutermostPrefabInstanceRoot(t.gameObject))
                    {
                        PrefabUtility.UnpackPrefabInstance(t.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                        modified = true;
                        break;
                    }
                }
            } while (modified);
        }
    }
}

