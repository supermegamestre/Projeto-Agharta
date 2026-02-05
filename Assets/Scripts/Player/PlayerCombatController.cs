using SmallUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    private PlayerMovementController movementController;

    [SerializeField]
    private List<PolygonCollider2D> referenceColliders;
    [SerializeField]
    private ContactFilter2D filter;
    [SerializeField, Range(1,10)]
    private int attackDamage = 1, strengthenedAttackDamage = 4;
    [SerializeField, Range(0.1f, 10)]
    private float attackDelay = 0.5f, hitTiming = 2f, comboCooldown = 2f;

    //timers, contadores e bools
    private Timer attackTimer, hitTimer, comboCooldownTimer;
    private int inputs = 0, hits = 0, hitTracker = 0;
    private bool comboOnCooldown = false, hit = false;

    //detecção de hit
    private PolygonCollider2D hitbox;
    private HitboxController hitboxController;
    private List<Collider2D> results = new List<Collider2D>();

    //animação
    private Animator animator;
    private AnimatorController animatorController;
    private Queue<int> animationQueue = new Queue<int>();
    private string[] attacks = { "attack 01", "attack 02", "attack 03" };

    public bool restrained;

    //maquina de estado (sim é necessário)
    private enum state
    {
        notAttacking,
        attackingStill,
        attackingMoving,
        attackingAirborne
    }
    private state currentState = state.notAttacking;


    public void changeDirection(int direction) => GameObject.FindGameObjectWithTag("Player Attack Hitbox Anchor").GetComponent<Transform>().transform.rotation = direction == 1 ? new Quaternion(0, 0, 0, 1): new Quaternion(0, 180, 0, 1);
    public void setHitbox(int index) => hitboxController.setHitbox(index);
    public void clearHitbox() => hitboxController.clearHitbox();


    private void Awake()
    {
        movementController = GetComponentInParent<PlayerMovementController>();
        attackTimer = new Timer(attackDelay);
        hitTimer = new Timer(hitTiming);
        comboCooldownTimer = new Timer(comboCooldown);
        hitbox = GetComponent<PolygonCollider2D>();
        animator = GetComponent<Animator>();
        hitboxController = new HitboxController(hitbox, referenceColliders);
        animatorController = new AnimatorController(animator, attacks, "dummy animation");
    }

    private void inputControl()
    {
        if(Input.GetButtonDown("Fire 2") && inputs < 3 && ((movementController.stillGrounded && currentState == state.notAttacking) || currentState == state.attackingStill))
        {
            currentState = state.attackingStill;
            animationQueue.Enqueue(inputs);
            attackTimer.reset();
            inputs++;
        }
        else if(Input.GetButtonDown("Fire 2") && !movementController.stillGrounded && animationQueue.Count == 0 && currentState == state.notAttacking)
        {
            currentState = state.attackingMoving;
            animationQueue.Enqueue(1);
            inputs = 0;
        }
        else if(Input.GetButtonDown("Fire 2") && movementController.airborne && animationQueue.Count == 0 && currentState == state.notAttacking)
        {
            currentState = state.attackingAirborne;
            animationQueue.Enqueue(1);
            inputs = 0;
        }
        if(currentState == state.attackingStill)
        {
            attackTimer.autoTick();
            if (attackTimer.isOver() || (inputs == 3 && animatorController.getCurrentState() == 2 && animatorController.isOnTheDummy()))
            {
                inputs = 0;
                currentState = state.notAttacking;
            }
        }
        if((currentState == state.attackingMoving || currentState == state.attackingAirborne) && animationQueue.Count == 0)
            currentState = state.notAttacking;

        hitTimer.autoTick();
        if (hitTimer.isOver())
            hits = 0;

        if (comboOnCooldown)
        {
            comboCooldownTimer.autoTick();
            if(comboCooldownTimer.isOver())
                comboOnCooldown = false;
        }

        if(hits == 3)
        {
            comboCooldownTimer.reset();
            comboOnCooldown = true;
        }

        if (animationQueue.Count > 0 && animatorController.isOnTheDummy())
            animatorController.animate(animationQueue.Dequeue(), true);

        if (animatorController.isOnTheDummy())
            hit = false;

        if(movementController.dashing)
        {
            animationQueue.Clear();
            inputs = 0;
            animatorController.animate("dummy animation");
        }

        restrained = currentState == state.attackingStill;
        
    }

    private void attack()
    {
        if (!animatorController.isOnTheDummy() && Physics2D.OverlapCollider(hitbox, filter, results) > 0 && !hit)
        {
            foreach(var hit in results)
                if(hit.gameObject.GetComponent<UniversalHealthController>() != null)
                    hit.gameObject.GetComponent<UniversalHealthController>().gotHit(attackDamage);
            hit = true;
        }
        if(hit && !comboOnCooldown)
            hits++;
        if (hitTracker != hits)
        {
            hitTimer.reset();
            hitTracker = hits;
        }
    }

    

    private void Update() => inputControl();

    private void FixedUpdate() => attack();
}
