using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteAnimator2
{
    Image image;
    public List<Sprite> frames;
    float frameRate;

    int currentFrame;
    float timer;

    public SpriteAnimator2(List<Sprite> frames, Image image, float frameRate = 0.08f)
    {
        this.frames = frames;
        this.image = image;
        this.frameRate = frameRate;
    }

    public void Start()
    {
        currentFrame = 0;
        timer = 0f;
        image.sprite = frames[0];
    }

    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        if (timer > frameRate)
        {
            currentFrame = (currentFrame + 1) % frames.Count;
            image.sprite = frames[currentFrame];
            timer -= frameRate;
        }
    }

    public List<Sprite> Frames
    {
        get { return frames; }
    }
}
