using UnityEditor;
using UnityEngine;
using MutationSwarm.Core;

namespace MutationSwarm.Editor
{
    [CustomEditor(typeof(SpawnPointChild))]
    public class SpawnPointChildEditor : UnityEditor.Editor
    {
        private SerializedProperty _prefabToSpawn;
        private SerializedProperty _isPlayerSpawn;

        private void OnEnable()
        {
            _prefabToSpawn = serializedObject.FindProperty("_prefabToSpawn");
            _isPlayerSpawn = serializedObject.FindProperty("_isPlayerSpawn");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Label("Spawn Point Child", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_prefabToSpawn, new GUIContent("Prefab to Spawn", "Arrastra aquí el prefab que se instanciará en este spawn point."));
            EditorGUILayout.PropertyField(_isPlayerSpawn, new GUIContent("Is Player Spawn", "Marca si este punto es para un jugador en lugar de un enemigo."));

            if (_prefabToSpawn.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Arrastra un prefab aquí para poder spawnear el objeto en esta posición.", MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Position", EditorStyles.miniLabel);
            EditorGUILayout.Vector3Field("World Position", ((SpawnPointChild)target).transform.position);

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(Script_39_SpawnPoint))]
    public class Script39SpawnPointEditor : UnityEditor.Editor
    {
        private SerializedProperty _spawnType;
        private SerializedProperty _prefabToSpawn;
        private SerializedProperty _showGizmo;

        private void OnEnable()
        {
            _spawnType = serializedObject.FindProperty("_spawnType");
            _prefabToSpawn = serializedObject.FindProperty("_prefabToSpawn");
            _showGizmo = serializedObject.FindProperty("_showGizmo");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Label("Spawn Point", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_spawnType, new GUIContent("Spawn Type", "Selecciona si este spawn point genera enemigos o el jugador."));
            EditorGUILayout.PropertyField(_prefabToSpawn, new GUIContent("Prefab to Spawn", "Arrastra aquí el prefab que se instanciará en este spawn point."));
            EditorGUILayout.PropertyField(_showGizmo, new GUIContent("Show Gizmo", "Muestra el gizmo del spawn point en la escena."));

            if (_prefabToSpawn.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Arrastra un prefab aquí para asignar este spawn point.", MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Position", EditorStyles.miniLabel);
            EditorGUILayout.Vector3Field("World Position", ((Script_39_SpawnPoint)target).transform.position);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
