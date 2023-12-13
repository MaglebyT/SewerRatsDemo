using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FighterAnimator : MonoBehaviour
{

    bool isInit = false;

    [SerializeField] Image image;

    SpriteAnimator2 currentAnim;
    SpriteAnimator2 startingAnimation;
    SpriteAnimator2 sleepingAnimation;

    public void Init(List<Sprite> startingSprites, List<Sprite> sleepingSprites)
    {

        startingAnimation = new SpriteAnimator2(startingSprites, image);
        sleepingAnimation = new SpriteAnimator2(sleepingSprites, image);

        isInit = true;
    }


    public void PlayIsStarting()
    {

        currentAnim = startingAnimation;
        currentAnim.Start();
    }

    public void PlayIsSleeping()
    {
        currentAnim = sleepingAnimation;
        currentAnim.Start();
    }

    private void Update()
    {

        if (isInit)
        {

            currentAnim.HandleUpdate();

        }
    }
}