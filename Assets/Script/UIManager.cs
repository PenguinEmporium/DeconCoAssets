using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public Animator mainMenu;
    public Text highscoreText;

    public TextMesh livesText;

    public TextMesh[] highscoreDigits;
    public TextMesh[] digits;
    public Animator[] spriteDigits;

    //Height goes from 0 - 0.84  Or  -0.4238 to -0.0021
    public SpriteRenderer[] bombAmmoCount;

    public bool processingScore;
    public bool processingHighscore;
    Coroutine processingCoroutine;
    Coroutine processingHighscoreCoroutine;

    public GameObject quitButton;

    public GameObject statusBox;
    public TextMesh statusText;

    public AudioSource uiAudio;

    public void Awake()
    {

        #if UNITY_WEBGL
            quitButton.SetActive(false);
#endif
    }

    //PercentageGoes from 0-300, 100 for each bomb
    public void UpdateBombProgress(float percentage)
    {
        if (percentage < 100)
        {
            bombAmmoCount[0].size = new Vector2(1f,(percentage/100f)*0.84f);
            bombAmmoCount[1].size = new Vector2(1f,0f);
            bombAmmoCount[2].size = new Vector2(1f, 0f);
        }
        else if (percentage < 200)
        {
            bombAmmoCount[0].size = new Vector2(1f, 0.84f);
            bombAmmoCount[1].size = new Vector2(1f, ((percentage - 100f) / 100f) * 0.84f);
            bombAmmoCount[2].size = new Vector2(1f, 0f);
        }
        else if (percentage < 300)
        {
            bombAmmoCount[0].size = new Vector2(1f, 0.84f);
            bombAmmoCount[1].size = new Vector2(1f, 0.84f);
            bombAmmoCount[2].size = new Vector2(1f, ((percentage - 200f) / 100f) * 0.84f);
        }
        else
        {
            bombAmmoCount[0].size = new Vector2(1f, 0.84f);
            bombAmmoCount[1].size = new Vector2(1f, 0.84f);
            bombAmmoCount[2].size = new Vector2(1f, 0.84f);
        }
    }

    public void SetIfThinkingAboutScore(bool state)
    {
        foreach(Animator i in spriteDigits)
        {
            i.SetBool("Flip", state);
            if(state)
            {
                i.GetComponent<SpriteRenderer>().sortingLayerName = "Gameplay";
            }
            else
            {
                i.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
            }
        }
    }

    public void SetStatus(bool status, string message, float boxWidth, AudioClip noticeClip)
    {
        if(boxWidth == 0)
        {
            statusBox.GetComponent<SpriteRenderer>().size = new Vector2(3f, 0.8f);
        }
        else
        {
            statusBox.GetComponent<SpriteRenderer>().size = new Vector2(boxWidth, 0.8f);
        }
        statusBox.SetActive(status);
        statusText.text = message;

        if (noticeClip != null)
            uiAudio.PlayOneShot(noticeClip);
    }

    public void SetScore(int score)
    {
        if(processingCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
        }
        processingCoroutine = StartCoroutine(UpdateScore(score));
    }

    IEnumerator UpdateScore(int score)
    {
        processingScore = true;
        int[] currentDigits = GetIntArray(score);

        SetIfThinkingAboutScore(true);

        for(int i = 0; i < digits.Length; i++)
        {
            if (i < currentDigits.Length)
            {
                digits[i].text = currentDigits[i].ToString();
            }
            else
            {
                digits[i].text = "0";
            }
        }

        yield return new WaitForSeconds(0.2f);
        SetIfThinkingAboutScore(false);
        processingScore = false;
    }

    public void SetHighscore(int score)
    {
        if (processingHighscoreCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
        }
        processingHighscoreCoroutine = StartCoroutine(UpdateHighScore(score));
    }

    IEnumerator UpdateHighScore(int score)
    {
        processingHighscore = true;
        int[] currentDigits = GetIntArray(score);

        SetIfThinkingAboutScore(true);

        for(int i = 0; i < highscoreDigits.Length; i++)
        {
            if (i < currentDigits.Length)
            {
                highscoreDigits[i].text = currentDigits[i].ToString();
            }
            else
            {
                highscoreDigits[i].text = "0";
            }
        }

        yield return new WaitForSeconds(0.2f);
        SetIfThinkingAboutScore(false);
        processingHighscore = false;
    }

    public void SetLives(int lives)
    {
        livesText.text = lives.ToString();
    }

    public void SetTutorial(bool state)
    {
        tutorialPanel.SetActive(state);
    }

    public void SetMenu(bool state)
    {
        mainMenu.SetBool("Remove", !state);
    }

    public void UpdateHighscore(int highScore)
    {
        highscoreText.text = highScore.ToString();
    }

    int[] GetIntArray(int num)
    {
        List<int> listOfInts = new List<int>();
        /*for (int i = 0; i < 6; i++)
        {
            switch (i)
            {
                case 0:
                    listOfInts.Add(Mathf.RoundToInt(num / 100000));
                    break;
                case 1:
                    listOfInts.Add(Mathf.RoundToInt(num / 10000));
                    break;
                case 2:
                    listOfInts.Add(Mathf.RoundToInt(num / 1000));
                    break;
                case 3:
                    listOfInts.Add(Mathf.RoundToInt(num / 100));
                    break;
                case 4:
                    listOfInts.Add(Mathf.RoundToInt(num / 10));
                    break;
                case 5:
                    listOfInts.Add(Mathf.RoundToInt(num / 1));
                    break;
            }
        }*/
        int temp = 0;

        while (num != 0)
        {
            temp = num % 10;

            //here you will get its element one by one but in reverse order
            //you can perform your action here.
            listOfInts.Add(temp);

            num /= 10;
        }

        //listOfInts.Reverse();
        return listOfInts.ToArray();
    }
}
