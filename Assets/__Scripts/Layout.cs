using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[System.Serializable] // Сделать экземпляры SlotDef видимыми в инспекторе Unity
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}

public class Layout : MonoBehaviour
{
    public PT_XMLReader xmlr; // Так же, как Deck имеет PT_XMLReader
    public PT_XMLHashtable xml; // Используется для ускорения доступа к xml
    public Vector2 multiplier; // Смещение от центра раскладки
    // Cсылки SlotDef
    public List<SlotDef> slotDefs; // Все экземпляры SlotDef для рядов 0-3
    public SlotDef drawPile;
    public SlotDef discardPile;
    // Хранит имена всех рядов
    public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    // Эта функция вызывается для чтения файла LayoutXML.xml
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText); // Загрузить XML
        xml = xmlr.xml["xml"][0]; // И определяется xml для ускорения доступа к XML

        // Прочитать множители, определяющие расстояние между картами
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"), CultureInfo.InvariantCulture);
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"), CultureInfo.InvariantCulture);

        // Прочитать слоты
        SlotDef tSD;
        // slotsX используется для ускорения доступа к элемментам <slot>
        PT_XMLHashList slotsX = xml["slot"];

        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef(); // Создать новый экземпляр SlotDef
            if(slotsX[i].HasAtt("type"))
            {
                // Если <slot> имеет атрибут type, прочитать его
                tSD.type = slotsX[i].att("type");
            } else
            {
                // Иначе определить тип как "slot" это отдельная карта в ряду
                tSD.type = "slot";
            }
            // Преобразовать некоторые атрибуты в числовые значения
            tSD.x = float.Parse(slotsX[i].att("x"), CultureInfo.InvariantCulture);
            tSD.y = float.Parse(slotsX[i].att("y"), CultureInfo.InvariantCulture);
            tSD.layerID = int.Parse(slotsX[i].att("layer"), CultureInfo.InvariantCulture);
            // Преобразовать номер ряда layerID в тексте layerName
            tSD.layerName = sortingLayerNames[tSD.layerID];

            switch(tSD.type)
            {
                // Прочитать дополнительные атрибуты, опираясь на тип слота
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if(slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach(string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"), CultureInfo.InvariantCulture);
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
            }
        }
    }
}
