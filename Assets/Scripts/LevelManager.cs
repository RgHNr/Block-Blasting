using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    [SerializeField]int level;
    string path ;
    LevelData levelData;
    [SerializeField] GameObject BlockPrefab , CanonPrefab;

    public static LevelManager Instance { get; private set; }

    public int shotsCount;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        path = $"Assets/Level_0{level}.asset";
        levelData = (LevelData)AssetDatabase.LoadAssetAtPath(path, typeof(LevelData));
        if (levelData)
            Debug.Log(levelData.name + " layers: " + levelData.layerCount + " list size: " + levelData.layers[0].gridColors.Length);
        else
            Debug.Log("Failed to load level");

        InitLevel(levelData);
        TutorialManager.Instance.Init(levelData);
        UIManager.Instance.UpdateLevelText(level);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadNextLevel(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void OnBlockShot()
    {
        shotsCount++;
        UIManager.Instance.UpdateProgressBar(shotsCount, 10 * levelData.rowCount * levelData.layerCount);
        if (shotsCount == 10*levelData.rowCount * levelData.layerCount)
        {
            UIManager.Instance.ShowWinPanel();
            UIManager.Instance.ContinueButton.onClick.RemoveAllListeners();
            UIManager.Instance.ContinueButton.onClick.AddListener(() =>LoadNextLevel(level));
        }
    }

    void InitLevel(LevelData data)
    {
        int rows = data.rowCount;
        int cols = 10;
        float x = 0;
        float y = 0;
        GridManager.Instance.layer = levelData.layerCount;
        //List<Block>[] columns = new List<Block>[10];
        for(int c=0; c < cols; c++)
        {
            GridManager.Grid[c] = new();
            for(int r=0; r<rows; r++)
            {

                for(int layer=0; layer < levelData.layerCount; layer++)
                {

                    //GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject block = Instantiate(BlockPrefab);
                    if(data.layerCount==3)block.transform.position = new Vector3(x, 2-layer, y);
                    if(data.layerCount==2) block.transform.position = new Vector3(x, 1 - layer, y);
                    if(data.layerCount==1) block.transform.position = new Vector3(x, 0 - layer, y);
                    Block b=block.GetComponent<Block>();
                    //block.GetComponent<MeshRenderer>().material.color = data.layers[0].gridColors[r*cols+c];
                    b.UpdateColor(data.layers[0].gridColors[r * cols + c]);
                    b.Column = c;
                
                
                    GridManager.Grid[c].Add(b);
                }
                //Instantiate(block);
                y++;
            }
            y = 0;
            x++;
        }

        InventoryManager.Instance.Init(data);

    }
}
