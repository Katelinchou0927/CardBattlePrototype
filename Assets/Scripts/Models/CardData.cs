using System.Collections;
using UnityEngine;

public enum CardType
{
    Normal,
    Attack,
    Defense,//HP
    Special
}

public class CardData : MonoBehaviour
{
    public string ownerId;       // 玩家ID或名字
    public int number;           // 数字：1~10
    public CardType cardType;    // 类型：攻击、防御等
    public bool used = false;    // 是否已使用
}

