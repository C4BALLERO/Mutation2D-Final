#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace MutationSwarm
{
    [InitializeOnLoad]
    public static class TagSetup
    {
        static TagSetup()
        {
            string[] tags = { "Ground", "Platform", "Enemy", "Defense", "DNAPickup" };
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");
            bool changed = false;
            foreach (var tag in tags)
            {
                bool found = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                    if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) { found = true; break; }
                if (!found)
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                    changed = true;
                }
            }
            if (changed)
            {
                tagManager.ApplyModifiedProperties();
                Debug.Log("[MutationSwarm] Tags registered.");
            }
        }
    }
}
#endif
