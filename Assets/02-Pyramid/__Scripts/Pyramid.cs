using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Pyramid : MonoBehaviour
{

    static public Pyramid S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;

    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;

    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);

    public float reloadDelay = 2f;

    public Text gameOverText, roundResultText, highScoreText;


    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardPyramid> drawPile;
    public List<CardPyramid> foundation;
    public Transform layoutAnchor;
    public List<CardPyramid> pyramid;
    public List<CardPyramid> discardPile;
    public FloatingScore fsRun;

    public CardPyramid firstCard;

    void Awake()
    {
        S = this;
        SetUpUITexts();
    }

    void SetUpUITexts()
    {
        GameObject go = GameObject.Find("HighScore");
        if (go != null)
        {
            highScoreText = go.GetComponent<Text>();
        }
        int highScore = PyramidScoreManager.LOW_MOVES;
        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
        go.GetComponent<Text>().text = hScore;

        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }

        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }

        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }

    void Start()
    {
        Scoreboard.S.score = PyramidScoreManager.SCORE;

        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);

        drawPile = ConvertListCardsToListCardPyramids(deck.cards);
        Debug.Log(drawPile[0]);
        LayoutGame();
        UpdatePyramidAvailability();
    }

    List<CardPyramid> ConvertListCardsToListCardPyramids(List<Card> lCD)
    {
        List<CardPyramid> lCP = new();
        CardPyramid tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardPyramid;
            Debug.Log(tCD);
            lCP.Add(tCP);
        }
        return lCP;
    }

    CardPyramid Draw()
    {
        CardPyramid cd = drawPile[0];
        drawPile.RemoveAt(0);
        return cd;
    }

    void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardPyramid cp;
        foreach (SlotDef tSD in layout.slotDefs)
        {
            cp = Draw();
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;

            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);

            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = PyramidCardState.pyramid;

            cp.SetSortingLayerName(tSD.layerName);

            pyramid.Add(cp);
        }

        foreach (CardPyramid tCP in pyramid)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.blocking.Add(cp);
            }
        }

        UpdateDrawPile();
    }

    CardPyramid FindCardByLayoutID(int layoutID)
    {
        foreach (CardPyramid tCP in pyramid)
        {
            if (tCP.layoutID == layoutID)
                return tCP;
        }
        return null;
    }

    void UpdatePyramidAvailability()
    {
        foreach (CardPyramid cd in pyramid)
        {
            int blockersGone = 0;
            if (cd.blocking.Count == 0)
                cd.state = PyramidCardState.available;

            foreach (CardPyramid blocker in cd.blocking)
            {
                if (blocker.state == PyramidCardState.discard)
                {
                    blockersGone++;
                }
            }

            if (blockersGone == cd.blocking.Count)
                cd.state = PyramidCardState.available;
        }
    }

    void MoveToDiscard(CardPyramid cd)
    {
        cd.state = PyramidCardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        cd.faceUp = false;

        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    void MoveToFoundation(CardPyramid cd)
    {
        CardPyramid fcd;
        for (int i = 0; i < foundation.Count; i++)
        {
            fcd = foundation[foundation.Count - 1 - i];

            fcd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.foundation.x),
                layout.multiplier.y * (layout.foundation.y),
                -layout.foundation.layerID + 0.1f * (i + 1));

            fcd.faceUp = true;
            fcd.SetSortingLayerName(layout.foundation.layerName);
            fcd.SetSortOrder(-10 * (i + 1));
        }

        cd.state = PyramidCardState.available;
        cd.transform.parent = layoutAnchor;
        foundation.Add(cd);

        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.foundation.x,
            layout.multiplier.y * layout.foundation.y,
            -layout.foundation.layerID);
        cd.faceUp = true;

        cd.SetSortingLayerName(layout.foundation.layerName);
        Debug.Log(layout.foundation.layerName);
        cd.SetSortOrder(0);

        if (firstCard != null)
        {
            firstCard.Halo.enabled = false;
            firstCard = null;
        }

    }

    void UpdateDrawPile()
    {
        CardPyramid cd;

        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);
            cd.faceUp = false;
            cd.state = PyramidCardState.drawpile;

            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public void CardClicked(CardPyramid cd)
    {
        switch (cd.state)
        {
            case PyramidCardState.pyramid:
                break;

            case PyramidCardState.drawpile:
                MoveToFoundation(Draw());
                UpdateDrawPile();
                PyramidScoreManager.EVENT(PyramidScoreEvent.draw);
                FloatingScoreHandler(PyramidScoreEvent.draw);
                break;
            case PyramidCardState.available:
                if (cd.rank == 13)
                {
                    pyramid.Remove(cd);
                    foundation.Remove(cd);
                    MoveToDiscard(cd);
                    UpdatePyramidAvailability();
                    PyramidScoreManager.EVENT(PyramidScoreEvent.match);
                    FloatingScoreHandler(PyramidScoreEvent.match);
                    return;
                }
                if (firstCard == null)
                {
                    firstCard = cd;
                    cd.Halo.enabled = true;
                }
                else
                {
                    if (!AdjacentRank(firstCard, cd))
                    {
                        firstCard = null;
                        return;
                    }

                    pyramid.Remove(firstCard);
                    MoveToDiscard(firstCard);
                    firstCard = null;

                    pyramid.Remove(cd);
                    MoveToDiscard(cd);
                    UpdatePyramidAvailability();
                    PyramidScoreManager.EVENT(PyramidScoreEvent.match);
                    FloatingScoreHandler(PyramidScoreEvent.match);
                }
                break;
        }

        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (pyramid.Count == 0)
        {
            GameOver(true);
            return;
        }

        if (drawPile.Count > 0)
        {
            return;
        }

        foreach (CardPyramid cd in pyramid)
        {
            if (cd.state == PyramidCardState.available)
            {
                foreach (CardPyramid pcd in pyramid)
                {
                    if (pcd.state == PyramidCardState.available)
                    {
                        if (AdjacentRank(cd, pcd))
                        {
                            return;
                        }
                    }
                }

                foreach (CardPyramid fcd in foundation)
                {

                    if (AdjacentRank(cd, fcd))
                    {
                        return;
                    }
                }
            }
        }
        GameOver(false);
    }

    void GameOver(bool won)
    {
        int score = PyramidScoreManager.SCORE;
        if (fsRun != null)
            score += fsRun.score;

        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round!\nRound Score: " + score;
            ShowResultsUI(true);
            PyramidScoreManager.EVENT(PyramidScoreEvent.gameWin);
            FloatingScoreHandler(PyramidScoreEvent.gameWin);
        }
        else
        {
            gameOverText.text = "Game Over";
            if (PyramidScoreManager.LOW_MOVES >= score)
            {
                string str = "You got a new high score!\nHigh Score: " + score;
                roundResultText.text = str;
            }
            else roundResultText.text = "Your final score was " + score;
            ShowResultsUI(true);

            PyramidScoreManager.EVENT(PyramidScoreEvent.gameLoss);
            FloatingScoreHandler(PyramidScoreEvent.gameLoss);
        }

        Invoke(nameof(ReloadLevel), reloadDelay);
    }

    void ReloadLevel()
    {
        SceneManager.LoadScene("__Prospector");
    }

    public bool AdjacentRank(CardPyramid c0, CardPyramid c1)
    {
        c0.Halo.enabled = false;

        if (Mathf.Abs(c0.rank + c1.rank) == 13)
            return true;

        return false;
    }

    void FloatingScoreHandler(PyramidScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            case PyramidScoreEvent.gameWin:
            case PyramidScoreEvent.gameLoss:
                if (fsRun != null)
                {
                    fsPts = new List<Vector2>
                    {
                        fsPosRun,
                        fsPosMid2,
                        fsPosEnd
                    };
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null;
                }
                break;
            case PyramidScoreEvent.draw:
            case PyramidScoreEvent.match:
                FloatingScore fs;
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>
                {
                    p0,
                    fsPosMid,
                    fsPosRun
                };
                fs = Scoreboard.S.CreateFloatingScore(PyramidScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }
    }

    public void MoveFoundationToDrawPile()
    {
        CardPyramid cd;
        for (int i = 0; i < foundation.Count; i++)
        {
            cd = foundation[i];
            cd.transform.parent = layoutAnchor;

            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);
            cd.faceUp = false;

            drawPile.Add(cd);
            cd.state = PyramidCardState.drawpile;

            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }

        foundation.Clear();
    }
}
