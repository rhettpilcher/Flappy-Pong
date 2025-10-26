using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using HutongGames.PlayMaker;

public class Collectible : MonoBehaviour
{
    private bool collected;
    private bool coin;
    private bool powerUp;
    private enum PowerUps
    {
        Invincibility,
        Shield,
        CoinTrail,
        Slowmo
    }
    private PowerUps powerUpType;
    private GameController gameController;
    public SpriteRenderer sprite;
    public ParticleSystem particles;
    public Rigidbody2D rb;
    private int randomChoice;

    private void Start()
    {
        gameController = GameObject.Find("Game Controller").GetComponent<GameController>();
        gameObject.SetActive(false);
    }

    public void SetCoin()
    {
        coin = true;
        powerUp = false;
        transform.localScale = new Vector3(2f, 2f, 1);
        sprite.sprite = gameController.coinSprite;
        sprite.color = Color.white;
        particles.startColor = Color.yellow;
        collected = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void SetPowerUp()
    {
        powerUp = true;
        coin = false;
        transform.localScale = new Vector3(2f, 2f, 1);
        randomChoice = Random.Range(0, 4);
        sprite.sprite = gameController.powerUpSprites[randomChoice];
        sprite.color = Color.white;
        collected = false;
        rb.bodyType = RigidbodyType2D.Kinematic;

        switch (randomChoice)
        {
            case 0:
                sprite.sprite = gameController.powerUpSprites[randomChoice];
                sprite.color = Color.yellow;
                powerUpType = PowerUps.Invincibility;
                break;

            case 1:
                if (PlayerPrefs.GetInt("Power-Ups1unlocked") == 1)
                {
                    sprite.sprite = gameController.powerUpSprites[randomChoice];
                    sprite.color = Color.blue;
                    particles.startColor = Color.blue;
                    powerUpType = PowerUps.Shield;
                }
                else
                    SetCoin();
                break;

            case 2:
                sprite.sprite = gameController.powerUpSprites[randomChoice];
                if (PlayerPrefs.GetInt("Power-Ups2unlocked") == 1)
                {
                    sprite.color = Color.green;
                    particles.startColor = Color.green;
                    powerUpType = PowerUps.CoinTrail;
                }
                else
                    SetCoin();
                break;

            case 3:
                sprite.sprite = gameController.powerUpSprites[randomChoice];
                if (PlayerPrefs.GetInt("Power-Ups3unlocked") == 1)
                {

                    sprite.color = Color.magenta;
                    particles.startColor = Color.magenta;
                    powerUpType = PowerUps.Slowmo;
                }
                else
                    SetCoin();
                break;
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collected)
        {
            collected = true;
            if (coin)
            {
                gameController.PlaySound(gameController.coinCollect, 1, 1);
                gameController.coins++;
                gameController.coinsThisGame++;
            }
            if (powerUp)
            {
                gameController.powerUpsCollected++;

                gameController.PlaySound(gameController.powerUp, 1, 1);
                if (powerUpType == PowerUps.Invincibility)
                    gameController.SetInvincible(5);
                else if (powerUpType == PowerUps.Shield)
                {
                    if (gameController.shields < 3)
                    {
                        gameController.shields++;
                        gameController.PlaySound(gameController.shieldUp, 1, 1);
                    }
                }
                else if (powerUpType == PowerUps.CoinTrail)
                    gameController.StartCoroutine(gameController.CoinTrail(5));
                else if (powerUpType == PowerUps.Slowmo)
                {
                    gameController.FindFSM(gameController.gameObject, "Slowmo").SendEvent("global event");
                }
            }

            particles.Play();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.AddForce(new Vector2(Random.Range(-1, 1), 5), ForceMode2D.Impulse);

            DOTween.ToAlpha(() => sprite.color, x => sprite.color = x, 0, .75f)
            .OnComplete(() => gameObject.SetActive(false));
        }    
    }
}
