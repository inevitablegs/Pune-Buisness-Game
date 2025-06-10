using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Dice : MonoBehaviour
{
    public Sprite[] diceFaces;
    public Image diceImage;
    public GameManager gameManager;

    private bool isActive = true;

    public void SetDiceActive(bool active)
    {
        isActive = active;
        GetComponent<Button>().interactable = active;
    }

    public void RollDice()
    {
        if (!isActive) return;
        StartCoroutine(RollAnimation());
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

        gameManager.OnDiceRolled(diceValue);
    }
}
