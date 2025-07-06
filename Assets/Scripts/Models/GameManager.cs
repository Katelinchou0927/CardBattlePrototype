using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 用于CardDisplay反向访问

    [Header("基础资源")]
    public GameObject cardPrefab;
    public Sprite[] cardSprites; // 所有卡面图
    public Sprite cardBack;      // 卡背图
    public Button confirmButton;  // 攻击、血量确认按钮;




    [Header("手牌区域")]
    public Transform player1HandArea;
    public Transform player2HandArea;

    private List<CardData> player1Cards = new List<CardData>();
    private List<CardData> player2Cards = new List<CardData>();

    private CardData selectedHpCard = null;
    private CardData selectedAtkCard = null;

    private bool atkConfirmed = false;
    private bool hpConfirmed = false;

    void Awake()
    {
        Instance = this;
    }

    public void Start(){}

    public void StartGame()
    {
        Debug.Log("[GameManager] StartGame 被调用，开始发牌");
        // 清除旧卡牌
        foreach (Transform child in player1HandArea) Destroy(child.gameObject);
        foreach (Transform child in player2HandArea) Destroy(child.gameObject);
        player1Cards.Clear();
        player2Cards.Clear();
        // 发牌逻辑
        DealCards("Player1", "club", player1HandArea, player1Cards);
        DealCards("Player2", "diamond", player2HandArea, player2Cards);

        //显示选牌确认按钮
        atkConfirmed = false;
        hpConfirmed = false;
        selectedAtkCard = null;
        selectedHpCard = null;

        confirmButton.gameObject.SetActive(true);
        confirmButton.interactable = false;
        // 监听器
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);
        confirmButton.GetComponentInChildren<TMP_Text>().text = "Confirm ATK";


    }

    void DealCards(string ownerId, string suit, Transform handArea, List<CardData> cardList)

    {
        List<int> numbers = new List<int>();
        for (int i = 1; i <= 10; i++) numbers.Add(i);
        Shuffle(numbers); // 完成卡牌顺序打乱

        // 按打乱的数组生成卡牌
        for (int i = 0; i < numbers.Count; i++)
        {
            GameObject cardGO = Instantiate(cardPrefab, handArea);
            CardDisplay display = cardGO.GetComponent<CardDisplay>();

            int number = numbers[i];
            Sprite cardSprite = GetCardSprite(number, suit);
            display.SetCardFace(cardSprite);
            display.ShowFront();

            CardData data = cardGO.GetComponent<CardData>();
            data.ownerId = ownerId;
            data.number = number;
            data.cardType = CardType.Normal;
            data.used = false;

            cardList.Add(data);
            display.RefreshUI();
        }

    }

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    Sprite GetCardSprite(int number, string suit)
    {
        string name = $"{number}_{suit.ToLower()}";  // e.g., "7_club"
        foreach (Sprite sprite in cardSprites)
        {
            if (sprite.name == name)
                return sprite;
        }

        Debug.LogWarning($"找不到对应卡图：{name}");
        return null;
    }

    public void OnCardClicked(CardDisplay clicked)
    {
        CardData data = clicked.GetData();
        if (data.used) return;

        // 阶段一：选择攻击卡
        if (!atkConfirmed)
        {
            // 点击已选择的攻击卡 → 取消
            if (selectedAtkCard == data)
            {
                data.cardType = CardType.Normal;
                selectedAtkCard = null;
            }
            else
            {
                if (selectedAtkCard != null)
                    selectedAtkCard.cardType = CardType.Normal;

                selectedAtkCard = data;
                data.cardType = CardType.Attack;
            }
        }
        // 阶段二：攻击已确认 → 选择 HP 卡
        else if (atkConfirmed && !hpConfirmed)
        {
            if (selectedHpCard == data)
            {
                data.cardType = CardType.Normal;
                selectedHpCard = null;
            }
            else
            {
                if (selectedHpCard != null)
                    selectedHpCard.cardType = CardType.Normal;

                selectedHpCard = data;
                data.cardType = CardType.Defense;
            }
        }
        Debug.Log($"点击了卡：{data.number}, 当前类型：{data.cardType}");


        // 更新所有卡片外观
        foreach (var card in player1Cards)
            card.GetComponent<CardDisplay>().RefreshUI();

        // 控制按钮是否可按
        confirmButton.interactable = (!atkConfirmed && selectedAtkCard != null)
                                  || (atkConfirmed && !hpConfirmed && selectedHpCard != null);
    }

    public void OnConfirmClicked()
    {
        if (!atkConfirmed && selectedAtkCard != null)
        {
            selectedAtkCard.used = true;
            atkConfirmed = true;
            confirmButton.interactable = false;

            Debug.Log($"攻击卡已确认：ATK = {selectedAtkCard.number}");
            confirmButton.GetComponentInChildren<TMP_Text>().text = "Confirm HP";
        }
        else if (atkConfirmed && !hpConfirmed && selectedHpCard != null)
        {
            selectedHpCard.used = true;
            hpConfirmed = true;
            confirmButton.interactable = true; // 允许点击进入战斗
            Debug.Log($"血量卡已确认：HP = {selectedHpCard.number}");
            confirmButton.GetComponentInChildren<TMP_Text>().text = "Battle Start";

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(StartBattle);
        }

        foreach (var card in player1Cards)
            card.GetComponent<CardDisplay>().RefreshUI();
    }

    void StartBattle()
    {
        confirmButton.gameObject.SetActive(false);
        Debug.Log($"战斗开始！ATK = {selectedAtkCard.number}, HP = {selectedHpCard.number}");
        // 可扩展战斗逻辑...
    }


}

