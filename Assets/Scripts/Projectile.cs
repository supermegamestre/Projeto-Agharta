using UnityEngine;
using SmallUtilities;

public class Projectile : MonoBehaviour
{
    [HideInInspector]
    public EnemyAI parent;
    [HideInInspector]
    public Vector2 target;
    [HideInInspector]
    public float speed, range;
    [HideInInspector]
    public int damage;
    private Timer rangeTimer;
    private Vector2 targetDirection;
    private bool calculated;

    private void Update()
    {
        if(range != 0 && rangeTimer == null)
            rangeTimer = new Timer(range);
        if (!calculated)
        {
            targetDirection = -((Vector2)transform.position - target).normalized;
            calculated = true;
        }
         
        
        rangeTimer.tick();
        if(rangeTimer.isOver())
        {
            calculated = false;
            rangeTimer.reset();
            speed = 0;
            range = 0;
            damage = 0;
            target = Vector2.zero;
            targetDirection = Vector2.zero;
            parent.projectilePool.release(this.gameObject);
        }
    }

    private void FixedUpdate()
    {
        transform.position = (Vector2)transform.position + targetDirection * speed * Time.deltaTime;
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
            collision.gameObject.GetComponent<UniversalHealthController>().gotHit(damage);
        calculated = false;
        rangeTimer.reset();
        speed = 0;
        range = 0;
        damage = 0;
        target = Vector2.zero;
        targetDirection = Vector2.zero;
        parent.projectilePool.release(this.gameObject);
    }
}
