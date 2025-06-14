using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;

public class Dice : MonoBehaviour
{
    public Sprite[] diceFaces;
    public Image diceImage;
    public GameManager gameManager;

    private bool isActive = true;

    public GameObject diceButton; // assign in inspector

    public void SetDiceActive(bool value)
    {
        diceButton.SetActive(value);
    }

    public void RollDice()
    {
        if (!isActive) return;
        StartCoroutine(RollAnimation());
    }

    [PunRPC]
    public void EnableDice(bool active)
    {
        isActive = active;
        diceButton.SetActive(active);
    }

    IEnumerator RollAnimation()
    {
        for (int i = 0; i < 10; i++)
        {
            int face = Random.Range(0, 6);
            diceImage.sprite = diceFaces[face];
            yield return new WaitForSeconds(0.05f);
        }

        int diceValue = Random.Range(1, 7);
        diceImage.sprite = diceFaces[diceValue - 1];

        if (gameManager != null)
        {
            gameManager.OnDiceRolled(diceValue);
        }
        else
        {
            Debug.LogError("❌ Dice has no reference to GameManager!");
        }

    }
}
