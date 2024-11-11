using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ExerciseSelectButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    private Button button;
    public void Init(int exerIndex)
    {
        button = GetComponent<Button>();
        //button.onClick.AddListener(() => UIController.Instance.SelectExercise(exerIndex));
        nameText.text = ExerciseData.ExerciseNames[exerIndex];
    }
}
