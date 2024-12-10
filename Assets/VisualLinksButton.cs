using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VisualLinksButton : MonoBehaviour
{

    public bool ButtonState;

    public UnityEvent m_MyEvent;

    private Material myMat;

    //public delegate void OnButtonActivated();

    //public event OnButtonActivated OnButtonActivatedEvent;

    // Start is called before the first frame update
    void Start()
    {
        if (m_MyEvent == null)
            m_MyEvent = new UnityEvent();

        var meshrenderer = GetComponent<MeshRenderer>();
        if (meshrenderer.material)
        {
            meshrenderer.material = Instantiate(meshrenderer.material);
            meshrenderer.material.color = ButtonState ? Color.green : Color.red;
            myMat = meshrenderer.material;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        m_MyEvent?.Invoke();
    }

    public void ToggleButtonState()
    {
        ButtonState = !ButtonState;
        myMat.color = ButtonState ? Color.green : Color.red;
    }

    public void SetButtonState(bool newState)
    {
        ButtonState = newState;
        myMat.color = ButtonState ? Color.green : Color.red;
    }
}
