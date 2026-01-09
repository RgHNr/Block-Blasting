using UnityEngine;

public class TouchManager : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player"; // Set this to your specific tag

    void Update()
    {
        // Handle both Mouse (Editor) and Touch (Mobile)
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(Input.mousePosition);
        }
    }

    private void HandleInput(Vector3 screenPosition)
    {
        // 1. Create a ray from the camera to the touch point
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        // 2. Cast the ray
        if (Physics.Raycast(ray, out hit))
        {
            // 3. Check if the hit object has the specific tag
            if (hit.collider.CompareTag(targetTag))
            {
                //Debug.Log("Hit a valid object: " + hit.collider.name);

                // Do something with the object (e.g., call a function on the Canon)
                if (hit.collider.TryGetComponent<Canon>(out Canon canon))
                {
                    // Move it to slot logic here...
                    if (canon != InventoryManager.Instance.inventoryColumns[canon.ColumnIndex][0]) return;
                    //if (UIManager.Instance.TutorialPanel != null && UIManager.Instance.TutorialPanel.activeInHierarchy) UIManager.Instance.TutorialPanel.SetActive(false);
                    TutorialManager.Instance.OnActionPerformed(TutorialCondition.SingleClick);
                    InventoryManager.Instance.OnCannonClicked(canon, canon.ColumnIndex);
                    AudioManager.Instance.PlayBurst();
                }
            }
        }
    }
}
