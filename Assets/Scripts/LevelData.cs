using UnityEngine;
using System.Collections.Generic;

public enum TutorialCondition { None, SingleClick, ThreeClicks, FirstFuse, ButtonClick }

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public int layerCount;

    // --- NEW: Save the dimensions so we can reconstruct the grid ---
    public int rowCount = 12;
    public int colCount = 10;

    // Stores the grid layout
    public List<GridLayer> layers = new List<GridLayer>();

    // Stores the Canon Inventory configuration
    public List<InventoryColumn> inventoryColumns = new List<InventoryColumn>();

    [Header("Hidden Canons")]
    public bool HasHiddenCanons;
    public bool AreAllHidden;

    [Header("Tutorial Settings")]
    public bool hasTutorial;
    public TutorialCondition dismissCondition;
}

[System.Serializable]
public class GridLayer
{
    // REMOVED the "= new Color[10 * 12]" so it isn't locked to a specific size.
    // The Editor tool will define the size when saving.
    public Color[] gridColors;
}