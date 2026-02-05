using System.Collections.Generic;
using UnityEngine;
using SmallUtilities;
using UnityEngine.Timeline;

public class PlayerMovementController : MonoBehaviour
{
    private Rigidbody2D rb;             //rigidbody do char

    private PlayerCombatController combatController;    //o controlador de combate

    [SerializeField, Header("Ativar/desativar habilidades de movimento.")]
    private bool DashOn = false;
    [SerializeField]
    private bool WallJumpOn = false, WallClimbOn = false, DoubleJumpOn = false, LedgeGrabOn = true;   //pra ativar as opções de movimento do char

    //sensores
    [SerializeField, Header("Opções dos sensores.")]
    private LayerMask player;                    //layermask do player
    [SerializeField]
    private LayerMask ground;                    //do chão
    [SerializeField]
    private LayerMask platform;                  //plataformas
    [SerializeField]
    private LayerMask obstruction;               //paredes e obstruções
    private ContactFilter2D platformFilter;
    [Space, SerializeField]
    private bool showSensors = true;
    [SerializeField, Range(0.01f, 1f)]
    private float GroundSensorSight;            //alcance da visão do sensor que lê o chão
    [SerializeField, Range(0.01f, 1f)]
    private float GroundSensorWidth;            //largura do sensor que lê o chão
    [SerializeField, Range(-2f, 2f)]
    private float GroundSensorPosition;         //posição do sensor que lê o chão
    [Space]
    [SerializeField, Range(0.01f, 1f)]
    private float WallSensorSight;              //alcance da visão do sensor que lê as paredes
    [SerializeField, Range(0.1f, 3f)]
    private float WallSensorHeight;             //altura do sensor que lê as paredes
    [SerializeField, Range(0.1f, 3f)]
    private float WallSensorDistance;           //posicão do sensor que lê as paredes
    [Space]
    [SerializeField, Range(0.1f, 5f)]
    private float ClippingSensorWidth;          //largura do sensor de intangibilidade
    [SerializeField, Range(0.1f, 5f)]
    private float ClippingSensorHeight;         //altura do sensor de intangibilidade
    [Space]
    [SerializeField, Range(0.1f, 2f)]
    private float PlatformDescendSensorWidth;
    [SerializeField, Range(0.1f, 2f)]
    private float PlatformDescendSensorHeight;
    [SerializeField, Range(-2f, 2f)]
    private float PlatformDescendSensorPosition;
    [Space]
    [SerializeField, Range(0.1f, 2f)]
    private float LedgeGrabSensorWidth;         //largura do sensor de ledge grab
    [SerializeField, Range(0.1f, 3f)]
    private float LedgeGrabSensorHeight;        //altura do sensor de ledge grab
    [SerializeField, Range(0f, 5f)]
    private float LedgeGrabSensorPosition;      //posição do sensor de ledge grab
    [Space]
    private bool grounded, leftwalled, rightwalled, wallwaiting, clipping, ledged;   //os bools que os sensores ativam
    private int platformed, descending;

    //opções de movimento

    [SerializeField, Range(0.1f, 100f), Header("Personalização de movimento.")]
    private float Speed = 5;                    //velocidade de caminhada
    [SerializeField, Range(0.01f, 1f)]
    private float Acceleration = 0.1f;          //aceleração
    [SerializeField]
    private bool smoothWalk = true;
    [Space]
    [SerializeField, Range(0.1f, 1000f)]
    private float JumpForce = 100;              //força do pulo
    [SerializeField, Range(0.1f, 10f)]
    private float Gravity = 5f;                 //gravidade padrão
    [SerializeField, Range(1f, 10f)]
    private float GravityMultiplier = 2f;       //multiplicador de gravidade
    [SerializeField]
    private float MaxFallSpeed = 20f;           //velocidade de queda maxima
    [SerializeField, Range(0.1f, 3f)]
    private float ControllableJumpTime = 1f;    //timer do pulo controlável
    [SerializeField, Range(0.01f, 1f)]
    private float JumpBuffer = 0.25f;           //tempo do coyote jump
    [Space]
    [SerializeField, Range(1.5f, 100f)]
    private float DashSpeed = 5f;               //velocidade do dash
    [SerializeField, Range(0.1f, 1f)]
    private float DashDuration = 0.1f;          //duração do dash
    [SerializeField, Range(0f, 1f)]
    private float DashCooldown = 0f;            //cooldown do dash
    [Space]
    [SerializeField, Range(0.1f, 20f)]
    private float WallSlideTime = 1f;           //quanto tempo pode se segurar na parede
    [SerializeField, Range(0.01f, 2f)]
    private float WallJumpTime = 1f;            //quanto tempo dura o wall jump
    [SerializeField, Range(0.1f, 1000f)]
    private float WallJumpXForce = 100f;        //a força do wall jump
    [SerializeField, Range(0.1f, 1000f)]
    private float WallJumpYForce = 100f;        //a força do wall jump
    [SerializeField, Range(0f, 50f)]
    private float WallSlideSpeed = 1f;          //velocidade em que ele deslisará pela parede
    [SerializeField, Range(0f, 10f)]
    private float WallSlideGravity = 0.5f;      //gravidade durante wallslide, visto que mathf.clamp na velocidade vertical não é o bastante
    [SerializeField, Range(0f, 100f)]
    private float WallClimbSpeed = 1f;          //velocidade em que ele escala paredes
    [SerializeField, Range(0f, 2f)]
    private float WallClimbCooldown = 0.5f;  //cooldown da escalada

    private float horizontalAxis, previousHorizontalAxis, verticalAxis, previousVerticalAxis;   //variaveis que armazenam inputs direcionais (esquerda, direita, cima, baixo)

    private Timer controllableJumpTimer, jumpBufferTimer, dashTimer, dashCooldownTimer, wallSlideTimer, wallJumpTimer, wallClimbCooldownTimer;   //timers

    private float normalSpeed, directionOfMovement, jumpCounter = 0, appliedMaxFallSpeed, wallRotation = 0;    //contador de pulos, velocidade padrão, velocidade de queda máxima em uso, e orientação do wall jump

    private bool restrained, onJump, goingDown, holdingLedge, canAirDash , onDash, enterDash, exitDash, enterWall, onWall, exitWall, onWallJump, onWallDash, wallClimbing;    //os varios bools que controlam o estado do movimento do char

    [HideInInspector]
    public bool stillGrounded, dashing, airborne;  //os que precisam ser publicos por causa do combatcontroler...

    private Vector2 speedRef = Vector2.zero;    //vetor que existe apenas para satisfazer o unity
    private List<Collider2D> resultsA = new List<Collider2D>(), resultsB = new List<Collider2D>(), targetPlatforms = new List<Collider2D>();
    
    //acorda!
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();       //o rigidbody do player
        combatController = GetComponentInChildren<PlayerCombatController>();
        controllableJumpTimer = new Timer(ControllableJumpTime);
        jumpBufferTimer = new Timer(JumpBuffer);
        dashTimer = new Timer(DashDuration);
        dashCooldownTimer = new Timer(DashCooldown);
        wallSlideTimer = new Timer(WallSlideTime);
        wallJumpTimer = new Timer(WallJumpTime);
        wallClimbCooldownTimer = new Timer(WallClimbCooldown);
    }


    //funções

    private void inputControl()    //lê o que você quer fazer
    {
        //observando?
        restrained = Input.GetButton("Fire 5") || combatController.restrained;
        //andar?
        horizontalAxis = Input.GetAxisRaw("Horizontal");
        verticalAxis = Input.GetAxisRaw("Vertical");
        if (onDash)
        {
            
            if (Input.GetAxisRaw("Horizontal") == 0)    //usa o ultimo comando de caminhada para dar dash parado
            {
                if (previousHorizontalAxis != 0)
                    directionOfMovement = previousHorizontalAxis;
                else
                    directionOfMovement = 1;
            }
            else if (onWallDash)
                directionOfMovement = wallRotation;  //caso dê o dash no muro, dará o dash na direção oposta ao muro
            else
                directionOfMovement = horizontalAxis;    //lê comandos de caminhada

        }
        else
            directionOfMovement = horizontalAxis;    //lê comandos de caminhada
        if(directionOfMovement == 1)
            combatController.changeDirection(1);
        else if(directionOfMovement == -1)
            combatController.changeDirection(-1);

        //se segurar na borda de uma plataforma?
        if (LedgeGrabOn && ledged && !grounded && !onWallJump && !onWall && verticalAxis > 0)  //condições para se segurar nas plataformas
            holdingLedge = true;
        else
            holdingLedge = false;

        //ou descer de uma?
        if (platformed > 0 && Input.GetButtonDown("Jump") && verticalAxis < 0 && !goingDown)
        {
            foreach(var platform in resultsA)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), platform, true);
                targetPlatforms.Add(platform);
            }
            goingDown = true;
            
        }
        
        


        //dash?
        if (DashOn) //se o dash tiver ligado e não estiver atacando
        {
            dashCooldownTimer.autoTick();
            if (dashCooldownTimer.isOver())    //e se não estiver em cooldown tambem...
            {
                if (Input.GetButtonDown("Fire 1"))
                {
                    if (grounded)   //dash no chão?
                    {
                        onDash = true;
                        enterDash = true;
                    }
                    else if (onWall)    //no muro?
                    {
                        enterDash = true;
                        onDash = true;
                        onWallDash = true;
                    }
                    else if (canAirDash)    //ou no ar?
                    {
                        onDash = true;
                        enterDash = true;
                        canAirDash = false;
                    }
                }
            }
            if (onDash) //quando parar?
            {
                dashTimer.autoTick();
                if (Input.GetButtonUp("Fire 1"))    //quando você quiser!
                {
                    onDash = false;
                    onWallDash = false;
                    exitDash = true;
                }
                if (dashTimer.isOver())    //ou quando o tempo acabar...
                {
                    onDash = false;
                    onWallDash = false;
                    exitDash = true;
                }
            }
        }
        if (Input.GetAxisRaw("Horizontal") != 0)
            previousHorizontalAxis = Input.GetAxisRaw("Horizontal"); //armazena o ultimo comando de caminhada para direcionar o dash caso você esteja parado

        //pular?
        if (onWall && wallwaiting) //evita que você use o pulo duplo enquanto está em um muro
            onJump = false;
        else if(!combatController.restrained)
        {
            //estabelece o jumpbuffer, que permite que o jogador pule mesmo que pressione o botão de pular um pouco antes de tocar o chão, é aqui que ele detecta o comando de pular do player
            if (Input.GetButtonDown("Jump") && verticalAxis >= 0)
                jumpBufferTimer.reset();
            else
                jumpBufferTimer.autoTick();
            //se o jumpbuffer der sinal verde e o player estiver no chão, ou tiver o pulo duplo, e não estiver atacando, pule
            if (((grounded || holdingLedge) && !jumpBufferTimer.isOver() && !onJump) && verticalAxis >= 0 && Input.GetButton("Jump") || (DoubleJumpOn && !grounded && !onJump && jumpCounter < 2 && !jumpBufferTimer.isOver()) && verticalAxis >= 0 && Input.GetButton("Jump"))
            {
                jumpCounter++;
                onJump = true;
                controllableJumpTimer.reset();
            }
            //estabelece o pulo controlável
            if (onJump && Input.GetButton("Jump") && !controllableJumpTimer.isOver())
                controllableJumpTimer.autoTick();
            else
                onJump = false;
            //evita que o jogador use o pulo duplo no chão ou pule mais do que 2 vezes
            if ((grounded || holdingLedge) && !onJump)
            {
                jumpCounter = 0;
            }
        }

        //pular do muro?
        if (exitWall)    //reseta o pulo duplo e o air dash após sair de um muro
        {
            canAirDash = true;
            jumpCounter = 1;
            exitWall = false;
        }
        if (WallJumpOn && (onWall || wallwaiting))   //se estiver em um muro e o wall jump estiver ligado...
        {
            if (Input.GetButtonDown("Jump"))    //e você apertar o botão de pulo, pule
            {
                onWallJump = true;
                wallJumpTimer.reset();
            }
        }

        wallJumpTimer.autoTick();
        if (onWallJump && wallJumpTimer.isOver())  //controla a duração do pulo
            onWallJump = false;

        //e, por fim, escalar o muro?
        if (WallClimbOn && onWall)   //se você estiver num muro e a escalada estiver ligada claro
        {
            if (previousVerticalAxis == 0 && verticalAxis > 0 && wallClimbCooldownTimer.isOver())   //só apertar pra cima :)
            {
                wallClimbing = true;
                wallClimbCooldownTimer.reset();
                wallSlideTimer.reset();
            }
            previousVerticalAxis = verticalAxis;
        }
        wallClimbCooldownTimer.autoTick();  //com um pequeno cooldown
    }

    private void generalMovement()  //faz o que você quer fazer
    {
        //andar, se segurar na plataforma e o dash
        if (normalSpeed == 0)
            normalSpeed = Speed;    //estabelece a velocidade de caminhada
        if (!restrained) //se não estiver segurando o gatilho 5 ou atacando...
        {
            if (holdingLedge)
                rb.velocity = new Vector2(horizontalAxis * normalSpeed / 2, 0);   //se segurando na plataforma
            else if (!onDash)
            {
                if (smoothWalk)
                    rb.velocity = Vector2.SmoothDamp(rb.velocity, new Vector2(horizontalAxis * normalSpeed, rb.velocity.y), ref speedRef, Acceleration);  //ande :v
                else
                    rb.velocity = new Vector2(horizontalAxis * normalSpeed, rb.velocity.y);
            }
            else    //quando estiver no dash...
            {
                rb.velocity = new Vector2(directionOfMovement * DashSpeed, rb.velocity.y); ////dê o dash :v
            }
        }
        else if (grounded) //se estiver segurando o gatilho 4 ou atacando e estiver no chão...
            rb.velocity = new Vector2(0, rb.velocity.y); //fique parado

        //dash
        if (enterDash)  //quando começar um dash...
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);    //mate o momentum vertical
            dashTimer.reset();
            enterDash = false;

        }
        if (exitDash)   //quando sair de um dash...
        {
            dashCooldownTimer.reset(); //comece a contar o cooldown
            dashTimer.reset();
            rb.velocity = new Vector2(rb.velocity.x, 0);    //mate o momentum vertical de novo
            exitDash = false;
        }

        //pulo
        if (onJump) //pule :v
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
        }

        //wall jump
        if (onWall) //se estiver em um muro isto é
        {
            wallSlideTimer.autoTick();

            if (!enterWall)
            {
                rb.velocity = Vector2.zero;
                enterWall = true;
            }

            if (!wallSlideTimer.isOver())   //controla o wall slide
                appliedMaxFallSpeed = WallSlideSpeed;
            else
                appliedMaxFallSpeed = MaxFallSpeed;
        }
        else    //reseta o timer do wall slide e desliga ele
        {
            wallSlideTimer.reset();
            appliedMaxFallSpeed = MaxFallSpeed;
        }
        if (onWallJump) //pule do muro :v
        {
            rb.velocity = new Vector2(WallJumpXForce * wallRotation, WallJumpYForce);
        }

        //wall climb
        if (onWall && WallClimbOn && wallClimbing)
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(rb.velocity.x, verticalAxis * WallClimbSpeed), ForceMode2D.Impulse);
            wallClimbing = false;
        }

        //descer plataforma
        if (goingDown && descending == 0)
        {
            foreach (var platform in targetPlatforms)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), platform, false);
            }
            targetPlatforms.Clear();
            goingDown = false;
        }
    }

    private void sensorControl()
    {
        //estabelece os sensores
        grounded = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y + GroundSensorPosition), new Vector2(GroundSensorWidth, GroundSensorSight), 0f, ground);
        platformed = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y + GroundSensorPosition), new Vector2(GroundSensorWidth, GroundSensorSight), 0f, platformFilter, resultsA);
        descending = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y + PlatformDescendSensorPosition), new Vector2(PlatformDescendSensorWidth, PlatformDescendSensorHeight), 0f, platformFilter, resultsB);
        leftwalled = Physics2D.OverlapBox(new Vector2(transform.position.x - WallSensorDistance, transform.position.y), new Vector2(WallSensorSight, WallSensorHeight), 0, obstruction);
        rightwalled = Physics2D.OverlapBox(new Vector2(transform.position.x + WallSensorDistance, transform.position.y), new Vector2(WallSensorSight, WallSensorHeight), 0, obstruction);
        clipping = Physics2D.OverlapBox(transform.position, new Vector2(ClippingSensorWidth, ClippingSensorHeight), 0, platform);
        ledged = Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y + LedgeGrabSensorPosition), new Vector2(LedgeGrabSensorWidth, LedgeGrabSensorHeight), 0, platform);
        //e controla eles
        if (
            (leftwalled || rightwalled) &&
            !grounded &&
            Input.GetAxisRaw("Horizontal") != 0 &&
            !onWallJump &&
            WallJumpOn &&
            !clipping
            )
        {
            onWall = true;
            exitWall = true;
            wallRotation = leftwalled? 1 : -1;
        }
        else if((leftwalled || rightwalled) &&
            !grounded &&
            !onWallJump &&
            WallJumpOn &&
            !clipping
            )
        {
            wallwaiting = true;
            wallRotation = leftwalled ? 1 : -1;
        }
        else
        {
            enterWall = false;
            wallwaiting = false;
            onWall = false;
        }
        if (grounded)
        {
            enterWall = true;
            exitWall = false;
            canAirDash = true;
        }
    }

    private void gravityControl()   //controla a gravidade
    {
        //esse aqui é facil de ler, não preciso comentar
        if (!grounded)
        {
            if (onDash || holdingLedge)
                rb.gravityScale = 0;
            else if (onJump)
                rb.gravityScale = Gravity;
            else if (onWall)
                rb.gravityScale = WallSlideGravity;
            else
                rb.gravityScale = Gravity * GravityMultiplier;
        }
        else
        {
            if (onDash)
                rb.gravityScale = 0;
            else
                rb.gravityScale = Gravity;
            
        }
        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -appliedMaxFallSpeed, float.MaxValue));  //limita a velocidade de queda máxima
    }
    

    private void Start()
    {
        platformFilter.useLayerMask = true;
        platformFilter.layerMask = platform;
        appliedMaxFallSpeed = MaxFallSpeed;      //estabelece a velocidade de queda máxima
    }

    void Update()
    {
        sensorControl();
        inputControl();
        stillGrounded = horizontalAxis == 0 && grounded;
        dashing = enterDash || onDash;
        airborne = !grounded;
    }

    private void FixedUpdate()
    {
        gravityControl();
        generalMovement();
    }

    private void OnDrawGizmosSelected() //desenha os sensores
    {
        if (showSensors)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(new Vector2(transform.position.x, transform.position.y + GroundSensorPosition), new Vector2(GroundSensorWidth, GroundSensorSight)); //mostra o sensor do chão

            Gizmos.color = Color.blue;
            Gizmos.DrawCube(new Vector2(transform.position.x + WallSensorDistance, transform.position.y), new Vector2(WallSensorSight, WallSensorHeight));    //mostra o sensor do muro direito
            Gizmos.DrawCube(new Vector2(transform.position.x - WallSensorDistance, transform.position.y), new Vector2(WallSensorSight, WallSensorHeight));    //mostra o sensor do muro esquerdo

            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(new Vector2(transform.position.x, transform.position.y + LedgeGrabSensorPosition), new Vector2(LedgeGrabSensorWidth, LedgeGrabSensorHeight));   //mostra o sensor de ledge grab

            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y + GroundSensorPosition), new Vector2(0, -0.1f+ GroundSensorPosition));

            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(new Vector2(transform.position.x, transform.position.y + PlatformDescendSensorPosition), new Vector2(PlatformDescendSensorWidth, PlatformDescendSensorHeight));
        }
    }
}
