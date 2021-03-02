using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Card : MonoBehaviour
{
    [Header("Set Dynamically")]
    public string suit; // Масть карты (C,D,H, или S)
    public int rank; // Достоинство карты (1-14)
    public Color color = Color.black; // Цвет значков
    public string colS = "Black"; // Имя цвета

    // Этот список хранит все игровые объекты Decorator
    public List<GameObject> decoGOs = new List<GameObject>();
    // Этот список хранит все игровые объекты Pip
    public List<GameObject> pipGos = new List<GameObject>();

    public GameObject back; // Игровой объект рубашки карты

    public CardDefinition def; // Извлекается из DeckXML.xml

    /// <summary>
    /// Список компонентов SpriteRenderer этого и вложенных в него игровых объектов
    /// </summary>
    public SpriteRenderer[] spriteRenderers;

    private void Start()
    {
        SetSortOrder(0); // Обеспечит правильную сортировку карт
    }
    
    /// <summary>
    /// Если spriteRenderers не определен, эта функция определит его
    /// </summary>
    public void PopulateSpriteRenderers()
    {
        // Если spriteRenderers содержит null или пустой список 
        if(spriteRenderers == null || spriteRenderers.Length == 0)
        {
            // Получить компоненты SpriteRenderer этого игрового объекта и вложенных в него игровых объектов
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Инициализирует поле sortingLayerName во всех компонентах SpriteRenderer
    /// </summary>
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();

        foreach(SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }

    /// <summary>
    /// Инициализирует поле sortingOrder всех компонентов SpriteRenderer
    /// </summary>
    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();

        foreach(SpriteRenderer tSR in spriteRenderers)
        {
            if(tSR.gameObject == this.gameObject)
            {
                // Если компонент принадлежит текущему игровому объекту, это фон
                tSR.sortingOrder = sOrd; // Установить порядковый номер для сортировки в sOrd
                continue; // И перейти к следующей итерации цикла
            }

            // Каждый дочерний игровой объект имеет имя
            // Установить порядковый номер для сортировки, в зависимости от имени
            switch(tSR.gameObject.name)
            {
                case "back":
                    // Установить наибольший порядковый номер для отображения поверх других спрайтов
                    tSR.sortingOrder = sOrd + 2;
                    break;
                case "face":
                default:
                     // Установить промежуточный порядковый номер для отображения поверх фона
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    public bool faceUp
    {
        get
        {
            return (!back.activeSelf);
        }
        set
        {
            back.SetActive(!value);
        }
    }

    virtual public void OnMouseUpAsButton()
    {
        //print(name);
    }
}


[System.Serializable]
public class Decorator{
    // Этот класс хранит информацию из DeckXML о каждом значке на карте
    public string type; // Значок, определяющий достоинство карты имеет type="pip"
    public Vector3 loc; // Местоположение спрайта на карте
    public bool flip = false; // Признак переворота спрайта по вертикали
    public float scale = 1f; // Масштаб спрайта
}

[System.Serializable]
public class CardDefinition
{
    // Этот класс хранит информацию о достоинстве карты
    public string face; // Спрайт, изображающий лицевую сторону карты
    public int rank; // Достоинство карты (1-13)
    public List<Decorator> pips = new List<Decorator>(); // Значки
}
