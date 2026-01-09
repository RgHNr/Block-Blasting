using UnityEngine;
using TMPro;
using DG.Tweening;

public class Canon : MonoBehaviour
{
    public Color color;
    public BlockColor blockColor;
    [SerializeField] int Count;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] float shootCooldown = 0.5f; // Time between shots
    [SerializeField] GameObject projectilePrefab  , outlineObject; // Assign your projectile in Inspector
    [SerializeField] Transform firePoint; // Where the projectile spawns
    [SerializeField] TMP_Text CountText;
    [SerializeField] Material HiddenMaterial;
    Material OriginalMaterial;
   
    public int ColumnIndex;
    private float lastShootTime;
    MaterialPropertyBlock mpb;
    MeshRenderer renderer;
    [SerializeField] float speed;
    public Canon connectedCanon; // The actual script of the partner
    public int linkID;           // The ID from data
    public GameObject connectorObject; // The visual bar/chain
    void Update()
    {
        //if (connectorObject != null) UpdateConnectorPosition();

        if (Count <= 0)
        {
            HandleExitLogic();
            return;
        }
        // Only attempt to shoot if cooldown has passed
        if (Time.time >= lastShootTime + shootCooldown )
        {
            CheckAndShoot();

        }
    }

    private void HandleExitLogic()
    {
        // Case A: I am a single cannon -> Just leave.
        if (connectedCanon == null)
        {
            RemoveCanon();
            return;
        }

        // Case B: I have a partner -> Check their status.
        // If my partner still has ammo, I must WAIT.
        if (connectedCanon.Count > 0)
        {
            // Optional: Play an "Idle/Waiting" animation here
            // Do NOT destroy myself yet.
            return;
        }

        // Case C: My partner is ALSO empty -> We both leave together.
        // To prevent double-calling, we can check a flag or just let both call LeaveSlot() safely.
        RemoveCanon();
    }
    public void ToggleOutline(bool isActive)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(isActive);
        }
    }

    public void ToggleMaterial(bool isHidden)
    {
        if (isHidden)
        {
            renderer.sharedMaterial = HiddenMaterial;

            renderer.SetPropertyBlock(null);
        }
        else
        {
            renderer.sharedMaterial = OriginalMaterial;
            renderer.SetPropertyBlock(mpb);
        }
        CountText.gameObject.SetActive(!isHidden);
       
    }

    public void Init(Color color, int count, int index, int linkID)
    {
        this.color = color;
        this.Count = count;
        this.ColumnIndex = index;
        renderer = GetComponent<MeshRenderer>();
        OriginalMaterial = renderer.sharedMaterial;
        mpb = new();
        mpb.SetColor("_BaseColor", color);
        renderer.SetPropertyBlock(mpb);
        UpdateCountText(Count);
        this.linkID = linkID;
    }

    private void Start()
    {
        
    }

    void UpdateCountText(int count)
    {
        CountText.text = count.ToString();
    }

    public void UpdateCountAndText(int newCount)
    {
        Count += newCount;
        UpdateCountText(Count);
        
    }

    public int GetCurrentCount()
    {
        return Count;
    }

    public void UpdateConnectorPosition()
    {
        if (connectorObject != null)
        {
            Vector3 center = (transform.position + connectedCanon.transform.position) / 2;
            connectorObject.transform.position = center;
            connectorObject.transform.LookAt(connectedCanon.transform);
            //connectorObject.transform.eulerAngles = new Vector3(connectorObject.transform.eulerAngles.x, connectorObject.transform.eulerAngles.y, 90);
            float distance = Vector3.Distance(transform.position, connectedCanon.transform.position);
            connectorObject.transform.localScale = new Vector3(0.2f, 0.2f, distance);
        }
    }


    void CheckAndShoot()
    {
        // Iterate through all columns in the Grid
        for (int i = 0; i < GridManager.Grid.Length; i++)
        {
            var column = GridManager.Grid[i];

            // Check if the column exists and has blocks
            if (column != null && column.Count > 0)
            {
                Block firstBlock = column[0];

                // Check if the block is valid, not destroyed, and matches color
                if (firstBlock != null && AreColorsSimilar(firstBlock.BColor,color) && !firstBlock.IsDestroyed)
                {

                    Shoot(firstBlock);
                    lastShootTime = Time.time;
                    transform.DOLookAt(firstBlock.transform.position, 0.2f);
                    AudioManager.Instance.PlayShoot();
                    // Break or return if you only want to shoot one projectile per cooldown period
                    // Remove 'break' if you want it to shoot at ALL matching columns simultaneously
                    break;
                }
            }
        }
    }

    public bool AreColorsSimilar(Color a, Color b, float threshold = 0.1f)
    {
        // Calculates the Euclidean distance between two colors
        float distance = Vector4.Distance(a, b);

        // If the distance is smaller than the threshold, they are "the same"
        return distance < threshold;
    }

    void Shoot(Block target)
    {
        //Debug.Log($"Shooting at {target.name}");

        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            Projectile ball = proj.GetComponent<Projectile>();
            ball.target = target.transform;
            target.IsDestroyed = true;
            ball.targetBlock = target;
            Count--;
            UpdateCountText(Count);
            LevelManager.Instance.OnBlockShot();
            if (Count <= 0)
            {
                HandleExitLogic();
            }
            //ball.MoveToTarget();
            // Assuming your projectile has a script to move it
            // Projectile script should handle moving towards target.transform.position
            // and calling the GridManager's removal logic on impact.
        }
    }

    void RemoveCanon()
    {
        //Debug.Log("connected canon count: " + connectedCanon.Count);
        //Debug.Log("Removing Canon.....");
        int SlotIndex = SlotManager.Instance.GetCanonSlotIndexAndRemove(this);
        if (SlotIndex == -1) return;
        int multiplier = SlotIndex >= SlotManager.Instance.occupiedSlots.Length / 2 ? 2 : -1 ;

        if (connectedCanon != null) multiplier = -1;
        float dis = Vector3.Distance(new Vector3(10 * multiplier, 0, -4), transform.position);
       transform.DOMove(new Vector3(10 * multiplier, 0, -4), dis/speed ).OnUpdate(() => { UpdateConnectorPosition(); });

        
                
                
    }
}
