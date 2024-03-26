using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public enum eScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}

public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;

    static public int SCORE_FROM_PREVIOUS_ROUND = 0;
    public static int HIGH_SCORE = 0;

    [Header("Set Dynamically")]
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    private void Awake()
    {
        if (S == null)
            S = this;
        else Debug.LogError("Error: SceneManager.Awake(): S is already set");

        if (PlayerPrefs.HasKey("ProspectorHighScore"))
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");

        score += SCORE_FROM_PREVIOUS_ROUND;

        SCORE_FROM_PREVIOUS_ROUND = 0;
    }

    static public void EVENT (eScoreEvent evt)
    {
        try
        {
            S.Event(evt);
        } catch (System.NullReferenceException nre) { Debug.LogError("ScoreManager:Event() called while S=null\n" + nre); }
    }
    
    void Event (eScoreEvent evt)
    {
        switch (evt)
        {
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                break;

            case eScoreEvent.mine:
                chain++;
                scoreRun += chain;
                break;
        }

        switch (evt)
        {
            case eScoreEvent.gameWin:
                SCORE_FROM_PREVIOUS_ROUND += score;
                print("You won this round! Round score: " + score);
                break;

            case eScoreEvent.gameLoss:
                if (HIGH_SCORE <= score)
                {
                    print("You got the high score! High Score: " + score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else print("Your final score for the game was " + score);
                break;

            default:
                print("score: " + score + "  scoreRun: " + scoreRun + "  chain: " + chain);
                break;
        }
    }

    static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    static public int SCORE_RUN { get { return S.scoreRun; } }
}
