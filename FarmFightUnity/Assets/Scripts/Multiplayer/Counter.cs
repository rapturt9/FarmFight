using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;
using UnityEngine.UI;

public class Counter : NetworkBehaviour
{
    [SerializeField] private float timeToChange = 2f;
    private float timeSinceChange;

    public Text myText;
    public NetworkVariableInt count = new NetworkVariableInt(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    private SpriteRenderer spriteRenderer;

    public override void NetworkStart()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        timeSinceChange = timeToChange;
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
    }

    void UpdateServer()
    {
        //timeSinceChange -= Time.deltaTime;
        //if (timeSinceChange <= 0)
        //{
        //    count.Value++;
        //    timeSinceChange = timeToChange;
        //}
    }

    void UpdateClient()
    {
        myText.text = count.Value.ToString();
        UpdateColor();
    }

    void UpdateColor()
    {
        float fractionBlack = 1 - ((count.Value % 10) / 9f);
        spriteRenderer.color = new Color(fractionBlack, fractionBlack, fractionBlack, 1f);
    }

    void Increment(int amount = 1)
    {
        count.Value += amount;
    }

    // Adds count when collided with
    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision");
        if (!IsServer) { return; }

        Increment();
    }

    // Adds count when clicked
    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clicked");
            IncrementServerRpc();
        }
    }


    // Server rpcs

    [ServerRpc(RequireOwnership = false)]
    void IncrementServerRpc(int amount = 1)
    {
        count.Value += amount;
    }
}