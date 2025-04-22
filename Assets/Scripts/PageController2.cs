using UnityEngine;
using UnityEngine.UI;

public class PageController2 : MonoBehaviour
{
    public GameObject homePage; 
    public GameObject selectionPage;
    public GameObject simPage;


    void Start()
    {
        OpenHome();
    }

    public void OpenSelectionPage()
    {
        selectionPage.SetActive(true);
        homePage.SetActive(false);
        simPage.SetActive(false);
    }

    public void OpenSimPage()
    {
        simPage.SetActive(true);
        selectionPage.SetActive(false);
        homePage.SetActive(false);
    }

    public void OpenHome()
    {
        homePage.SetActive(true);
        selectionPage.SetActive(false);
        simPage.SetActive(false);
    }
}
