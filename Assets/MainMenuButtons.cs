using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    public void ProspectorButton()
    {
        SceneManager.LoadScene("__Prospector");
    }

    public void PyramidBUtton()
    {
        SceneManager.LoadScene("__Pyramid");
    }
}
