using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterInfo: MonoBehaviour
{
    [SerializeField]
     Text name; // Character's name field

    [SerializeField]
     Sprite image; // Character's image field

    //Constructor for CharacterInfo
    public CharacterInfo(Text characterName, Sprite characterImage)
    {
        name = characterName;
        image = characterImage;
    }

    // Getter for the character's name
    public Text Name
    {
        get { return name; }
    }

    // Getter for the character's image
    public Sprite Image
    {
        get { return image; }
    }
}