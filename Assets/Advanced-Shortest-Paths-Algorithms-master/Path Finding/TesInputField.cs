using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TesInputField : MonoBehaviour
{
    InputField InputField;
    // Start is called before the first frame update
    void Start()
    {
        InputField = GetComponent<InputField>();
        InputField.onEndEdit.AddListener(InputClose);

        //为输入框添加点击事件
        var eventTrigger = InputField.gameObject.AddComponent<EventTrigger>();
        UnityAction<BaseEventData> clickEvent = OnInputFieldClicked;
        EventTrigger.Entry onClick = new EventTrigger.Entry()
        {
            eventID = EventTriggerType.PointerClick
        };
        onClick.callback.AddListener(clickEvent);
        eventTrigger.triggers.Add(onClick);
        //为输入框添加点击事件

    }

    private void OnInputFieldClicked(BaseEventData arg0)
    {
        Debug.Log("OnInputFieldClicked");
    }

    private void OnValueChanged(string arg0)
    {
        Debug.Log("OnValueChanged: " + arg0);
    }

    private void InputClose(string arg0)
    {
        Debug.Log("InputClose: " + arg0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
