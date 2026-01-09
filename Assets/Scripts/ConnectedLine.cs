using UnityEngine;



[RequireComponent(typeof(LineRenderer))]
public class ConnectedLine : MonoBehaviour
{
    public Transform objectA;
    public Transform objectB;
    public Color colorA = Color.red;
    public Color colorB = Color.blue;

    private LineRenderer _line;

    void Start()
    {
        _line = GetComponent<LineRenderer>();
        _line.positionCount = 2; // Only two points: Start and End

        SetupSplitGradient();
    }

    void Update()
    {
        if (objectA != null && objectB != null)
        {
            // Keep the line connected as they move
            _line.SetPosition(0, objectA.position);
            _line.SetPosition(1, objectB.position);
        }
    }

    void SetupSplitGradient()
    {
        Renderer rend = _line;
        if (rend != null)
        {
            // Create a 2x1 texture
            Texture2D texture = new Texture2D(2, 1);
            texture.filterMode = FilterMode.Point; // Keep sharp edge between colors
            texture.wrapMode = TextureWrapMode.Clamp;

            // Set pixel 0 to Color A, pixel 1 to Color B
            texture.SetPixel(0, 0, colorA);
            texture.SetPixel(1, 0, colorB);
            texture.Apply();

            // Create a new material instance to avoid changing other objects
            Material mat = new Material(Shader.Find("/2D/Sprite-Lit-Default")); // Or "Standard"
            mat.SetTexture("_MainTex", texture);
            //mat.mainTexture = texture;
            rend.material = mat;
        }
    }
}