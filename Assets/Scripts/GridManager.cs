using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting;

public class GridManager : MonoBehaviour
{
    public static List<Block>[] Grid = new List<Block>[10];

    public static GridManager Instance { get; private set; }

    private int[] columnDestructionCounters = new int[10];

    public int layer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        // Initialize lists in the array to prevent NullReferenceExceptions
        for (int i = 0; i < Grid.Length; i++)
        {
            if (Grid[i] == null) Grid[i] = new List<Block>();
        }
    }



    public void TriggerColumnMove(int index)
    {
        StartCoroutine(MoveColumnCoroutine(index));
    }

    private IEnumerator MoveColumnCoroutine(int index)
    {
        if (Grid[index] == null || Grid[index].Count == 0) yield break;

        // 1. Identify the block to remove
        Block blockToRemove = Grid[index][0];
        Grid[index].RemoveAt(0);

        // 2. Shrink the block and wait for the animation to finish
        // We yield return the Tween itself
        yield return blockToRemove.Shrink().WaitForCompletion();
        

        // 3. Destroy the block object
        Destroy(blockToRemove.gameObject);

        // 4. Move the rest of the blocks downward
        columnDestructionCounters[index]++;

        // 4. Only move the rest of the column down once the full stack (3) is gone
        if (columnDestructionCounters[index] >= layer)
        {
            columnDestructionCounters[index] = 0; // Reset counter

            foreach (var block in Grid[index])
            {
                if (block != null) block.MoveDownWard();
            }
        }

        // Optional: If you want to wait for the downward movement to finish:
        // yield return new WaitForSeconds(MoveDuration);
    }

    //public void MoveColumn(int index)
    //{
    //    if (Grid[index].Count == 0) return;
    //    Block lastElement = Grid[index][0];
    //    Grid[index].RemoveAt(0);
    //    Destroy(lastElement.gameObject);
    //    foreach(var block in Grid[index])
    //    {
    //        block.MoveDownWard();
    //    }
    //}
}
