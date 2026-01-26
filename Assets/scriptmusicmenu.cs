using UnityEngine;

public class scriptmusicmenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        AudioManager.Instance?.PlayMenuMusic();


    }

    // Update is called once per frame
    void Update()
    {

    }
}