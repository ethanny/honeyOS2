using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SegmentManager : MonoBehaviour
{
    public GameObject memSegmentPrefab;
    public GameObject memoryFrame;

    private const float MAX= 1024;
    public void AddSegment(Segment segment){
        
        int baseAdr = segment.baseAdr;
        int memSize = segment.process.memorySize;

        Transform memoryFrameTransform = memoryFrame.GetComponent<Transform>();
        
        if (memoryFrameTransform != null){

            GameObject newSegment = Object.Instantiate(memSegmentPrefab, memoryFrame.transform);

            segment.SetObjectRef(newSegment);

            TextMeshProUGUI segmentIDComponent = FindTextMeshProComponent(newSegment.transform);
            if (segmentIDComponent != null)
            {
                string id = segment.process.id.ToString();
                segmentIDComponent.text = id;
            }
            else
            {
                Debug.LogError("TextMeshPro component not found.");
            }
            
            RectTransform newSegmentTransform = newSegment.GetComponent<RectTransform>();
            
            if (newSegmentTransform != null){

                // Set base address using Position Y
                // 0 to -1 (MAX)
                float newBaseAdr = (baseAdr / MAX) * -1;

                // Set size using scale Y
                // 0 to 1 (MAX)
                float newSize = (memSize / MAX);
                
                newSegmentTransform.localScale = new Vector2(newSegmentTransform.sizeDelta.x, newSize);
                // Randomize color
                SpriteRenderer segmentSprite = newSegment.GetComponent<SpriteRenderer>();
                if (segmentSprite != null)
                {
                    Color randomColor = new Color(Random.value, Random.value, Random.value);
                    segmentSprite.color = randomColor;
                }
                newSegmentTransform.anchoredPosition = new Vector2(newSegmentTransform.anchoredPosition.x, newBaseAdr);
            }
        }
    }

    private TextMeshProUGUI FindTextMeshProComponent(Transform parentTransform)
    {
        // Check if the current transform has a TextMeshPro component
        TextMeshProUGUI tmPro = parentTransform.GetComponent<TextMeshProUGUI>();
        if (tmPro != null)
        {
            return tmPro;
        }

        // If not, recursively search the children
        foreach (Transform child in parentTransform)
        {
            TextMeshProUGUI result = FindTextMeshProComponent(child)    ;
            if (result != null)
            {
                return result;
            }
        }

        // If no TextMeshPro component was found, return null
        return null;
    }

    public void DeleteSegment (Segment segment)
    {
        Destroy(segment.segmentObjRef);
    }
}