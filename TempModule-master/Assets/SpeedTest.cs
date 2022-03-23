using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class SpeedTest : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;
    public Material buttonMat; // Used for the button material that should change into a color.
    public KMSelectable button;
    public Material back;

    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;
    private static Color[] color = { Color.red, Color.green, Color.blue, Color.yellow, Color.cyan, Color.magenta, Color.white, Color.black };
    private int timeInS;
    private int presses = 0;
    private int pressesOnButton = 0;
    private int initialTime = 0;

    /// <summary>
    /// (cosmetic) Used to cycle the colors of the button. Should only be called once per strike with new colors.
    /// </summary>
    /// <param name="colors"></param>
    /// <returns></returns>
    IEnumerator cycleColors(Color[] colors)
    {
        yield return null;
        int i = 0;
        while (i < colors.Length)
        {
            buttonMat.color = colors[i];
            i++;
            if (i == colors.Length) i = 0;
            yield return new WaitForSeconds(0.3f);
        }
    }
    /// <summary>
    /// Counts initialTime up to timeInS, when it hits timeInS, the module strikes.
    /// </summary>
    /// <returns></returns>
    IEnumerator secondsCounting()
    {
        yield return null;
        while (true)
        {
            initialTime++;
            if (initialTime >= timeInS)
            {
                initialTime = 0;
                Module.HandleStrike();
                yield break;
            }
            yield return new WaitForSeconds(1f);
        }
    }
    /// <summary>
    /// (after solve) Cycles the color of the background material.
    /// </summary>
    /// <param name="colors"></param>
    /// <returns></returns>
    IEnumerator cycleBackColors(Color[] colors)
    {
        yield return null;
        int i = 0;
        while (true)
        {
            back.color = colors[i];
            i++;
            if (i >= colors.Length) i = 0;
            yield return new WaitForSeconds(0.01f);
        }
    }
    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        back.color = Color.white;
        Color[] randomColors = new Color[10];
        for (int i = 0; i < 10; i++)
        {
            int randValue = Rnd.Range(0, color.Length - 1);
            Color randColor = color[randValue]; // Used to get a random color used for the amount of presses.
            randomColors[i] = randColor;
            Debug.LogFormat("[Speed Test #{0}] Color {1}: {2}", _moduleId, i+1, randColor);
        }
        StartCoroutine(cycleColors(randomColors));
        presses = calculatePresses(randomColors);
        button.OnInteract += delegate
        {
            if (initialTime == 0)
            {
                StopAllCoroutines();
                buttonMat.color = Color.white;
                StartCoroutine(secondsCounting());
            }
            pressesOnButton++;
            if (pressesOnButton >= presses)
            {
                Module.HandlePass();
                button.gameObject.SetActive(false);
                StopAllCoroutines();
                StartCoroutine(cycleBackColors(randomColors));
            }
            Debug.Log("Current press: " + pressesOnButton);
            return false;

        };
    }
    /// <summary>
    /// Used for calculating the presses. Should only reset on strike.
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    private int calculatePresses(Color[] colors)
    {
        // The calculation: Number of R values cycled * Number of G values cycled * Number of B values Cycled, (1 if 0).
        int red = 0;
        int green = 0;
        int blue = 0;
        foreach (Color color in colors)
        {
            if (color.r == 1) red++;
            if (color.g == 1) green++;
            if (color.b == 1) blue++;
        }
        int rgb = red * green * blue;
        timeInS = timeToTake(rgb);
        Debug.LogFormat("[Speed Test #{0}] Number of presses: {1}", _moduleId, rgb);
        return rgb;
    }
    /// <summary>
    /// Returns the time you need to do all of your presses. Resets on strike.
    /// </summary>
    /// <param name="presses"></param>
    /// <returns></returns>
    private int timeToTake(int presses)
    {
        Debug.LogFormat("[Speed Test #{0}] Time to take: {1}", _moduleId, presses/5);
        return presses / 5;
    }
}
