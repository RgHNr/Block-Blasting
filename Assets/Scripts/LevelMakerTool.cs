using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// --- 1. Define Helper Classes for Inventory Data ---
[System.Serializable]
public class CannonConfig
{
    public Color visualColor = Color.red;
    public BlockColor blockColor = BlockColor.Red;
    public int ammoCount = 10;
    public int linkID;
}

[System.Serializable]
public class InventoryColumn
{
    public List<CannonConfig> cannons = new List<CannonConfig>();
}

// --- 2. The Main Editor Tool ---
public class LevelMakerTool : EditorWindow
{
    // --- Tool Settings ---
    private string levelName = "Level_01";

    private int layerCount = 1;
    private Color currentPaintColor = Color.green;
    private int currentLayerTab = 0;

    // --- Inventory Data ---
    private List<InventoryColumn> inventoryData = new List<InventoryColumn>();
    private Vector2 inventoryScrollPos;

    // --- NEW: Main Window Scroll Position ---
    private Vector2 windowScrollPos;

    // --- Grid Data Storage ---
    private List<Color[]> workingLayers;

    // Default values
    private int Rows = 12;
    private const int Cols = 10;

    private const float CellSize = 30f;
    private const float CellPadding = 2f;

    // --- Selection / Dragging Variables ---
    private bool isDragging = false;
    private Vector2Int dragStartPos;
    private Vector2Int dragCurrentPos;

    [MenuItem("Tools/Level Maker")]
    public static void ShowWindow()
    {
        GetWindow<LevelMakerTool>("Level Maker");
    }

    private void OnEnable()
    {
        if (workingLayers == null) InitializeGridData();
        if (inventoryData == null) inventoryData = new List<InventoryColumn>();
    }

    private void InitializeGridData()
    {
        workingLayers = new List<Color[]>();
        for (int i = 0; i < 3; i++)
        {
            Color[] newLayer = new Color[Rows * Cols];
            for (int j = 0; j < newLayer.Length; j++) newLayer[j] = Color.white;
            workingLayers.Add(newLayer);
        }
    }

    private void ResizeGridData(int newRowCount)
    {
        int oldRows = Rows;
        Rows = newRowCount;

        for (int i = 0; i < workingLayers.Count; i++)
        {
            Color[] oldLayer = workingLayers[i];
            Color[] newLayer = new Color[Rows * Cols];
            for (int k = 0; k < newLayer.Length; k++) newLayer[k] = Color.white;

            int minRows = Mathf.Min(oldRows, Rows);
            for (int r = 0; r < minRows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    int oldIndex = r * Cols + c;
                    int newIndex = r * Cols + c;
                    if (oldIndex < oldLayer.Length && newIndex < newLayer.Length)
                    {
                        newLayer[newIndex] = oldLayer[oldIndex];
                    }
                }
            }
            workingLayers[i] = newLayer;
        }
    }

    private void OnGUI()
    {
        // --- START SCROLL VIEW ---
        windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);
        EditorGUILayout.BeginVertical();

        GUILayout.Label("Level Configuration", EditorStyles.boldLabel);
        levelName = EditorGUILayout.TextField("Level Name", levelName);
        layerCount = EditorGUILayout.IntSlider("Number of Layers", layerCount, 1, 3);

        int newRows = EditorGUILayout.IntField("Number of Rows", Rows);
        if (newRows != Rows && newRows > 0)
        {
            ResizeGridData(newRows);
        }

        EditorGUILayout.Space();

        GUILayout.Label("Painting Tools", EditorStyles.boldLabel);
        currentPaintColor = EditorGUILayout.ColorField("Paint Brush Color", currentPaintColor);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Red")) currentPaintColor = Color.red;
        if (GUILayout.Button("Yellow")) currentPaintColor = Color.yellow;
        if (GUILayout.Button("Green")) currentPaintColor = Color.green;
        if (GUILayout.Button("Blue")) currentPaintColor = Color.blue;
        if (GUILayout.Button("Eraser")) currentPaintColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (layerCount > 1)
        {
            string[] tabs = new string[layerCount];
            for (int i = 0; i < layerCount; i++) tabs[i] = $"Layer {i + 1}";
            currentLayerTab = GUILayout.Toolbar(currentLayerTab, tabs);
        }
        else currentLayerTab = 0;

        EditorGUILayout.Space();

        DrawCustomGrid(currentLayerTab);

        EditorGUILayout.Space();

        DrawInventoryEditor();

        EditorGUILayout.Space(20);

        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Save Level to Asset", GUILayout.Height(40)))
        {
            SaveLevel();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndVertical();
        // --- END SCROLL VIEW ---
        EditorGUILayout.EndScrollView();
    }

    private void DrawInventoryEditor()
    {
        GUILayout.Label("Canon Inventory Setup", EditorStyles.boldLabel);
        if (GUILayout.Button("+ Add New Column", GUILayout.Width(150))) inventoryData.Add(new InventoryColumn());
        EditorGUILayout.Space();

        // This is a nested scroll view for the inventory (Horizontal)
        inventoryScrollPos = EditorGUILayout.BeginScrollView(inventoryScrollPos, GUILayout.Height(300));
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < inventoryData.Count; i++) DrawOneInventoryColumn(i);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    private void DrawOneInventoryColumn(int colIndex)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(200));
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"Column {colIndex + 1}", EditorStyles.boldLabel);
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            inventoryData.RemoveAt(colIndex);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        InventoryColumn col = inventoryData[colIndex];
        for (int j = 0; j < col.cannons.Count; j++)
        {
            CannonConfig canon = col.cannons[j];
            EditorGUILayout.BeginVertical("box");
            canon.visualColor = EditorGUILayout.ColorField("Color", canon.visualColor);
            canon.blockColor = (BlockColor)EditorGUILayout.EnumPopup("Logic Type", canon.blockColor);
            canon.ammoCount = EditorGUILayout.IntField("Count", canon.ammoCount);
            canon.linkID = EditorGUILayout.IntField("LinkID", canon.linkID);
            if (GUILayout.Button("Remove Canon")) { col.cannons.RemoveAt(j); break; }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
        EditorGUILayout.Space();
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("+ Add Canon")) col.cannons.Add(new CannonConfig());
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();
    }

    private void DrawCustomGrid(int layerIndex)
    {
        float totalWidth = Cols * (CellSize + CellPadding);
        float totalHeight = Rows * (CellSize + CellPadding);

        Rect gridRect = GUILayoutUtility.GetRect(totalWidth, totalHeight);

        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.black;

        if (workingLayers[layerIndex].Length != Rows * Cols)
        {
            InitializeGridData();
        }

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                float yPos = gridRect.y + totalHeight - ((r + 1) * (CellSize + CellPadding));
                Rect cellRect = new Rect(gridRect.x + c * (CellSize + CellPadding), yPos, CellSize, CellSize);

                int index = (r * Cols) + c;

                if (index < workingLayers[layerIndex].Length)
                {
                    Color cellColor = workingLayers[layerIndex][index];
                    EditorGUI.DrawRect(cellRect, cellColor);
                }
            }
        }

        HandleMouseEvents(gridRect, totalHeight, layerIndex);
    }

    private void HandleMouseEvents(Rect gridRect, float totalHeight, int layerIndex)
    {
        Event e = Event.current;
        Vector2 mousePos = e.mousePosition;
        bool isMouseOverGrid = gridRect.Contains(mousePos);

        int mouseCol = Mathf.FloorToInt((mousePos.x - gridRect.x) / (CellSize + CellPadding));
        float gridBottom = gridRect.y + totalHeight;
        int mouseRow = Mathf.FloorToInt((gridBottom - mousePos.y) / (CellSize + CellPadding));

        mouseCol = Mathf.Clamp(mouseCol, 0, Cols - 1);
        mouseRow = Mathf.Clamp(mouseRow, 0, Rows - 1);
        Vector2Int gridPos = new Vector2Int(mouseCol, mouseRow);

        if (e.type == EventType.MouseDown && e.button == 0 && isMouseOverGrid)
        {
            isDragging = true;
            dragStartPos = gridPos;
            dragCurrentPos = gridPos;
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && isDragging)
        {
            if (dragCurrentPos != gridPos)
            {
                dragCurrentPos = gridPos;
                Repaint();
            }
            e.Use();
        }
        else if (e.type == EventType.MouseUp && isDragging)
        {
            isDragging = false;
            ApplyPaintToSelection(layerIndex);
            Repaint();
            e.Use();
        }

        if (isDragging) DrawSelectionRect(gridRect, totalHeight);
    }

    private void DrawSelectionRect(Rect gridRect, float totalHeight)
    {
        int minCol = Mathf.Min(dragStartPos.x, dragCurrentPos.x);
        int maxCol = Mathf.Max(dragStartPos.x, dragCurrentPos.x);
        int minRow = Mathf.Min(dragStartPos.y, dragCurrentPos.y);
        int maxRow = Mathf.Max(dragStartPos.y, dragCurrentPos.y);

        float x = gridRect.x + minCol * (CellSize + CellPadding);
        float y = gridRect.y + totalHeight - ((maxRow + 1) * (CellSize + CellPadding));
        float width = (maxCol - minCol + 1) * (CellSize + CellPadding) - CellPadding;
        float height = (maxRow - minRow + 1) * (CellSize + CellPadding) - CellPadding;

        Color highlightColor = currentPaintColor;
        highlightColor.a = 0.4f;
        EditorGUI.DrawRect(new Rect(x, y, width, height), highlightColor);
    }

    private void ApplyPaintToSelection(int layerIndex)
    {
        int minCol = Mathf.Min(dragStartPos.x, dragCurrentPos.x);
        int maxCol = Mathf.Max(dragStartPos.x, dragCurrentPos.x);
        int minRow = Mathf.Min(dragStartPos.y, dragCurrentPos.y);
        int maxRow = Mathf.Max(dragStartPos.y, dragCurrentPos.y);

        for (int r = minRow; r <= maxRow; r++)
        {
            for (int c = minCol; c <= maxCol; c++)
            {
                int index = (r * Cols) + c;
                if (index < workingLayers[layerIndex].Length)
                {
                    workingLayers[layerIndex][index] = currentPaintColor;
                }
            }
        }
    }

    private void SaveLevel()
    {
        LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
        newLevel.levelName = levelName;
        newLevel.layerCount = layerCount;
        newLevel.rowCount = Rows;
        newLevel.colCount = Cols;

        for (int i = 0; i < layerCount; i++)
        {
            GridLayer layer = new GridLayer();
            layer.gridColors = (Color[])workingLayers[i].Clone();
            newLevel.layers.Add(layer);
        }

        foreach (var col in inventoryData)
        {
            InventoryColumn newCol = new InventoryColumn();
            foreach (var cannon in col.cannons)
            {
                CannonConfig newCannon = new CannonConfig();
                newCannon.visualColor = cannon.visualColor;
                newCannon.blockColor = cannon.blockColor;
                newCannon.ammoCount = cannon.ammoCount;
                newCannon.linkID = cannon.linkID;
                newCol.cannons.Add(newCannon);
            }
            newLevel.inventoryColumns.Add(newCol);
        }

        string path = $"Assets/{levelName}.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CreateAsset(newLevel, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Level saved to {path} with {Rows} Rows.", "OK");
    }
}