using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum PyramidCardState
{
    drawpile,
    pyramid,
    available,
    discard
}

public class CardPyramid : Card
{
    public PyramidCardState state = PyramidCardState.drawpile;

    public List<CardPyramid> blocking = new();

    public int layoutID;

    public SlotDef slotDef;

    public Behaviour Halo;

    public override void OnMouseUpAsButton()
    {
        Pyramid.S.CardClicked(this);

        base.OnMouseUpAsButton();
    }
}
