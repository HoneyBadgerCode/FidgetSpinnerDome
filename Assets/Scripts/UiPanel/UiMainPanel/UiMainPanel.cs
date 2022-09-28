using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiMainPanel : MonoBehaviour
{
    public Text SpeedText;
    public Text SpinsText;
    public InputField OnChangeSlideOffsetInput;
    
    // Start is called before the first frame update
    void Start()
    {
        EventManager.OnShowSpinnerSpeed.AddEventHandler(OnShowSpinnerSpeed);
        EventManager.OnShowSpinnerSpins.AddEventHandler(OnShowSpinnerSpins);
        OnChangeSlideOffsetInput.onEndEdit.AddListener(OnChangeSlideOffset);
    }

    private void OnDestroy()
    {
        EventManager.OnShowSpinnerSpeed.RemoveEventHandler(OnShowSpinnerSpeed);
        EventManager.OnShowSpinnerSpins.RemoveEventHandler(OnShowSpinnerSpins);
    }

    private void OnShowSpinnerSpins(int obj)
    {
        SpinsText.text = $"SPINS: <b><size=80>{obj}</size></b>";;
    }

    private void OnShowSpinnerSpeed(int obj)
    {
        SpeedText.text = $"<b><size=80>{obj}</size></b> /min";
    }

    void OnChangeSlideOffset(string org)
    {
        EventManager.OnChangeSlideOffset.BroadCastEvent(org);
    }
}
