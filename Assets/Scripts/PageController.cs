using UnityEngine;
using UnityEngine.UI;

public class PageController : MonoBehaviour
{
    public GameObject homePage; 
    public GameObject aboutUs;
    public GameObject systemBasics;
    public GameObject appGuide;

    void Start()
    {
        OpenHome();
    }

    public void OpenAboutUs()
    {
        aboutUs.SetActive(true);
        systemBasics.SetActive(false);
        homePage.SetActive(false);
        appGuide.SetActive(false);
    }

    public void OpenSystemBasics()
    {
        systemBasics.SetActive(true);
        aboutUs.SetActive(false);
        homePage.SetActive(false);
        appGuide.SetActive(false);
    }

    public void OpenAppGuide()
    {
        appGuide.SetActive(true);
        aboutUs.SetActive(false);
        homePage.SetActive(false);
        systemBasics.SetActive(false);
    }

    public void OpenHome()
    {
        homePage.SetActive(true);
        aboutUs.SetActive(false);
        systemBasics.SetActive(false);
        appGuide.SetActive(false);
    }
}
