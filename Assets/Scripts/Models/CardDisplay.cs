using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // 因为Label是TMP格式


public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler

{
    public RectTransform offsetContainer; // 手动在 Inspector 绑定 OffsetContainer
    public Image frontImage;  // 卡面图
    public Image backImage;   // 卡背图
    public TMP_Text label; // 在Prefab里加一个Text来标记“HP”或“ATK”

    private CardData data;
    public bool isSelected = false;

    private Vector3 originalOffset;
    //private Vector3 originalPosition;
    //private bool isHovered = false;


    void Awake()
    {
        data = GetComponent<CardData>();
    }



    //唤醒Y坐标
    void Start()
    {
        if (offsetContainer == null)
        {
            Debug.LogError("未绑定 offsetContainer！");
            return;
        }

        originalOffset = offsetContainer.localPosition;
    }
    
    // 鼠标悬停时抬高
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data.ownerId == "Player1" && !data.used)
        {
            offsetContainer.localPosition = originalOffset + new Vector3(0, 20, 0); // 抬高视觉
        }
    }

    // 鼠标离开时还原
    public void OnPointerExit(PointerEventData eventData)
    {
        if (offsetContainer != null)
            offsetContainer.localPosition = originalOffset;
    }



    public void SetCardFace(Sprite cardSprite)
    {
        if (cardSprite == null)
        {
            Debug.LogError("cardSprite 为 null，请检查 GetCardSprite 是否找到了图！");
        }

        if (frontImage == null)
        {
            Debug.LogError("frontImage 未绑定，请在 prefab 中绑定 Image 组件！");
        }

        frontImage.sprite = cardSprite;
    }


    public void ShowFront()
    {
        frontImage.gameObject.SetActive(true);
        backImage.gameObject.SetActive(false);
    }

    public void ShowBack()
    {
        frontImage.gameObject.SetActive(false);
        backImage.gameObject.SetActive(true);
    }

  /*
    // GameManager 要调用的,不知道为什么要调用，总之没有这个会报错
    public void RefreshUI()
    {
        if (data == null) data = GetComponent<CardData>();

        // 根据卡牌类型改变颜色
        Color color = Color.white;
        switch (data.cardType)
        {
            case CardType.Attack: color = Color.red; break;
            case CardType.Defense: color = Color.blue; break;
            case CardType.Special: color = Color.magenta; break;
        }

        if (data.used)
            color.a = 0.3f;

        frontImage.color = color;
    }

    */

  public void RefreshUI()
  {
      if (data == null) data = GetComponent<CardData>();

      Color color = Color.white;
      string labelText = "";
    
      // 检查是否是技能牌
      bool isSkillCard = data.number >= 11 && data.number <= 13;
    
      if (isSkillCard)
      {
          // 技能牌显示逻辑
          switch (data.number)
          {
              case 11: // J
                  color = Color.red;
                  labelText = "J";
                  break;
              case 12: // Q
                  color = Color.green;
                  labelText = "Q";
                  break;
              case 13: // K
                  color = Color.blue;
                  labelText = "K";
                  break;
          }
        
          // 如果技能已使用，降低透明度
          if (data.used)
          {
              color.a = 0.3f;
              labelText += " (USED)";
          }
      }
      else
      {
          // 数字牌显示逻辑
          switch (data.cardType)
          {
              case CardType.Attack:
                  color = Color.red;
                  labelText = "ATK";
                  break;
              case CardType.Defense:
                  color = Color.blue;
                  labelText = "HP";
                  break;
              default:
                  labelText = "";
                  break;
          }

          if (data.used)
              color.a = 0.3f;
      }

      frontImage.color = color;
      if (label != null) label.text = labelText;
  }

    public void OnPointerClick(PointerEventData eventData)
    {
        CardData cardData = GetData();
        if (cardData == null) return;

        // 根据卡牌类型调用不同的处理方法
        if (cardData.cardType == CardType.Special && cardData.number >= 11 && cardData.number <= 13)
        {
            // 技能牌点击 - 只有在战斗阶段才响应
            GameManager.Instance.OnSkillCardClicked(this);
        }
        else
        {
            // 数字牌点击 - 用于HP/ATK选择
            GameManager.Instance.OnCardClicked(this);
        }
    }




    public CardData GetData()
    {
        return data;
    }

}
