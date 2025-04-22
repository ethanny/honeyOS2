using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timedate : MonoBehaviour
{
    [SerializeField] TMP_Text Time;
    [SerializeField] TMP_Text Date;

    void Update()
    {
        Time.text = DateTime.Now.ToString("hh:mm tt");
        Date.text = DateTime.Now.ToString("MMMM dd, yyyy");
    }
}
