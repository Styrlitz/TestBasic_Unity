using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IDragHandler//, IEndDragHandler
{   
    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void OnDrag(PointerEventData eventData)
    {
        Vector3 screenPoint;
        screenPoint.x = rectTransform.position.x;
        screenPoint.y = Input.mousePosition.y;
        screenPoint.z = 10.0f;
        rectTransform.position = screenPoint;
    }
}
