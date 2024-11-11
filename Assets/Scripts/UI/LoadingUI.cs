using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Slider loadingSlider;

    public void SetActive(bool active, int totalProgress = 1)
    {
        gameObject.SetActive(active);
        loadingSlider.value = 0;
        loadingSlider.maxValue = totalProgress;
    }

    public void Loading(int progress)
    {
        loadingSlider.value = progress;
    }
}
