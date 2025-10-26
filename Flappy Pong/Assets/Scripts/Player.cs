using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private GameController gameController;
    private bool touchCooldown;
    private Rigidbody2D rb;

    void Start()
    {
        gameController = FindAnyObjectByType<GameController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") && !touchCooldown) // wall
        {
            gameController.Vibrate();
            gameController.PlaySound(gameController.wallBreak, 1, Random.Range(0.5f, 1));
            gameController.combo++;
            gameController.wallsBroke++;
            gameController.direction = gameController.direction * -1;
            StartCoroutine(TouchCooldown(0.1f));
            if (collision.gameObject.GetComponent<Wall>().isBounce) // bounce wall
            {
                if (gameController.activeCharm == 1)
                    gameController.SetInvincible(2);
                gameController.PlaySound(gameController.bounce, 1, 1);
                gameController.Bounce();
                gameController.SetDisableJump(0.2f);
            }
            else if (collision.gameObject.GetComponent<Wall>().hasGem) // gem wall
            {
                gameController.gems++;
            }
        }
        else if (collision.gameObject.CompareTag("Hazard") && !gameController.invincible) // potential fail
        {
            gameController.Vibrate();
            if (gameController.shields > 0)
            {
                // has shield
                gameController.shields--;
                gameController.PlaySound(gameController.shieldDown, 1, 1);
                gameController.SetInvincible(3);
                gameController.direction = gameController.direction * -1;
                gameController.combo++;
                StartCoroutine(TouchCooldown(0.2f));
            }
            else if (gameController.activeCharm == 5 && Random.Range(0f, 1f) < 0.2f) // 20% survival chance if charm 5 active
                InvincibleHazard();
            else
            {
                // actually dead
                gameController.player.gameObject.layer = 7;
                rb.AddForce(new Vector2(-gameController.direction * 3, 1), ForceMode2D.Impulse);
                gameController.PlaySound(gameController.hitHazard, 1, 1);

                if (gameController.maxHeight >= 20 && gameController.revives < 3) // revive conditions, above 20m and less than 3 prev revives
                {
                    gameController.trail.GetComponent<PlayMakerFSM>().SendEvent("global off");
                    gameController.transform.GetChild(0).GetComponent<AudioSource>().Stop();
                    gameController.gameState = GameController.GameState.Revive;
                    gameController.fsmLink = 1;
                    gameController.clockActive = false;
                }
                else
                {
                    gameController.EndGame();
                    gameController.fsmLink = 2;
                }
            }
        }
        else if (collision.gameObject.CompareTag("Hazard") && gameController.invincible && !touchCooldown) // hazard invincible
        {
            gameController.Vibrate();
            InvincibleHazard();
        }
        else if (collision.gameObject.CompareTag("Combo")) // Combo: hazard bouncer, 
        {
            if (gameController.activeCharm == 1)
                gameController.SetInvincible(2);
            gameController.Vibrate();
            gameController.PlaySound(gameController.wallBreak, 1, Random.Range(0.5f, 1));
            gameController.PlaySound(gameController.bounce, 1, 1);
            gameController.combo++;
            StartCoroutine(TouchCooldown(0.1f));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Cannon") && !gameController.inCannon && gameController.gameState == GameController.GameState.Playing) // Cannon 
        {
            gameController.trail.GetComponent<PlayMakerFSM>().SendEvent("global off");
            gameController.cannon = collision.transform;
            gameController.inCannon = true;
            rb.bodyType = RigidbodyType2D.Static;
            transform.position = collision.transform.position;
            transform.parent = collision.transform;
        }
    }

    private IEnumerator TouchCooldown(float wait)
    {
        touchCooldown = true;
        yield return new WaitForSeconds(wait);
        touchCooldown = false;
    }

    private void InvincibleHazard()
    {
        gameController.PlaySound(gameController.hitHazardInvincible, 1, 1);
        gameController.invincibleHazards++;
        gameController.direction = gameController.direction * -1;
        //gameController.combo++;
        StartCoroutine(TouchCooldown(0.2f));
    }
}
