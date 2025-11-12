using UnityEngine;
using TMPro;
using System.Collections;

public class TurnManager : MonoBehaviour
{
    [Header("Player Objects")]
    public GameObject player1;
    public GameObject player2;

    [Header("UI Elements")]
    public TextMeshProUGUI turnText;

    private int currentPlayer; 
    private bool isAnimating = false;

    void Start()
    {
        if (player1 == null || player2 == null || turnText == null)
        {
            Debug.LogError("Please assign both players and the TextMeshPro text in the Inspector!");
            enabled = false;
            return;
        }

        currentPlayer = Random.Range(1, 3); 
        UpdateTurnText();
        HighlightCurrentPlayer();
    }

    public void SwitchTurn()
    {
        if (isAnimating) return; 

        AttackCurrentPlayer();

        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        UpdateTurnText();
        HighlightCurrentPlayer();
    }

    private void AttackCurrentPlayer()
    {
        GameObject attacker = (currentPlayer == 1) ? player1 : player2;
        GameObject target = (currentPlayer == 1) ? player2 : player1;

        Transform attackerStats = attacker.transform.Find("Stats");
        Transform targetStats = target.transform.Find("Stats");

        if (attackerStats == null || targetStats == null)
        {
            Debug.LogError("Both players must have a 'Stats' child with HitPoints and Atack Damage texts!");
            return;
        }

        TextMeshPro damageText = attackerStats.Find("Atack Damage")?.GetComponent<TextMeshPro>();
        TextMeshPro hitPointsText = targetStats.Find("HitPoints")?.GetComponent<TextMeshPro>();

        if (damageText == null || hitPointsText == null)
        {
            Debug.LogError("Missing TextMeshPro components on 'Atack Damage' or 'HitPoints' objects!");
            return;
        }

        if (int.TryParse(damageText.text, out int damage) && int.TryParse(hitPointsText.text, out int hp))
        {
            int newHP = Mathf.Max(hp - damage, 0);
            hitPointsText.text = newHP.ToString();

            Debug.Log($"Player {currentPlayer} attacked! Target HP: {hp} → {newHP}");
        }
        else
        {
            Debug.LogWarning("Could not parse damage or hit points as integers!");
        }
    }

    private void UpdateTurnText()
    {
        turnText.text = $"Player {currentPlayer}'s Turn!";
    }

    private void HighlightCurrentPlayer()
    {
        if (player1 != null && player2 != null)
        {
            if (currentPlayer == 1)
                StartCoroutine(Start_Attack_Animation_P2());
            else
                StartCoroutine(Start_Attack_Animation_P1());
        }
    }

    private IEnumerator Start_Attack_Animation_P1()
    {
        isAnimating = true;
        Vector3 originalPos = player1.transform.position;
        Vector3 attackPos = originalPos + new Vector3(0.6f, 0, 0);

        float elapsed = 0f;
        float duration = 0.1f;
        while (elapsed < duration)
        {
            player1.transform.position = Vector3.Lerp(originalPos, attackPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player1.transform.position = attackPos;

        yield return new WaitForSeconds(0.1f);

        elapsed = 0f;
        while (elapsed < duration)
        {
            player1.transform.position = Vector3.Lerp(attackPos, originalPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player1.transform.position = originalPos;

        isAnimating = false;
    }

    private IEnumerator Start_Attack_Animation_P2()
    {
        isAnimating = true;
        Vector3 originalPos = player2.transform.position;
        Vector3 attackPos = originalPos - new Vector3(0.6f, 0, 0);

        float elapsed = 0f;
        float duration = 0.1f;
        while (elapsed < duration)
        {
            player2.transform.position = Vector3.Lerp(originalPos, attackPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player2.transform.position = attackPos;

        yield return new WaitForSeconds(0.1f);

        elapsed = 0f;
        while (elapsed < duration)
        {
            player2.transform.position = Vector3.Lerp(attackPos, originalPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        player2.transform.position = originalPos;

        isAnimating = false;
    }
}
