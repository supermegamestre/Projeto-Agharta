using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SmallUtilities;
using System;

public class EnemyAI : MonoBehaviour
{
    /* Muito obrigado unity por me fazer definir cada segunda variavel que vai ser mostrada no inspetor em uma linha separada
    porque vocês estavam com preguiça de mais pra deixar o dev escolher se cada variavel definida em uma linha deveria ou não ter seu proprio 'header' >:( */
    //mostrar os indicativos:
    [Header("O que mostrar?")]
    [SerializeField]
    private bool showPositions;
    [SerializeField]
    private bool showSight;

    
    //opções de ataque e movimento:
    [Header("Opções de ataque")]
    //timers de aquisição de trava, de ataque e de reset de ataque.
    [SerializeField, Range(0.1f, 10f)]
    private float lockingTimer;    
    [SerializeField, Range(0.1f, 10f)]
    private float volleyLockingTimer, attackTimer, volleyAttackTimer, attackCooldown;
    //se ele é voador e quais os tipos de ataque que deve executar, e se deve executar rajadas.
    [SerializeField]
    private bool flying, ram, melee, laser, projectile, volley;
    //velocidade de investida, velocidade que te segue e alcance do laser.
    [SerializeField, Range(0.1f, 20f)]
    private float rammingSpeed, followSpeed, laserRange, laserLingerTime, meleeAttackDistance;
    //alcance e velocidade dos projéteis.
    [SerializeField, Range(0.1f, 40f)]
    private float projectileSpeed, projectileRange;
    //tamanho da pool.
    [SerializeField]
    private int poolSize;
    //prefab do projétil e do reticulo de mira.
    [SerializeField]
    private GameObject projectilePrefab, laserPrefab, aimReticlePrefab;
    //por onde os tiros sairão, a quantidade de objetos também é a quantidade de projéteis que serão lançados de vez.
    [SerializeField]
    private Transform[] muzzles;
    //velocidade que gira
    [SerializeField, Range(1f, 1000f)]
    private float rotatingSpeed;
    //distância (como porcetagens em cima do alcançe da visão) em que ele deve te seguir ou se afastar.
    [SerializeField, Range(0.01f, 1)]
    private float minFollowDistance, maxFollowDistance;
    //o que para o laser.
    [SerializeField]
    private LayerMask laserMask;
    //o dano individual de cada ataque.
    [SerializeField, Range(1,10)]
    private int rammingDamage = 1, laserDamage = 1, projectileDamage = 1, meleeDamage = 1, volleySize = 1;

    
    //movimento:
    [Header("Opções de patrulha e movimento")]
    //os limites da patrulha do inimigo.
    [SerializeField]
    private Transform center;
    //as posições de patrulha.
    [SerializeField]
    private Transform[] positions;
    //a velocidade de patrulha.
    [SerializeField, Range(0.1f, 10f)]
    private float speed;


    //visão:
    [Header("Opções da visão")]
    //diferentes mascaras que ele usa para enxergar o player, o terreno, e a si mesmo.
    [SerializeField]
    private LayerMask playerMask;
    [SerializeField]
    private LayerMask obstructionMask, enemyMask;
    //o alcance da visão e quanto tempo ele passa procurando.
    [SerializeField, Range(0.1f,20)]
    private float sight, lookTimer;
    //o angulo de visão dele enquanto procura e enquanto anda.
    [SerializeField, Range(45, 360)]
    private float lookingAngle, walkingAngle;

    
    //variaveis privadas relacionadas a maquina de estados.
    //patrulha:
    //qual das posições ele está patrulhando agora.
    private int positionIndex = 0;                                      
    
    //procura:
    //o timer parcial da patrulha.
    private Timer lookPartialTimer;                                 
    
    //agro:
    //a posição para a qual ele irá quando te ver e o ponto que fica exatamente entre vocês 2.
    private Vector2 targetPosition, midPoint;
    //bools que determinam: se o inimigo pode investir ou atacar corpo-a-corpo, se ele tem uma posição para seguir o player, se ele deve ignora-lo, se pode executar qualquer ataque, se já atacou (melee) ou se esta atacando (melee).
    private bool canRam = false, canMelee = false, lockedPosition = false, ignoreSight = false,  canAttack = true;
    //bool que determina se o inimigo ja conseguiu uma posição para ataca-lo, é uma lista por causa da configuração de rajada
    private bool[] lockAcquired, firedPosition;
    //o timer corrente do ataque e o timer entre travas.
    private Timer lockingPartialTimer, attackPartialTimer, attackCooldownTimer;
    private SteppedTimer lockingPartialSteppedTimer, attackPartialSteppedTimer;
    //a trava no player, precisa ser lida por outros scripts por isso é publica...
    private Vector2[] lockedPositions;
    //a pool de projetéis caso vá usar, precisa ser lida pelos proprios projéteis então é publica.
    public ObjectPool projectilePool;
    //para onde olha
    private int lookingIndex = 0;
    //hitbox do ataque melee
    private GameObject meleeHitbox;
    //o animador do ataque melee
    private Animator meleeAnimator;
    //o spriteRenderer do ataque melee
    private SpriteRenderer spriteRenderer;
    //o collider do ataque melee
    private PolygonCollider2D meleeHitboxCollider;
    //o filtro de contato do ataque melee
    private ContactFilter2D contactfilter;
    //o controle do animador do ataque melee
    private AnimatorController meleeController;
    //a fila de ataques melee, alem daquela que esta rodando no momento
    private Queue<IEnumerator> meleeCoroutines = new Queue<IEnumerator>();
    private IEnumerator currentMeleeAttack = null;

    //outras variaveis:
    //camera
    private GameObject camObj;
    private Camera cam;
    //reticulos e lasers desse inimigo na cena e suas pools
    private ObjectPool aimReticlePool, laserPool;
    private GameObject[] aimReticles, lasers;
    //os animadores dos reticulos
    private List<AnimatorController> aimAnimators = new List<AnimatorController>();
    //as animações dos reticulos
    private string[] reticleAnimations = new string[] { "aim start", "aim lock" };
    //o rigidbody do inimigo.
    private Rigidbody2D rb;
    //a rotação para a qual ele olhará o tempo todo.
    private Quaternion targetRotation;
    //variavel que armazena a ultima posição do inimigo (usada para calcular a orientação do mesmo) e a variavel que armazena o valor da orientação dele.
    private Vector2 lastPosition, facing;
    //a posição do player.
    private Transform player;
    //o controlador de vida do inimigo.
    private UniversalHealthController playerHealthController;

    //maquina de estado:
    //enumerador que contem as definições dos estados.
    private enum state
    {
        patrol,
        scan,
        locked,
        dead
    }
    //estado atual e estado anterior.
    private state currentState = state.patrol, previousState = state.patrol;
    //checa se a corrotina de resetar ataque está rodando
    private bool resetingAtacks = false;


    //funções que retornam variaveis:

    //para onde ele está virado.
    private Quaternion facingDirection(Vector2 direction) {return Quaternion.AngleAxis(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, Vector3.forward); }

    //a visão do inimigo.
    private bool search(LayerMask target, LayerMask terrain, Vector2 orientation,float angle,float sight)
    {
        if(ignoreSight)     //se não estiver cego...
            return false;

        Collider2D looking = Physics2D.OverlapCircle(transform.position, sight, target);
        if (looking == null)    //e ver o player...
            return false;

        Vector2 targetDirection = (looking.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, looking.transform.position);
        if (Vector2.Angle(targetDirection, orientation) < angle / 2)    //e não tiver nada entre você e ele...
            if (!Physics2D.Raycast(transform.position, targetDirection, distance, terrain)) 
                return true;    //então você viu ele.

        return false;   //do contrario, você não o viu.
    }

    //funções de estado:
    //patrulha.
    private void patrol()
    {

        if ((flying && Vector2.Distance(transform.position, positions[positionIndex].position) < 0.1f) || (Vector2.Distance(new Vector2(transform.position.x, positions[positionIndex].position.y), positions[positionIndex].position) < 0.1f && !flying))
        {
            positionIndex++;
            currentState = state.scan;
            previousState = state.patrol;
            if(positionIndex >= positions.Length)
                positionIndex = 0;
        }
        rb.velocity = Vector2.zero;
        
        if(flying)
            transform.position = Vector2.MoveTowards(transform.position, positions[positionIndex].position, speed * Time.deltaTime);
        else
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(positions[positionIndex].position.x, transform.position.y), speed * Time.deltaTime);

        targetRotation = facingDirection(facing);

        if (search(playerMask, obstructionMask, facing, walkingAngle, sight))
        {
            currentState = state.locked;
            previousState = state.patrol;
        }
    }
    
    //procura.
    private void scan()
    {

        if (!lookPartialTimer.isOver())
        {
            if(search(playerMask, obstructionMask, facing, lookingAngle, sight))
            {
                lookPartialTimer.reset();
                currentState = state.locked;
                previousState = state.scan;
            }
            lookPartialTimer.tick();
        }else 
        {
            lookPartialTimer.reset();
            if (previousState == state.patrol || previousState == state.scan)
                currentState = state.patrol;
            previousState = state.scan;
        }
    }

    //agro.
    private void locked()
    {

        if (search(playerMask, obstructionMask, facing, lookingAngle, sight))
        {
            //mantem uma certa distância do player se for puramente ranged
            if(!ram && (melee || projectile || laser))
            {
                midPoint = (transform.position + player.position) / 2;
                if (!lockedPosition)
                {
                    targetPosition = midPoint;
                    lockedPosition = true;
                }
            }
            if (!ram && !melee)
            {
                if (flying ? Vector2.Distance(transform.position, player.position) > sight * maxFollowDistance : MathF.Abs(transform.position.x - player.position.x) > sight * maxFollowDistance)
                    lockedPosition = false;
                else if (flying ? Vector2.Distance(targetPosition, player.position) < sight * minFollowDistance : MathF.Abs(transform.position.x - player.position.x) < sight * minFollowDistance)
                    targetPosition += (Vector2)(transform.position - player.position).normalized;
                if (!flying)
                    targetPosition.y = transform.position.y;
                transform.position = Vector2.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            }
            else if (melee)
            {
                if (flying ? Vector2.Distance(transform.position, player.position) >= meleeAttackDistance : MathF.Abs(transform.position.x - player.position.x) >= meleeAttackDistance)
                    lockedPosition = false;
                transform.position = flying ? Vector2.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime) : Vector2.Lerp(transform.position, new Vector2(targetPosition.x, transform.position.y), followSpeed * Time.deltaTime);
            }

            //olha para o player se não tiver trava, do contrario olha para a trava, tambem mostra o reticulo de mira.
            if (aimReticles == null)
            {
                aimReticles = aimReticlePool.getInBulkAsArray(volleySize, transform, false);
                for(int i = 0;i < aimReticles.Length; i++)
                    aimAnimators.Add(new AnimatorController(aimReticles[i].GetComponent<Animator>(), reticleAnimations));
            }
            if (volley)
            {
                
                lockingPartialSteppedTimer.autoTick();
                for (int i = 0; i < lockAcquired.Length; i++)
                {
                    if (lockingPartialSteppedTimer.isOverStepped()[i] && !lockAcquired[i])
                    {

                        lockedPositions[i] = player.position;
                        lockAcquired[i] = true;
                        
                    }
                }
                if (BoolListReaders.allFalse(lockAcquired))
                    targetRotation = facingDirection(player.position - transform.position);
                if(!BoolListReaders.allFalse(lockAcquired))
                    attackPartialSteppedTimer.autoTick();
                for (int i = 0; i < lockAcquired.Length;i++)
                {

                    if (lockAcquired[i])    //para cada trava
                    {
                        //mostre a trava
                        aimReticles[i].transform.position = (cam.WorldToScreenPoint(lockedPositions[i]));
                        aimReticles[i].SetActive(true);
                        aimAnimators[i].animate(1);
                    }
                    else
                    {
                        aimReticles[i].transform.position = (cam.WorldToScreenPoint(player.position));
                        aimReticles[i].SetActive(true);
                        aimAnimators[i].animate(0);
                    }
                    
                }
                if (lockAcquired[lookingIndex] && !firedPosition[lookingIndex])
                    targetRotation = facingDirection(lockedPositions[lookingIndex] - (Vector2)transform.position);
                else if (firedPosition[lookingIndex])
                {
                    lookingIndex++;
                    if(lookingIndex == firedPosition.Length)
                        lookingIndex = 0;
                }
                if (BoolListReaders.allTrue(firedPosition))
                    lookingIndex = 0;

                for (int i = 0; i < attackPartialSteppedTimer.isOverStepped().Count; i++)
                {
                    if (attackPartialSteppedTimer.isOverStepped()[i] && lockAcquired[i] && !firedPosition[i])
                    {
                        if (laser)
                        {
                            if (lasers == null)
                                lasers = laserPool.getInBulkIntoTransformsAsArray(muzzles);
                            StartCoroutine(Laser());
                        }
                        if (ram)
                            StartCoroutine(Ram(i));
                        if (projectile)
                            StartCoroutine(Projectile(i));
                        if (melee)
                            if(currentMeleeAttack == null)
                            {
                                currentMeleeAttack = Melee(i);
                                StartCoroutine(currentMeleeAttack);
                            }
                            else
                                meleeCoroutines.Enqueue(Melee(i));

                        firedPosition[i] = true;
                    }
                }
                if (BoolListReaders.allTrue(firedPosition))
                {
                    attackCooldownTimer.autoTick();
                    if (attackCooldownTimer.isOver())
                    {
                        StartCoroutine(resetAttack());
                        attackCooldownTimer.reset();
                    }

                }
            }
            else
            {
                lockingPartialTimer.autoTick();
                if (!lockAcquired[0] && lockingPartialTimer.isOver())
                {
                    lockedPositions[0] = player.position;
                    lockAcquired[0] = true;
                }
                
                if (lockAcquired[0])//reticulo mostrando a trava do inimigo
                {
                    attackPartialTimer.autoTick();  //aproveitando a logica pra ticar o timer de ataque

                    aimReticles[0].transform.position = (cam.WorldToScreenPoint(lockedPositions[0]));
                    aimAnimators[0].animate(1);
                    targetRotation = facingDirection(lockedPositions[0] - (Vector2)transform.position);
                }
                else//ou seguindo o player
                {
                    aimReticles[0].transform.position = (cam.WorldToScreenPoint(player.position));
                    aimReticles[0].SetActive(true);
                    aimAnimators[0].animate(0);
                    targetRotation = facingDirection(player.position - transform.position);
                }

                if(attackPartialTimer.isOver() && lockAcquired[0] && !firedPosition[0])
                {
                    if (laser)
                    {
                        if (lasers == null)
                            lasers = laserPool.getInBulkIntoTransformsAsArray(muzzles);
                        StartCoroutine(Laser());
                    }
                    if (ram)
                        StartCoroutine(Ram(0));
                    if (projectile)
                        StartCoroutine(Projectile(0));
                    if (melee)
                        StartCoroutine(Melee(0));
                    
                    firedPosition[0] = true;
                }
                else if (firedPosition[0])
                {
                    attackCooldownTimer.autoTick();
                    if (attackCooldownTimer.isOver())
                    {
                        StartCoroutine(resetAttack());
                        attackCooldownTimer.reset();
                    }
                }
            }
        }
        else
            canAttack = false;
        
        if(((!canRam && !canMelee) || (!melee && !canRam) || (!ram && !canMelee) || (!melee && !ram)) && !laser && !projectile)
            canAttack = false;

        if (!canAttack)
        {
            rb.velocity = Vector2.zero;
            StartCoroutine(resetAttack());
            currentState = previousState;
            canAttack = true;
        }

    }

    private IEnumerator resetAttack()
    {
        resetingAtacks = true;
        aimReticlePool.releaseAllIn(aimReticles);
        Timer.resetAllIn(new TimerCore[] { lockingPartialTimer, attackPartialTimer, lockingPartialSteppedTimer, attackPartialSteppedTimer });
        Array.Clear(lockAcquired, 0, lockAcquired.Length);
        Array.Clear(lockedPositions, 0, lockedPositions.Length);
        Array.Clear(firedPosition, 0, firedPosition.Length);
        yield return null;
        resetingAtacks = false;
    }

    //funções dos ataques, separei pra ficar mais facil de organizar
    private IEnumerator Ram(int position)
    {
        canRam = Physics2D.OverlapBox(center.position, center.lossyScale, 0f, playerMask);
        bool reached = false;
        bool notReseted = false;
        while (canRam && !reached && !notReseted)
        {
            canRam = Physics2D.OverlapBox(center.position, center.lossyScale, 0f, playerMask);

            reached = flying ? Vector2.Distance(transform.position, lockedPositions[position]) <= 0.1f : MathF.Abs(transform.position.x - lockedPositions[position].x) <= 0.1f;

            notReseted = lockedPositions[position] != Vector2.zero ? false : true;

            transform.position = flying ? Vector2.Lerp(transform.position, lockedPositions[position], rammingSpeed * Time.deltaTime) : Vector2.Lerp(transform.position, new Vector2(lockedPositions[position].x, transform.position.y), rammingSpeed * Time.deltaTime);

            yield return null;
        }
        
    }

    private IEnumerator Melee(int position)
    {
        canMelee = Physics2D.OverlapBox(center.position, center.lossyScale, 0f, playerMask);
        bool notReseted = false;

        if (canMelee && !notReseted)
        {
            canMelee = Physics2D.OverlapBox(center.position, center.lossyScale, 0f, playerMask);


            notReseted = lockedPositions[position] != Vector2.zero ? false : true;

            List<Collider2D> results = new List<Collider2D> ();
            meleeController.animate();
            if (Physics2D.OverlapCollider(meleeHitboxCollider, contactfilter, results) > 0)
                playerHealthController.gotHit(meleeDamage);
        }
        currentMeleeAttack = null;
        if(meleeCoroutines.Count > 0)
        {
            currentMeleeAttack = meleeCoroutines.Dequeue();
            StartCoroutine(currentMeleeAttack);
        }
        yield return null;
    }

    private IEnumerator Laser()
    {
        var lingering = new Timer(laserLingerTime);
        while (!lingering.isOver())
        {
            lingering.autoTick();
            for(int i = 0;i < lasers.Length; i++)
            {
                var lineRenderer = lasers[i].GetComponent<LineRenderer>();
                RaycastHit2D hit = Physics2D.Raycast(muzzles[i].position, transform.right * laserRange, laserRange, laserMask);
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, muzzles[i].position);
                if (hit && hit.collider.gameObject.tag == "Player")
                    playerHealthController.gotHit(laserDamage);
                lineRenderer.SetPosition(1,hit ? hit.point : muzzles[i].position + transform.right * laserRange);
            }
            yield return null;
        }
        foreach (GameObject laser in lasers)
            laser.GetComponent<LineRenderer>().enabled = false;
        yield return null;
    }

    private IEnumerator Projectile(int position)
    {
        foreach (Transform muzzle in muzzles)
        {
            GameObject projectile = projectilePool.get(muzzle);
            Projectile script = projectile.GetComponent<Projectile>();
            script.target = lockedPositions[position];
            script.parent = this;
            script.speed = projectileSpeed;
            script.range = projectileRange;
            script.damage = projectileDamage;
        }
        yield return null;
    }

    public void endOfLife() => StartCoroutine(endOfLifeRoutine());

    private IEnumerator endOfLifeRoutine()
    {
        Debug.Log("dead");
        currentState = state.dead;
        StartCoroutine(resetAttack());
        yield return null;
        if(!resetingAtacks)
            gameObject.SetActive(false);
    }

    //funções padrão:
    private void Awake()
    {
        if (positions == null)
            throw new ArgumentNullException("fill the positions list you idiot" + nameof(positions));
        lookPartialTimer = new Timer(lookTimer);
        attackCooldownTimer = new Timer(attackCooldown);
        laserLingerTime = Mathf.Clamp(laserLingerTime, 0f, attackCooldown);
        if (volley)
        {
            var finalSteppedLockingTimer = new List<float>() { lockingTimer };
            for(int i = 1; i < volleySize; i++)
                finalSteppedLockingTimer.Add(lockingTimer + (i * volleyLockingTimer));
            lockingPartialSteppedTimer = new SteppedTimer(finalSteppedLockingTimer);
            
            var finalSteppedAttackTimer = new List<float>() { attackTimer };
            for(int i = 1;i < volleySize; i++)
                finalSteppedAttackTimer.Add(attackTimer + (i * volleyAttackTimer));
            attackPartialSteppedTimer = new SteppedTimer(finalSteppedAttackTimer);

            lockAcquired = new bool[volleySize];
            lockedPositions = new Vector2[volleySize];
            firedPosition = new bool[volleySize];
        }
        else
        {
            lockingPartialTimer = new Timer(lockingTimer);
            attackPartialTimer = new Timer(attackTimer);
            lockAcquired = new bool[] { false };
            firedPosition = new bool[] { false };
            lockedPositions = new Vector2[1];
        }
        aimReticlePool = new ObjectPool(aimReticlePrefab, volley ? volleySize : 1, GameObject.FindGameObjectWithTag("Enemy Aim Reticle Pool").transform);   //inicializa a pool de mira de acordo com quantos ataques vai executar
        camObj = GameObject.FindGameObjectWithTag("MainCamera");                                                        //rastreia o objeto da camera
        cam = camObj.GetComponent<Camera>();                                                                            //'extrai' a camera do objeto 
        rb = GetComponent<Rigidbody2D>();                                                                               //registra o rigidbody.
        if (laser)
            laserPool = new ObjectPool(laserPrefab, muzzles.Length, transform);                                         //inicializa a pool de lasers se for usar
        player = GameObject.FindGameObjectWithTag("Player").transform;                                                  //procura e registra o player.
        playerHealthController = GameObject.FindGameObjectWithTag("Player").GetComponent<UniversalHealthController>();  //procura e registra o controlador de vida do player.
        lastPosition = transform.position;                                                                              //registra a propria ultima posição (provavelmente é redundante, mas nunca se sabe).
        if(projectile)
            projectilePool = new ObjectPool(projectilePrefab, poolSize);                                                          //inicia a pool de projéteis caso va usa-los
        if (melee)
        {
            meleeHitbox = GetComponentInChildren<PolygonCollider2D>().gameObject;
            spriteRenderer = meleeHitbox.GetComponent<SpriteRenderer>();
            meleeHitboxCollider = meleeHitbox.GetComponent<PolygonCollider2D>();
            meleeAnimator = meleeHitbox.GetComponent<Animator>();
            meleeController = new AnimatorController(meleeHitbox.GetComponent<Animator>(),"melee hit effect");
        }
        contactfilter = new ContactFilter2D();
        contactfilter.layerMask = playerMask;
        contactfilter.useLayerMask = true;
        canRam = ram;
        canMelee = melee;
    }


    private void FixedUpdate()
    {
        lastPosition = transform.position;  //continua registrando a ultima posição.
        //maquina de estado.
        switch (currentState)
        {
            case state.patrol: patrol(); break;
            case state.scan: scan(); break;
            case state.locked: locked(); break;
        }
        //garante que o inimigo ficará dentro de seus limites para evitar bugs (não é infalivel).
        if(!Physics2D.OverlapBox(center.position, center.lossyScale, 0f, enemyMask))
        {
            rb.velocity = Vector2.zero;
            ignoreSight = true;
        }
        else
            ignoreSight = false;
        //gira o inimigo para onde ele deve olhar.
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotatingSpeed * Time.deltaTime);
        //registra a direção que ele esta olhando.
        facing = (Vector2)transform.position - lastPosition;
    }

    private void OnDrawGizmos()
    {
        //mostra as posições de patrulha.
        if (showPositions)
        {
            Gizmos.color = Color.yellow;
            foreach(Transform position in positions)
            {
                Gizmos.DrawCube(position.position, Vector2.one/2);
            }
        }
        //mostra o alcance de visão.
        if(showSight)
        {
            Gizmos.color= Color.blue;
            Gizmos.DrawSphere(transform.position, sight);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //dano de colisão.
        if(collision.gameObject.tag == "Player")
            playerHealthController.gotHit(rammingDamage);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
            playerHealthController.gotHit(rammingDamage);
    }
}
