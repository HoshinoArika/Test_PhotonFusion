using System;
using Fusion;
using TMPro;
using UnityEngine;

public sealed class Fusion100Player : NetworkBehaviour
{
    [SerializeField] private Fusion100Ball pfBall;
    [SerializeField] private Fusion100PhysxBall pfPhysxBall;


    #region 105 Property Changes

    [Networked(OnChanged = nameof(OnBallSpawned))]
    public NetworkBool BallSpawned { get; set; }

    private Material _material;

    private Material Material
    {
        get
        {
            if (_material == null)
                _material = GetComponentInChildren<MeshRenderer>().material;
            return _material;
        }
    }

    public static void OnBallSpawned(Changed<Fusion100Player> changed)
    {
        changed.Behaviour.Material.color = Color.white;
    }

    public override void Render()
    {
        Material.color = Color.Lerp(Material.color, Color.blue, Time.deltaTime);
    }

    #endregion

    private Vector3 _forward;
    [Networked] private TickTimer Delay { get; set; }

    private NetworkCharacterControllerPrototype _cc;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void FixedUpdateNetwork()
    {
        CalculateInput();
    }

    private void CalculateInput()
    {
        if (!GetInput(out NetworkInputData data)) return;

        data.Direction.Normalize();
        _cc.Move(5 * data.Direction * Runner.DeltaTime);

        if (data.Direction.sqrMagnitude > 0)
            _forward = data.Direction;

        if (Delay.ExpiredOrNotRunning(Runner))
        {
            if ((data.Buttons & NetworkInputData.MouseButton1) != 0)
            {
                Delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                Runner.Spawn(pfBall,
                    transform.position + _forward, Quaternion.LookRotation(_forward),
                    Object.InputAuthority, ((runner, o) => { o.GetComponent<Fusion100Ball>().Init(); })
                );
                BallSpawned = !BallSpawned;
            }
            else if ((data.Buttons & NetworkInputData.MouseButton2) != 0)
            {
                Delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

                Runner.Spawn(pfPhysxBall,
                    transform.position + _forward, Quaternion.LookRotation(_forward),
                    Object.InputAuthority, ((runner, o) => { o.GetComponent<Fusion100PhysxBall>().Init(_forward); })
                );

                BallSpawned = !BallSpawned;
            }
        }
    }

    #region RPC

    private void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    [SerializeField] private TextMeshProUGUI tmpMessagesDisplay;

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        if (tmpMessagesDisplay == null)
            tmpMessagesDisplay = FindObjectOfType<TextMeshProUGUI>();
        if (info.IsInvokeLocal)
            message = $"You said: {message}\n";
        else
            message = $"Some other player said: {message}\n";
        tmpMessagesDisplay.text += message;
    }

    #endregion
}