using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class FramerateController : MonoBehaviour
{
    public Slider framerateSlider;
    public TextMeshProUGUI framerateText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int[] framerates = new int[] {60, 120, 144, 240 };
    private const string FrameRateKey = "FrameRate";
    void Start()
    {
        Application.targetFrameRate = 60;
        framerateSlider.minValue = 0;
        framerateSlider.maxValue = framerates.Length - 1;
        framerateSlider.wholeNumbers = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
