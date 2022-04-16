using System;
using System.Collections;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;



public class SpeedTest : MonoBehaviour
{
    public enum ColorName
    {
        Red,
        Green,
        Blue,
        Yellow,
        Cyan,
        Magenta,
        White,
        Black
    }

    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;
    public Material ButtonMat; // Used for the button material that should change into a color.
    public MeshRenderer ButtonMesh;
    public KMSelectable Button;
    public Material Back;
    public MeshRenderer BackMesh;

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
    IEnumerator cycleColors(Color[] colors, Material mat)
    {
        yield return null;
        int i = 0;
        while (i < colors.Length)
        {
            mat.color = colors[i];
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
    IEnumerator cycleBackColors(Color[] colors, Material mat)
    {
        yield return null;
        int i = 0;
        while (true)
        {
            mat.color = colors[i];
            i++;
            if (i >= colors.Length) i = 0;
            yield return new WaitForSeconds(0.75f);
        }
    }
    public void justForColorLogging(int pos, int color)
    {
        switch (color)
        {
            case 0:
                Debug.LogFormat("[Speed Test #{0}] Color {1}: {2}", _moduleId, pos + 1, ColorName.Red);
                break;
            case 1:
                Debug.LogFormat("[Speed Test #{0}] Color {1}: {2}", _moduleId, pos + 1, ColorName.Green);
                break;
            case 2:
                Debug.LogFormat("[Speed Test #{0}] Color {1}: {2}", _moduleId, pos + 1, ColorName.Blue);
                break;
            case 3:
                Debug.LogFormat("[Speed Test #{0}] Color {1}: {2}", _moduleId, pos + 1, ColorName.Yellow);
                break;
            case 4:
                Debug.LogFormat("[Speed Test #{0}] Color {1}: {2}", _moduleId, pos + 1, ColorName.Cyan);
                break;
            case 5:
                Debug.LogFormat("[Speed Test #{0}] Color {1}: {2}", _moduleId, pos + 1, ColorName.Magenta);
                break;
            case 6:
                Debug.LogFormat("[Speed Test #{0}] Color {1}: {2}", _moduleId, pos + 1, ColorName.White);
                break;
            case 7:
                Debug.LogFormat("[Speed Test #{0}] Color {1}: {2}", _moduleId, pos + 1, ColorName.Black);
                break;
        }
    }

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        Back.color = Color.white;
        Material tempMat = new Material(ButtonMat);
        ButtonMesh.material = tempMat;
        Material tempBack = new Material(Back);
        BackMesh.material = tempBack;
        Color[] randomColors = new Color[10];
        for (int i = 0; i < 10; i++)
        {
            int randValue = Rnd.Range(0, color.Length - 1);
            Color randColor = color[randValue]; // Used to get a random color used for the amount of presses.
            justForColorLogging(i, randValue);
            randomColors[i] = randColor;
            
        }
        StartCoroutine(cycleColors(randomColors, tempMat));
        presses = calculatePresses(randomColors);
        Button.OnInteract += delegate
        {
            if (initialTime == 0)
            {
                StopAllCoroutines();
                tempMat.color = Color.white;
                StartCoroutine(secondsCounting());
            }
            pressesOnButton++;
            if (pressesOnButton >= presses)
            {
                Module.HandlePass();
                Button.gameObject.SetActive(false);
                StopAllCoroutines();
                StartCoroutine(cycleBackColors(randomColors, tempBack));
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
            
            if (Math.Round(color.r) == 1) red++;
            if (Math.Round(color.g) == 1) green++;
            if (Math.Round(color.b) == 1) blue++;
        }
        if (red == 0) red++;
        if (green == 0) green++;
        if (blue == 0) blue++;
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
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} press # (to press the button a number of times. Bounded from answer to answer + 100)";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        string[] param = command.Split(' ');
        if (Regex.IsMatch(command, @"^\s*(press)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            int a;
            int.TryParse(param[1], out a);
            if (a <= presses+100)
            {
                Button.OnInteract();
                yield return new WaitForSeconds(0.01f);
            }
            yield return "sendtochaterror Too high or too low of a number.";
            yield break;
        }
        yield return "sendtochaterror Invalid command. Use help to see the commands.";
        yield break;
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        int answer = 0;
        while (answer < presses)
        {
            Button.OnInteract();
            answer++;
            yield return new WaitForSeconds(0.01f);
        }
    }
}
