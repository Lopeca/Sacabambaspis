using System;
using TMPro;
using UnityEngine;

public class CheckBox : MonoBehaviour
{
    private bool checkEnabled;
    public TMP_Text checkIcon;

    private void Awake()
    {
        checkIcon = transform.GetChild(0).GetComponent<TMP_Text>();
    }

    public void SetCheckEnabled(bool enabled)
    {
        this.checkEnabled = enabled;
        checkIcon.color = enabled ? Color.green : Color.gray;
    }
}
