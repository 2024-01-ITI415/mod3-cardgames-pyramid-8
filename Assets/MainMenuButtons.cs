using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtons : MonoBehaviour
{

    public Text ProspectorHS, PyramidHS;

    private void Start()
    {
        ProspectorHS.text = "High Score: " + PlayerPrefs.GetInt("ProspectorHighScore");
        PyramidHS.text = "Lowest Moves: " + PlayerPrefs.GetInt("PyramidLeastMoves");
    }

    public void ProspectorButton()
    {
        SceneManager.LoadScene("__Prospector");
    }

    public void PyramidBUtton()
    {
        SceneManager.LoadScene("__Pyramid");
    }
}
