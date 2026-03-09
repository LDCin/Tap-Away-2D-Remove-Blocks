﻿using UnityEditor;
using UnityEngine;

namespace Scripts.EditorTools
{
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : UnityEditor.Editor
    {
        private enum BrushMode
        {
            Paint,
            Erase
        }

        private int _selectedLevelIndex;
        private BrushMode _brushMode = BrushMode.Paint;
        private int _brushColorId;
        private Direction _brushDirection = Direction.Right;
        private BlockShape _brushShape = BlockShape.Single;
        private float _cellButtonSize = 30f;

        public override void OnInspectorGUI()
        {
            var levelData = (LevelData)target;
            if (levelData.Levels == null)
            {
                DrawDefaultInspector();
                return;
            }

            DrawToolbar(levelData);

            if (levelData.Count == 0)
            {
                EditorGUILayout.HelpBox("No levels. Click Add Level to start.", MessageType.Info);
                return;
            }

            _selectedLevelIndex = Mathf.Clamp(_selectedLevelIndex, 0, levelData.Count - 1);
            _selectedLevelIndex = EditorGUILayout.IntSlider("Selected Level", _selectedLevelIndex, 0, levelData.Count - 1);

            LevelConfig level = levelData.Levels[_selectedLevelIndex];
            DrawLevelSettings(levelData, level);
            DrawBrushSettings();
            DrawGrid(levelData, level);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(levelData);
            }
        }

        private void DrawToolbar(LevelData levelData)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Level"))
            {
                Undo.RecordObject(levelData, "Add Level");
                levelData.AddLevel();
                _selectedLevelIndex = Mathf.Max(0, levelData.Count - 1);
                EditorUtility.SetDirty(levelData);
            }

            using (new EditorGUI.DisabledScope(levelData.Count == 0))
            {
                if (GUILayout.Button("Remove Selected"))
                {
                    Undo.RecordObject(levelData, "Remove Level");
                    levelData.RemoveLevelAt(_selectedLevelIndex);
                    _selectedLevelIndex = Mathf.Clamp(_selectedLevelIndex, 0, Mathf.Max(0, levelData.Count - 1));
                    EditorUtility.SetDirty(levelData);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLevelSettings(LevelData levelData, LevelConfig level)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            int newWidth = EditorGUILayout.IntField("Width", level.width);
            int newHeight = EditorGUILayout.IntField("Height", level.height);
            float newPadding = EditorGUILayout.FloatField("Screen Padding", level.screenPadding);
            float newBlockFillRatio = EditorGUILayout.Slider("Block Fill Ratio", level.blockFillRatio, 0.5f, 1f);
            int newMoveLimit = EditorGUILayout.IntField("Move Limit", level.moveLimit);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(levelData, "Edit Level Settings");
                level.width = Mathf.Max(1, newWidth);
                level.height = Mathf.Max(1, newHeight);
                level.screenPadding = Mathf.Max(0f, newPadding);
                level.blockFillRatio = Mathf.Clamp(newBlockFillRatio, 0.5f, 1f);
                level.moveLimit = Mathf.Max(1, newMoveLimit);
                level.ClampAndDeduplicate();
                EditorUtility.SetDirty(levelData);
            }
        }

        private void DrawBrushSettings()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Brush", EditorStyles.boldLabel);
            _brushMode = (BrushMode)EditorGUILayout.EnumPopup("Mode", _brushMode);
            _brushColorId = EditorGUILayout.IntField("Color Id", _brushColorId);
            _brushDirection = (Direction)EditorGUILayout.EnumPopup("Direction", _brushDirection);
            _brushShape = (BlockShape)EditorGUILayout.EnumPopup("Shape", _brushShape);
            _cellButtonSize = EditorGUILayout.Slider("Cell Size", _cellButtonSize, 20f, 42f);
        }

        private void DrawGrid(LevelData levelData, LevelConfig level)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Click to paint with current brush. Use Erase mode to clear cells.", MessageType.None);

            for (int y = level.height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < level.width; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    DrawCell(levelData, level, pos);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField($"Total blocks: {level.blocks.Count}");
        }

        private void DrawCell(LevelData levelData, LevelConfig level, Vector2Int pos)
        {
            bool hasBlock = level.TryGetBlockAt(pos, out LevelBlockSpawn block);
            string label = hasBlock ? $"{block.colorId}\n{ToShortDir(block.direction)}" : ".";

            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = hasBlock ? new Color(0.58f, 0.82f, 1f) : Color.white;

            if (GUILayout.Button(label, GUILayout.Width(_cellButtonSize), GUILayout.Height(_cellButtonSize)))
            {
                Undo.RecordObject(levelData, _brushMode == BrushMode.Erase ? "Erase Cell" : "Paint Cell");
                ApplyBrush(level, pos);
                level.ClampAndDeduplicate();
                EditorUtility.SetDirty(levelData);
            }

            GUI.backgroundColor = oldColor;
        }

        private void ApplyBrush(LevelConfig level, Vector2Int anchor)
        {
            if (_brushMode == BrushMode.Erase)
            {
                level.RemoveBlockAt(anchor);
                return;
            }

            Vector2Int[] offsets = LevelConfig.GetShapeOffsets(_brushShape);
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector2Int pos = anchor + offsets[i];
                level.SetBlockAt(pos, _brushColorId, _brushDirection, _brushShape);
            }
        }

        private static string ToShortDir(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return "U";
                case Direction.Down:
                    return "D";
                case Direction.Left:
                    return "L";
                case Direction.Right:
                    return "R";
                default:
                    return "?";
            }
        }
    }
}
