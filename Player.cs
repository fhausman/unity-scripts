using UnityEngine;

public enum PlayerState
{
    Running,
    Jumping,
    Falling
}

public class PlayerBaseState : BaseState
{
    protected Player player;
    protected Vector3 movementVector = new Vector3();

    public PlayerBaseState(Player p)
    {
        player = p;
    }

    public override void onFixedUpdate(float deltaTime)
    {
        //isGrounded won't work if gravity isn't applied all the time
        player.Velocity += -player.transform.up * player.FallSpeed;

        var input = Input.GetAxis("Horizontal");
        movementVector = player.transform.forward * player.Speed //constant move forward
                            - player.transform.right * input * player.DashSpeed; //dashing sides

        player.Controller.Move(movementVector * deltaTime);
    }
}

public class PlayerRunningState : PlayerBaseState
{
    public PlayerRunningState(Player p)
        : base (p)
    {
    }

    public override void onFixedUpdate(float deltaTime)
    {
        base.onFixedUpdate(deltaTime);
        player.Controller.Move(player.Velocity * deltaTime);
    }

    public override void onUpdate(float deltaTime)
    {
        base.onUpdate(deltaTime);

        if (!player.Controller.isGrounded)
        {
            player.StateMachine.ChangeState(PlayerState.Falling);
        }
        else if(Input.GetKeyDown(KeyCode.Space))
        {
            player.StateMachine.ChangeState(PlayerState.Jumping);
        }
        else
        {
            player.Velocity = Vector3.zero;
        }
    }
}

public class PlayerFallingState : PlayerBaseState
{
    private float _fallingTime = 0.0f;
    private bool CanJump { get => player.JumpEnabled && _fallingTime <= player.CoyoteTime; }
    
    public PlayerFallingState(Player p)
        : base(p)
    {
    }

    public override void onFixedUpdate(float deltaTime)
    {
        base.onFixedUpdate(deltaTime);
        player.Controller.Move(player.Velocity * deltaTime);
    }

    public override void onUpdate(float deltaTime)
    {
        base.onUpdate(deltaTime);

        if (player.Controller.isGrounded)
        {
            player.StateMachine.ChangeState(PlayerState.Running);
            player.Velocity = Vector3.zero;
            player.JumpEnabled = true;
        }
        else if(Input.GetKeyDown(KeyCode.Space) && CanJump)
        {
            player.StateMachine.ChangeState(PlayerState.Jumping);
            player.JumpEnabled = false;
        }

        _fallingTime += deltaTime;
    }

    public override void onExit()
    {
        _fallingTime = 0.0f;
    }
}

public class PlayerJumpingState : PlayerBaseState
{
    private float _currentJumpTime = 0.0f;

    public PlayerJumpingState(Player p)
        : base(p)
    {
    }

    public override void onInit(params object[] args)
    {
        player.Velocity += player.transform.up * Mathf.Sqrt(player.JumpForce * -2f * -player.FallSpeed);
        if(!player.DoubleJump)
            player.JumpEnabled = false;
    }

    public override void onFixedUpdate(float deltaTime)
    {
        base.onFixedUpdate(deltaTime);
        player.Controller.Move(player.Velocity * deltaTime);
    }

    public override void onUpdate(float deltaTime)
    {
        base.onUpdate(deltaTime);

        if (_currentJumpTime > player.JumpTime || Input.GetKeyUp(KeyCode.Space))
        {
            player.Velocity = new Vector3(player.Velocity.x, 0.0f, player.Velocity.z);
            player.StateMachine.ChangeState(PlayerState.Falling);
        }

        _currentJumpTime += deltaTime;
    }

    public override void onExit()
    {
        _currentJumpTime = 0.0f;
    }
}


[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    public float Speed { get => _speed; }

    [SerializeField]
    private float _dashSpeed;
    public float DashSpeed { get => _dashSpeed; }

    [SerializeField]
    private float _fallSpeed;
    public float FallSpeed { get => _fallSpeed; }

    [SerializeField]
    private float _jumpForce;
    public float JumpForce { get => _jumpForce; }

    [SerializeField]
    private float _jumpTime;
    public float JumpTime { get => _jumpTime; }

    [SerializeField]
    private float _coyoteTime;
    public float CoyoteTime { get => _coyoteTime; }

    [SerializeField]
    private bool _doubleJump = false;
    public bool DoubleJump { get => _doubleJump; }

    public Vector3 Velocity { get; set; }
    public bool JumpEnabled { get; set; }

    public StateMachine<PlayerState> StateMachine { get; private set; } = new StateMachine<PlayerState>();
    public CharacterController Controller { get; private set; }

    void Start()
    {
        Controller = GetComponent<CharacterController>();
        StateMachine.AddState(PlayerState.Running, new PlayerRunningState(this));
        StateMachine.AddState(PlayerState.Jumping, new PlayerJumpingState(this));
        StateMachine.AddState(PlayerState.Falling, new PlayerFallingState(this));
        StateMachine.ChangeState(PlayerState.Running);
    }

    void Update()
    {
        StateMachine.OnUpdate(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        StateMachine.OnFixedUpdate(Time.fixedDeltaTime);
    }
}
