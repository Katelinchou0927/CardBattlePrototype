using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public string PlayerName;
    public int Health = 100;
    public bool IsAlive = true;      // ÊÇ·ñ´æ»î

    public void ResetPlayer()
    {
        Health = 100;
        IsAlive = true;
       
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
