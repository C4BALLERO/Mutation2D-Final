using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using MutationSwarm.Core;

namespace MutationSwarm.Editor
{
    [CustomEditor(typeof(Script_SpawnPointGizmos))]
    public class Script_SpawnPointGizmosEditor : UnityEditor.Editor
    {
        private Script_SpawnPointGizmos _manager;
        private SerializedProperty _levelEnemyPrefabs;
        private ReorderableList _enemyList;
        private bool _showSpawnPoints = true;
        private GameObject _massAssignPrefab;

        private void OnEnable()
        {
            _manager = (Script_SpawnPointGizmos)target;
            _levelEnemyPrefabs = serializedObject.FindProperty("_levelEnemyPrefabs");
            BuildEnemyReorderableList();
        }

        private void BuildEnemyReorderableList()
        {
            _enemyList = new ReorderableList(serializedObject, _levelEnemyPrefabs,
                draggable: true, displayHeader: true,
                displayAddButton: true, displayRemoveButton: true);

            _enemyList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, $"Enemigos del Nivel  ({_levelEnemyPrefabs.arraySize})", EditorStyles.boldLabel);
            };

            _enemyList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var element = _levelEnemyPrefabs.GetArrayElementAtIndex(index);
                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;

                // Color tag
                var tagRect = new Rect(rect.x, rect.y, 14f, rect.height);
                var fieldRect = new Rect(rect.x + 18f, rect.y, rect.width - 18f, rect.height);

                bool hasValue = element.objectReferenceValue != null;
                EditorGUI.DrawRect(tagRect, hasValue ? new Color(0.2f, 0.8f, 0.35f) : new Color(0.9f, 0.3f, 0.3f));
                EditorGUI.PropertyField(fieldRect, element, GUIContent.none);
            };

            _enemyList.elementHeight = EditorGUIUtility.singleLineHeight + 4f;

            _enemyList.onAddCallback = list =>
            {
                _levelEnemyPrefabs.arraySize++;
                _levelEnemyPrefabs.GetArrayElementAtIndex(_levelEnemyPrefabs.arraySize - 1).objectReferenceValue = null;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawEnemyListSection();
            EditorGUILayout.Space(4);
            DrawDistributeSection();
            EditorGUILayout.Space(8);
            DrawDivider();
            DrawSpawnPointsSection();
            EditorGUILayout.Space(4);
            DrawDivider();
            DrawMassAssignSection();
            EditorGUILayout.Space(4);
            DrawDivider();
            DrawTestSection();
            EditorGUILayout.Space(4);
            DrawDivider();

            // Arena settings at the bottom (collapsed by default)
            EditorGUILayout.Space(4);
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }

        // ── Enemy List ───────────────────────────────────────────────────────

        private void DrawEnemyListSection()
        {
            DrawSectionHeader("Enemigos del Nivel");

            if (_levelEnemyPrefabs.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "Agrega los prefabs de enemigos que aparecerán en este nivel.\n" +
                    "Luego usa los botones de Distribuir para asignarlos a los spawn points.",
                    MessageType.Info);
            }

            _enemyList.DoLayoutList();
        }

        // ── Distribution ─────────────────────────────────────────────────────

        private void DrawDistributeSection()
        {
            if (_levelEnemyPrefabs.arraySize == 0)
                return;

            int enemySpawnCount = CountEnemySpawnPoints();
            EditorGUILayout.LabelField($"Spawn points de enemigos disponibles: {enemySpawnCount}", EditorStyles.miniLabel);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.5f);
            if (GUILayout.Button("Distribuir en Orden", GUILayout.Height(30)))
            {
                Undo.RecordObjects(_manager.GetAllSpawnPoints(), "Distribuir Enemigos en Orden");
                _manager.DistributeRoundRobin();
                foreach (var sp in _manager.GetAllSpawnPoints())
                    EditorUtility.SetDirty(sp);
            }

            GUI.backgroundColor = new Color(0.5f, 0.6f, 0.9f);
            if (GUILayout.Button("Distribuir Aleatorio", GUILayout.Height(30)))
            {
                Undo.RecordObjects(_manager.GetAllSpawnPoints(), "Distribuir Enemigos Aleatorio");
                _manager.DistributeRandom();
                foreach (var sp in _manager.GetAllSpawnPoints())
                    EditorUtility.SetDirty(sp);
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        // ── Per-Spawn-Point List ──────────────────────────────────────────────

        private void DrawSpawnPointsSection()
        {
            _showSpawnPoints = EditorGUILayout.Foldout(_showSpawnPoints, "Spawn Points Individuales", true, EditorStyles.foldoutHeader);

            if (!_showSpawnPoints)
                return;

            SpawnPointChild[] spawnPoints = _manager.GetAllSpawnPoints();

            if (spawnPoints.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No hay spawn points.\n" +
                    "Crea GameObjects hijo, añade SpawnPointChild a cada uno y asigna los prefabs aquí.",
                    MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);

            foreach (var sp in spawnPoints)
                DrawSpawnPointRow(sp);

            EditorGUILayout.EndVertical();
        }

        private void DrawSpawnPointRow(SpawnPointChild sp)
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box);

            GUI.color = sp.IsPlayerSpawn ? Color.cyan : Color.red;
            EditorGUILayout.LabelField(
                $"{sp.gameObject.name}  {(sp.IsPlayerSpawn ? "[PLAYER]" : "[ENEMY]")}",
                GUILayout.Width(160));
            GUI.color = Color.white;

            EditorGUILayout.LabelField(
                $"({sp.SpawnPosition.x:F1}, {sp.SpawnPosition.y:F1})",
                EditorStyles.miniLabel, GUILayout.Width(90));

            var so = new SerializedObject(sp);
            var prefabProp = so.FindProperty("_prefabToSpawn");
            EditorGUILayout.PropertyField(prefabProp, GUIContent.none, GUILayout.ExpandWidth(true));
            so.ApplyModifiedProperties();

            GUI.backgroundColor = new Color(1f, 0.85f, 0.4f);
            if (GUILayout.Button("Spawn", GUILayout.Width(56)))
            {
                GameObject inst = sp.Spawn();
                if (inst != null)
                    Selection.activeGameObject = inst;
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        // ── Mass Assign ──────────────────────────────────────────────────────

        private void DrawMassAssignSection()
        {
            DrawSectionHeader("Asignar Prefab a Todos");

            _massAssignPrefab = (GameObject)EditorGUILayout.ObjectField(
                "Prefab", _massAssignPrefab, typeof(GameObject), false);

            if (_massAssignPrefab != null)
            {
                GUI.backgroundColor = new Color(1f, 0.75f, 0.3f);
                if (GUILayout.Button("Asignar a todos los spawn points", GUILayout.Height(28)))
                {
                    var spawnPoints = _manager.GetAllSpawnPoints();
                    Undo.RecordObjects(spawnPoints, "Asignar Prefab a Todos");
                    foreach (var sp in spawnPoints)
                    {
                        sp.PrefabToSpawn = _massAssignPrefab;
                        EditorUtility.SetDirty(sp);
                    }
                    Debug.Log($"[SpawnEditor] '{_massAssignPrefab.name}' asignado a {spawnPoints.Length} spawn points.");
                }
                GUI.backgroundColor = Color.white;
            }
        }

        // ── Test Spawning ────────────────────────────────────────────────────

        private void DrawTestSection()
        {
            DrawSectionHeader("Test Spawning");

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.5f, 1f, 0.6f);
            if (GUILayout.Button("✦ Spawn All", GUILayout.Height(32)))
            {
                _manager.SpawnAll();
                Debug.Log("[SpawnEditor] Todos los spawn points spawneados.");
            }

            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("Limpiar Spawns", GUILayout.Height(32)))
                ClearSpawnedObjects();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void ClearSpawnedObjects()
        {
            var spawnPoints = _manager.GetAllSpawnPoints();
            var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var spawnPointSet = new System.Collections.Generic.HashSet<GameObject>();
            foreach (var sp in spawnPoints)
                spawnPointSet.Add(sp.gameObject);

            int cleared = 0;
            foreach (var obj in allObjects)
            {
                if (obj == null || spawnPointSet.Contains(obj)) continue;
                if (obj.name.Contains("Enemy") || obj.name.Contains("Prefab") || obj.name.Contains("Player"))
                {
                    DestroyImmediate(obj);
                    cleared++;
                }
            }
            Debug.Log($"[SpawnEditor] {cleared} objetos eliminados.");
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void DrawSectionHeader(string title)
        {
            EditorGUILayout.Space(2);
            var rect = EditorGUILayout.GetControlRect(false, 20f);
            EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f));
            EditorGUI.LabelField(rect, $"  {title}", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
        }

        private static void DrawDivider()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, new Color(0.35f, 0.35f, 0.35f));
        }

        private int CountEnemySpawnPoints()
        {
            int count = 0;
            foreach (var sp in _manager.GetAllSpawnPoints())
                if (!sp.IsPlayerSpawn) count++;
            return count;
        }
    }
}
