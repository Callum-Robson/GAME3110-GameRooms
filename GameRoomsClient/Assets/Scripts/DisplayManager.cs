using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayManager : MonoBehaviour
{
    [SerializeField]
    private GameObject textObject;
    private TMP_Text invalidInputMessageText;
    int charIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        invalidInputMessageText = textObject.GetComponent<TMP_Text>();
        while (charIndex != invalidInputMessageText.text.LastIndexOf(","))
        {
            // Get index of character.
            charIndex = invalidInputMessageText.text.IndexOf(",");
            // Replace text with color value for character.
            invalidInputMessageText.text = invalidInputMessageText.text.Replace(invalidInputMessageText.text[charIndex].ToString(), "<color=#000000>" + invalidInputMessageText.text[charIndex].ToString() + "</color>");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DisplayInvalidInputMessafe()
    {
        this.gameObject.SetActive(true);
    }

}
