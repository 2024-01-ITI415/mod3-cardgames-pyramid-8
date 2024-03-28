using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PyramidCardState
{
    drawpile,
    pyramid,
    foundation,
    discard
}

public class CardPyramid : Card
{
    public PyramidCardState state = PyramidCardState.drawpile;

    public List<CardPyramid> hiddenBy = new();

    public int layoutID;

    public SlotDef slotDef;

    public override void OnMouseUpAsButton()
    {
        Pyramid.S.CardClicked(this);

        base.OnMouseUpAsButton();
    }
}
