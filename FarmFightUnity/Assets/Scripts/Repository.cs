using UnityEngine;
using System.Collections;

public class Repository : MonoBehaviour
{
    ///Place Variables here

    [SerializeField]
    public Hex selectedHex = Hex.zero;





    /// <summary>
    /// the central repository to store necessary game info inside
    /// </summary>
    public static Repository Central;

    /// <summary>
    /// ensures that should a usuper attempt to arise,
    /// it shall be strangled in the cradle
    /// </summary>
    void Awake()
    {
        if (Central == null)
        {
            Central = this;
        }
        else if (Central != this)
        {
            Destroy(gameObject);
        }
    }




}
