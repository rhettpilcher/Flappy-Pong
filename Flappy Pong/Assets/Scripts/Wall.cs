using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Destructible2D;

public class Wall : MonoBehaviour
{
    public bool isBounce;
    public bool hasGem;
    public Color bounceColor;
    public Color gemColor;
    private GameController gameController;
    public SpriteRenderer sprite;
    public GameObject wallBreakPrefab;

    private void Start()
    {
        gameController = GameObject.Find("Game Controller").GetComponent<GameController>();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (Random.value < 0.1f) // 10% chance to be bounce
        {
            sprite.color = bounceColor;
            transform.localScale = new Vector2(1, 1);
            isBounce = true;
            hasGem = false;
        }
        else if (gameController != null && gameController.activeCharm == 2 && Random.value < 0.01f) // 0.9% chance to be gem
        {
            sprite.color = gemColor;
            transform.localScale = new Vector2(.2f, .75f);
            hasGem = true;
            isBounce = false;
        }
        else
        {
            sprite.color = Color.gray;
            transform.localScale = new Vector2(1, 1);
            isBounce = false;
            hasGem = false;
        }
    }

    /*public void Init()
    {
        if (Random.value < 0.1f) // 10% chance to be bounce
        {
            sprite.color = bounceColor;
            transform.localScale = new Vector2(1, 1);
            isBounce = true;
            hasGem = false;
        }
        else if (gameController.activeCharm == 2 && Random.value < 0.01f) // 0.9% chance to be gem
        {
            sprite.color = gemColor;
            transform.localScale = new Vector2(.2f, .75f);
            hasGem = true;
            isBounce = false;
        }
        else
        {
            sprite.color = Color.gray;
            transform.localScale = new Vector2(1, 1);
            isBounce = false;
            hasGem = false;
        }
    }*/

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameController.Reuse(gameController.wallBreakPool, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }
}
