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

    [Header("Animation Settings")]
    public string attackAnimationName = "AttackAnim";

    private int currentPlayer = 1;
    private bool isAnimating = false;

    // Animatori pentru playeri
    private Animator player1Animator;
    private Animator player2Animator;

    void Start()
    {
        if (player1 == null || player2 == null || turnText == null)
        {
            Debug.LogError("Please assign both players and the TextMeshPro text in the Inspector!");
            enabled = false;
            return;
        }

        // Obține Animator-ul fiecărui player (dacă există)
        player1Animator = player1.GetComponent<Animator>();
        player2Animator = player2.GetComponent<Animator>();

        // Verifică și avertizează pentru animator-ii lipsă
        if (player1Animator == null)
            Debug.LogWarning("⚠️ Player 1 nu are Animator - atacurile nu vor avea animație!");
        else
            player1Animator.SetBool("Attack", false);

        if (player2Animator == null)
            Debug.LogWarning("⚠️ Player 2 nu are Animator - atacurile nu vor avea animație!");
        else
            player2Animator.SetBool("Attack", false);

        // Alege random primul jucător
        currentPlayer = Random.Range(1, 3);

        // DOAR afișează textul, fără animație
        UpdateTurnText();
        Debug.Log($"Jocul începe! Player {currentPlayer} atacă primul. Apasă butonul pentru a ataca!");
    }

    public void SwitchTurn()
    {
        if (isAnimating)
        {
            Debug.Log("Așteaptă să se termine animația!");
            return;
        }

        // Pornește corutina care face totul în ordine
        StartCoroutine(ExecuteTurnSequence());
    }

    private IEnumerator ExecuteTurnSequence()
    {
        isAnimating = true;

        // 1. Joacă animația de atac
        Animator currentAnimator = (currentPlayer == 1) ? player1Animator : player2Animator;
        yield return StartCoroutine(PlayAttackAnimation(currentAnimator));

        // 2. Aplică damage-ul DUPĂ animație
        AttackCurrentPlayer();

        // 3. Verifică dacă jocul s-a terminat
        if (CheckGameOver())
        {
            isAnimating = false;
            yield break; // Oprește aici dacă jocul s-a terminat
        }

        // 4. Schimbă jucătorul
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        UpdateTurnText();

        isAnimating = false;
        Debug.Log($"Gata! Acum e rândul lui Player {currentPlayer}");
    }

    private IEnumerator PlayAttackAnimation(Animator animator)
    {
        // Dacă nu există animator, doar așteaptă puțin și continuă
        if (animator == null)
        {
            Debug.Log($"Player {currentPlayer} atacă (fără animație)");
            yield return new WaitForSeconds(0.5f); // Pauză scurtă pentru feedback vizual
            yield break;
        }

        // Găsește durata animației
        float animationLength = GetAnimationLength(animator, attackAnimationName);

        if (animationLength <= 0)
        {
            Debug.LogWarning($"Animația '{attackAnimationName}' nu a fost găsită! Folosesc 1 secundă.");
            animationLength = 1f;
        }

        Debug.Log($"Player {currentPlayer} atacă! Animație durată: {animationLength}s");

        // Activează animația
        animator.SetBool("Attack", true);

        // Așteaptă să se termine animația
        yield return new WaitForSeconds(animationLength);

        // IMPORTANT: Resetează Attack la false
        animator.SetBool("Attack", false);

        Debug.Log("Animație terminată!");
    }

    private float GetAnimationLength(Animator animator, string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return 0f;
        }

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in clips)
        {
            if (clip.name == clipName)
            {
                Debug.Log($"✓ Animația '{clipName}' găsită! Durată: {clip.length}s");
                return clip.length;
            }
        }

        return 0f;
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

            Debug.Log($"💥 Player {currentPlayer} a atacat cu {damage} damage! HP: {hp} → {newHP}");
        }
        else
        {
            Debug.LogWarning("Could not parse damage or hit points as integers!");
        }
    }

    private bool CheckGameOver()
    {
        GameObject target = (currentPlayer == 1) ? player2 : player1;
        Transform targetStats = target.transform.Find("Stats");

        if (targetStats == null) return false;

        TextMeshPro hitPointsText = targetStats.Find("HitPoints")?.GetComponent<TextMeshPro>();

        if (hitPointsText != null && int.TryParse(hitPointsText.text, out int hp))
        {
            if (hp <= 0)
            {
                Debug.Log($"🏆 PLAYER {currentPlayer} CÂȘTIGĂ! 🏆");
                turnText.text = $"Player {currentPlayer} WINS!";
                enabled = false; // Dezactivează scriptul
                return true;
            }
        }

        return false;
    }

    private void UpdateTurnText()
    {
        turnText.text = $"Player {currentPlayer}'s Turn!";
    }
}