using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SlotManager : MonoBehaviour
{
    public static SlotManager Instance;
    public Transform[] slotPositions; // The 3-5 empty squares in the UI
    public Canon[] occupiedSlots;     // Tracks which cannon is in which slot
    public Ease EaseType;
    public float CanonMovingTime;
    private void Awake()
    {
        Instance = this;
        // Initialize based on the number of slots for this level
        occupiedSlots = new Canon[slotPositions.Length];
    }

    public int GetFirstEmptySlot()
    {
        for (int i = 0; i < occupiedSlots.Length; i++)
        {
            if (occupiedSlots[i] == null) return i;
        }
        return -1; // All slots full
    }

    public void AssignCannonToSlot(Canon cannon, int index)
    {
        occupiedSlots[index] = cannon;
        // Smoothly animate the cannon into the slot position
        cannon.transform.DOMove(slotPositions[index].position, CanonMovingTime).OnUpdate(() =>
        {

            cannon.UpdateConnectorPosition();
        }).SetEase(EaseType).OnComplete(() =>
        {
            cannon.ToggleOutline(false);
            cannon.enabled = true;
            var checkSameCanons = CheckSameColor();
            if (checkSameCanons.Item1)
            {
                FuseCanon(GetSameCanon(checkSameCanons.Item2));
            }
        });
    }

    public int GetAdjacentEmptySlots()
    {
        // Loop through slots up to the second-to-last one
        for (int i = 0; i < occupiedSlots.Length - 1; i++)
        {
            // Check if THIS slot and the NEXT slot are both null
            if (occupiedSlots[i] == null && occupiedSlots[i + 1] == null)
            {
                return i; // Found a valid pair starting at 'i'
            }
        }
        return -1; // No adjacent slots found
    }

    //This method checks if 3 canons are the same colors
    public (bool,Color) CheckSameColor()
    {
        
        Dictionary<Color, int> colors = new();
        List<Canon> canons = new();
        foreach(var canon in occupiedSlots)
        {
            if (canon == null || !canon.enabled) continue;
            if (!colors.ContainsKey(canon.color))
            {
                colors.Add(canon.color, 0);
            }
            
            colors[canon.color]++;

            if (colors[canon.color] == 3) return (true,canon.color);
        }
        return (false,Color.clear);
    }
    List<Canon> GetSameCanon(Color color)
    {

        return occupiedSlots.Where((a) => a !=null && a.enabled && a.color == color).ToList();
    }

    public void FuseCanon(List<Canon> canons)
    {
        Canon middle = canons[1];
        canons[0]?.transform.DOMove(middle.transform.position, 0.5f);
        canons[2]?.transform.DOMove(middle.transform.position, 0.5f).OnComplete(() => {


            middle.UpdateCountAndText(canons[0].GetCurrentCount() + canons[2].GetCurrentCount());
            GetCanonSlotIndexAndRemove(canons[0]);
            GetCanonSlotIndexAndRemove(canons[2]);

            Destroy(canons[0].gameObject);
            Destroy(canons[2].gameObject);

        });

    }

    public int GetCanonSlotIndexAndRemove(Canon canonToSearch)
    {
        for( int i=0;i<occupiedSlots.Length;i++)
        {
            if (occupiedSlots[i] == canonToSearch) {
                occupiedSlots[i] = null;
                return i; 
            
            }
        }
        return -1;
    }
}