using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckReshuffler : MonoBehaviour
{
    public Pyramid Pyramid;

    public void OnMouseUpAsButton()
    {
        Debug.Log("reshuffle?");
        Pyramid.MoveFoundationToDrawPile();
    }
}
