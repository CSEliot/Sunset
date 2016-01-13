using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToggleSpriteSwap : MonoBehaviour
{

    public Toggle targetToggle;
    public Sprite selectedSprite;
    public Sprite otherSprite;

    // Use this for initialization
    void Start()
    {
        targetToggle.toggleTransition = Toggle.ToggleTransition.None;
        targetToggle.onValueChanged.AddListener(OnTargetToggleValueChanged);
    }

    void OnTargetToggleValueChanged(bool newValue)
    {
        Image targetImage = targetToggle.targetGraphic as Image;
        if (targetImage != null)
        {
            if (newValue)
            {
                targetImage.overrideSprite = selectedSprite;
            }
            else
            {
                targetImage.overrideSprite = otherSprite;
            }
        }
    }
}