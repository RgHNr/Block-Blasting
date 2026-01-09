using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    // Each list represents one of the 3-5 columns at the bottom
    public List<Canon>[] inventoryColumns = new List<Canon>[5];
    [SerializeField] GameObject canonPrefab, connectionPrefab;
    [SerializeField] public Vector3 offset;
    bool direction = false;

    private void Awake()
    {
        // 2. Initialize Singleton logic
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            // Optional: DontDestroyOnLoad(gameObject); 
        }
    }

    public void Init(LevelData levelData)
    {
        float columnSpacing = 2f; // Distance between columns (Left/Right)
        float rowSpacing = 2f;    // Distance between cannons in a stack (Up/Down)

        int totalCols = levelData.inventoryColumns.Count;

        for (int colIndex = 0; colIndex < totalCols; colIndex++)
        {
            // 1. Calculate X Position
            // This math automatically handles both Even and Odd cases
            float x = (colIndex - (totalCols - 1) / 2f) * columnSpacing;

            // Apply to the offset
            float finalX = offset.x + x;

            InventoryColumn currentColumn = levelData.inventoryColumns[colIndex];
            Instance.inventoryColumns[colIndex] = new();

            for (int rowIndex = 0; rowIndex < currentColumn.cannons.Count; rowIndex++)
            {
                // 2. Calculate Z Position (assuming 'y' in your snippet meant Z-axis depth)
                // We usually subtract rowIndex to stack them "downwards" or add to stack "upwards"
                float z = offset.z - (rowIndex * rowSpacing);

                // 3. Create the Spawn Location
                Vector3 location = new Vector3(finalX, offset.y, z);

                // 4. Instantiate and Initialize
                CannonConfig data = currentColumn.cannons[rowIndex];
                GameObject canonObj = Instantiate(canonPrefab, location, Quaternion.identity);

                Canon canonScript = canonObj.GetComponent<Canon>();
                canonScript.Init( data.visualColor, data.ammoCount,colIndex,data.linkID);
                if (levelData.HasHiddenCanons)
                {
                    if (levelData.AreAllHidden)
                    {
                        canonScript.ToggleMaterial(true);
                    }
                    else
                    {
                        int rand = Random.Range(0, 2);
                        Debug.Log("Random= " + rand.ToString());
                        if (rand == 1) canonScript.ToggleMaterial(true);
                    }
                }
                canonScript.enabled = false;
                // Add to your runtime inventory list so you can track/move them later
                InventoryManager.Instance.inventoryColumns[colIndex].Add(canonScript);
            }
        }
        UpdateColumnOutlines();
        CreateLinks();
       
    }
    private void CreateLinks()
    {
        List<Canon> allCannons = new List<Canon>();
        foreach (var col in inventoryColumns) {

            if (col != null) { 
                allCannons.AddRange(col);
                Debug.Log("Added range canons to list");
            }
            else Debug.Log("col is null");
        }

        foreach (var cannon in allCannons)
        {
            // If has ID, hasn't been linked yet, and isn't 0
            if (cannon.linkID != 0 && cannon.connectedCanon == null)
            {
                // Find its partner
                Canon partner = allCannons.Find(c => c != cannon && c.linkID == cannon.linkID);

                if (partner != null)
                {
                    // Link them logically
                    cannon.connectedCanon = partner;
                    partner.connectedCanon = cannon;

                    // Link them Visually (Spawn a bar between them)
                    DrawConnection(cannon, partner);
                }
            }
        }
    }

    private void UpdateColumnOutlines()
    {
        // Loop through every column
        foreach (var column in inventoryColumns)
        {
            if (column == null) continue;

            for (int i = 0; i < column.Count; i++)
            {
                Canon canon = column[i];

                // If i == 0, it's the top one -> Turn Outline ON. 
                // Otherwise -> Turn Outline OFF.
                bool isTop = (i == 0);
                canon.ToggleOutline(isTop);
                if (canon.connectedCanon != null) canon.connectedCanon.ToggleOutline(isTop);
                if(isTop)canon.ToggleMaterial(false);

            }
        }
    }

    private void DrawConnection(Canon a, Canon b)
    {
        // 1. Instantiate the connector prefab
        Vector3 center = (a.transform.position + b.transform.position) / 2;
        GameObject connector = Instantiate(connectionPrefab, center, Quaternion.identity);

        // 2. Rotate and Scale
        connector.transform.LookAt(b.transform);
        //connector.transform.eulerAngles = new Vector3(connector.transform.eulerAngles.x, connector.transform.eulerAngles.y, 90);
        float distance = Vector3.Distance(a.transform.position, b.transform.position);
        // Adjust scale: Z is usually the "forward" axis in LookAt, so we scale Z to bridge the gap
        // Adjust (0.2f, 0.2f) to match your bar's thickness
        connector.transform.localScale = new Vector3(0.2f, 0.2f, distance);

        // 3. APPLY THE HALF-HALF COLOR
        Renderer rend = connector.GetComponent<Renderer>();
        if (rend != null)
        {
            // Create a 2x1 texture
            Texture2D texture = new Texture2D(1, 2);
            texture.filterMode = FilterMode.Point; // Keep sharp edge between colors
            texture.wrapMode = TextureWrapMode.Clamp;

            // Set pixel 0 to Color A, pixel 1 to Color B
            texture.SetPixel(0, 1, a.color);
            texture.SetPixel(0, 0, b.color);
            texture.Apply();

            // Create a new material instance to avoid changing other objects
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); // Or "Standard"
            mat.mainTexture = texture;
            rend.material = mat;
        }

        // 4. Save references
        a.connectorObject = connector;
        b.connectorObject = connector;
    }


    public void OnCannonClicked(Canon clickedCanon, int columnIndex)
    {
        // CASE 1: Linked Cannons
        if (clickedCanon.connectedCanon != null)
        {
            Canon partner = clickedCanon.connectedCanon;

            // Use the new helper to find adjacent slots
            int startSlot = SlotManager.Instance.GetAdjacentEmptySlots();

            if (startSlot != -1)
            {
                // Destroy visual connector
                //clickedCanon.DestroyConnection();

                // Move them to startSlot and startSlot + 1
                MoveSingleCannon(clickedCanon, columnIndex, startSlot);
                MoveSingleCannon(partner, partner.ColumnIndex, startSlot + 1);
            }
            else
            {
                // Optional: Shake animation to show "No room!"
                clickedCanon.transform.DOShakePosition(0.5f, 0.2f);
            }
        }
        // CASE 2: Single Cannon
        else
        {
            // ... existing single logic ...
            int slotIndex = SlotManager.Instance.GetFirstEmptySlot();

            if (slotIndex != -1) // If a slot is available
            {
                // 2. Remove from inventory list
                inventoryColumns[columnIndex].Remove(clickedCanon);

                // 3. Move the cannon to the middle slot
                SlotManager.Instance.AssignCannonToSlot(clickedCanon, slotIndex);

                // 4. Shift the remaining cannons in this column UP
                foreach (var cannon in inventoryColumns[columnIndex])
                {
                    // Similar to your Block.MoveDownWard logic, but moving UP
                    cannon.transform.DOMoveZ(cannon.transform.position.z + 2, 0.6f).OnUpdate(() => { if (cannon.connectedCanon != null) cannon.UpdateConnectorPosition(); }); 
                }
            }
        }
        UpdateColumnOutlines();
    }

    private void MoveSingleCannon(Canon cannon, int colIndex, int slotIndex)
    {
        int amountToMove = 1;
        if (cannon.connectedCanon != null && cannon.connectedCanon.ColumnIndex == colIndex) amountToMove = 2;


        inventoryColumns[colIndex].Remove(cannon);
        if (amountToMove == 2) inventoryColumns[colIndex].Remove(cannon.connectedCanon);
        SlotManager.Instance.AssignCannonToSlot(cannon, slotIndex);

        float shiftDistance = 2 * amountToMove;
        // Shift remaining cannons up
        foreach (var c in inventoryColumns[colIndex])
        {
            c.transform.DOMoveZ(c.transform.position.z + shiftDistance, 0.6f).OnUpdate(() => { if (c.connectedCanon != null) c.UpdateConnectorPosition(); });
        }

        // Update outlines (from previous question)
        UpdateColumnOutlines();
    }

    //public void OnCannonClicked(Canon clickedCanon, int columnIndex)
    //{
       
    //    // 1. Find the first empty slot in the middle
    //    int slotIndex = SlotManager.Instance.GetFirstEmptySlot();

    //    if (slotIndex != -1) // If a slot is available
    //    {
    //        // 2. Remove from inventory list
    //        inventoryColumns[columnIndex].Remove(clickedCanon);

    //        // 3. Move the cannon to the middle slot
    //        SlotManager.Instance.AssignCannonToSlot(clickedCanon, slotIndex);

    //        // 4. Shift the remaining cannons in this column UP
    //        foreach (var cannon in inventoryColumns[columnIndex])
    //        {
    //            // Similar to your Block.MoveDownWard logic, but moving UP
    //            cannon.transform.DOMoveZ(cannon.transform.position.z+2, 0.6f);
    //        }
    //        UpdateColumnOutlines();
    //    }
    //}
}