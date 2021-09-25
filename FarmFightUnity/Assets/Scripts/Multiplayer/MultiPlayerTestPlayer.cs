using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class MultiPlayerTestPlayer : NetworkBehaviour
{
    [SerializeField] private float speed = 1f;
    public GameObject counter;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });
    public NetworkVariableInt tint = new NetworkVariableInt(0);

    private SpriteRenderer spriteRenderer;

    public override void NetworkStart()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (IsServer)
        {
            UpdateServer();
        }

        if (IsClient)
        {
            UpdateClient();
        }

        UpdateColor();
    }

    void UpdateServer()
    {
            
    }

    void UpdateClient()
    {
        if (!IsLocalPlayer)
        {
            return;
        }

        if (Input.GetKeyDown("q"))
            SubmitCounterSpawnRequestServerRpc();

        else if (Input.GetKeyDown("space"))
            SubmitIncrementRequestServerRpc();

        float horizontalMove = Input.GetAxisRaw("Horizontal") * speed;
        float verticalMove = Input.GetAxisRaw("Vertical") * speed;
        transform.position += new Vector3(horizontalMove, verticalMove, 0);
    }

    void UpdateColor()
    {
        float fractionTinted = 1 - ((tint.Value % 5) / 4f);
        spriteRenderer.color = new Color(1f, fractionTinted, fractionTinted, 1f);
    }


    // ServerRpcs

    [ServerRpc]
    void SubmitCounterSpawnRequestServerRpc()
    {
        GameObject go = Instantiate(counter, Vector3.zero, Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    void SubmitIncrementRequestServerRpc()
    {
        tint.Value++;
    }
}