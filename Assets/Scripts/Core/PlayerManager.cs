using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public List<Player> Players = new List<Player>();
    public List<Player> GetAlivePlayers()
    {
        return Players.FindAll(p => p.IsAlive);
    }
    public void InitializePlayers()
    {
        foreach (var player in Players)
        {
            player.ResetPlayer();
        }
        Debug.Log("[PlayerManager] 所有玩家已初始化");
    }
    public void ResetAllPlayers()
    {
        InitializePlayers();
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
