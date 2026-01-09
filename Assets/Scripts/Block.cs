using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum BlockColor
{
    Blue,
    Red,
    Yellow
}
public class Block : MonoBehaviour
{


    [SerializeField] public BlockColor color;
    public Color BColor;
    Transform transform;
    [SerializeField]float MoveDuration , shrinkDuration;
    
    [SerializeField] Ease easeType , shrinkEaseType;
    public bool IsDestroyed=false;
    public int Column;
    MaterialPropertyBlock mpb;
    MeshRenderer renderer;
    private static readonly int ColorID = Shader.PropertyToID("_BaseColor");
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform = GetComponent<Transform>();
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateColor(Color color)
    {
        BColor = color;
        mpb = new();
        renderer = GetComponent<MeshRenderer>();
        renderer.GetPropertyBlock(mpb);
        mpb.SetColor(ColorID, color);
        renderer.SetPropertyBlock(mpb);
    }

    public Tween Shrink()
    {
        // Shrink to zero and return the tween so we can wait for it
        return transform.DOScale(Vector3.zero, shrinkDuration).SetEase(shrinkEaseType);
    }
    public void MoveDownWard()
    {
        Vector3 newLocation = new Vector3(transform.position.x, transform.position.y, transform.position.x - 1);
        transform.DOMoveZ(transform.position.z - 1,MoveDuration).SetEase(easeType);
    }
}
