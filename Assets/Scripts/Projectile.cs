using UnityEngine;
using DG.Tweening;

public class Projectile : MonoBehaviour
{
    public Transform target;
    [SerializeField] float speed;
    public Block targetBlock;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

        if (target == null)
        {
            Destroy(gameObject); // Or fall down
            return;
        }

        // Move towards the target's current position every frame
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Rotate to look at target (Optional: makes it look better)
        transform.LookAt(target);

        // Check if we arrived
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            OnReachedTarget();
        }
    }

    void OnReachedTarget()
    {
        // Prevent double-triggering
        if (targetBlock != null)
        {
            GridManager.Instance.TriggerColumnMove(targetBlock.Column);
            targetBlock = null; // Mark as handled
        }
        
        Destroy(gameObject);
    }
    //public void MoveToTarget()
    //{
    //    float distance = Vector3.Distance(transform.position, target.position);
    //    transform.DOMove(target.position, distance / speed).SetEase(Ease.Linear).SetTarget(target).OnUpdate(() => { transform.DOMove(target.position,0.1f); }).OnComplete(() => {
    //        GridManager.Instance.TriggerColumnMove(targetBlock.Column);
    //        Destroy(this.gameObject);
    //    }); 
    //}

    private void OnCollisionEnter(Collision collision)
    {
        Block block = collision.collider.GetComponent<Block>();
        if (block !=null)
        {
            
        }
    }
}
