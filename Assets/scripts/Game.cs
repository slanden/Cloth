using UnityEngine;
using UnityEngine.UI;

public class Game : MonoBehaviour 
{
    public Button reset, exit;
    SCloth clothProgram;
    

    void Awake()
    {
        clothProgram = FindObjectOfType<SCloth>();

        reset.onClick.AddListener(Reset);
        exit.onClick.AddListener(Exit);
    }

    void Exit()
    {
        Application.Quit();
    }

    void Reset()
    {
        clothProgram.Reset();
    }
}
