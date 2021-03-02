using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Перечесление, определяющее тип переменной, которая может принимать несколько предопрделенных значений
public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}

/// <summary>
/// Расширяет Card
/// </summary>
public class CardProspector : Card
{
    [Header("Set Dynamically: CardProspector")]
    // Так используется перечесление eCardState
    public eCardState state = eCardState.drawpile;
    // hiddenBy - списко других карт, не позволяющих перевернуть эту лицом вверх
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    // layoutID определяет для этой карты ряд в раскладке
    public int layoutID;
    // Класс SlotDef хранит информацию из элемента <slot> в LayoutXML
    public SlotDef slotDef;

    public override void OnMouseUpAsButton()
    {
        // Вызвать метод CardClicked объекта одиночки Prospector
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
