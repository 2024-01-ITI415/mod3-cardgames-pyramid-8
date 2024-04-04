using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public enum PyramidScoreEvent
{
    draw,
    match,
    gameWin,
    gameLoss
}

public class PyramidScoreManager : MonoBehaviour
{
    static private PyramidScoreManager S;

    public static int LOW_MOVES = 1000;

    [Header("Set Dynamically")]
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    private void Awake()
    {
        if (S == null)
            S = this;
        else Debug.LogError("Error: SceneManager.Awake(): S is already set");

        if (PlayerPrefs.HasKey("PyramidLeastMoves"))
            LOW_MOVES = PlayerPrefs.GetInt("PyramidLeastMoves");
    }

    static public void EVENT(PyramidScoreEvent evt)
    {
        try
        {
            S.Event(evt);
        }
        catch (System.NullReferenceException nre) { Debug.LogError("ScoreManager:Event() called while S=null\n" + nre); }
    }

    void Event(PyramidScoreEvent evt)
    {
        switch (evt)
        {
            case PyramidScoreEvent.gameWin:
            case PyramidScoreEvent.gameLoss:
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                break;


            case PyramidScoreEvent.draw:
            case PyramidScoreEvent.match:
                chain = 1;
                scoreRun += chain;
                break;
        }

        switch (evt)
        {
            case PyramidScoreEvent.gameWin:
            case PyramidScoreEvent.gameLoss:
                if (LOW_MOVES >= score)
                {
                    print("You got the fewest moves! Moves: " + score);
                    LOW_MOVES = score;
                    PlayerPrefs.SetInt("PyramidLeastMoves", score);
                }
                else print("Your final moves for the game was " + score);
                break;

            default:
                print("score: " + score + "  scoreRun: " + scoreRun);
                break;
        }
    }
    static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    static public int SCORE_RUN { get { return S.scoreRun; } }
}
