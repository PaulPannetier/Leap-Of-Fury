using UnityEngine;

public class Fiole : MonoBehaviour
{
    public PlayerCommon playerCommon;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Char"))
        {
            GameObject player = collision.GetComponent<ToricObject>().original;
            if(playerCommon.id == player.GetComponent<PlayerCommon>().id)
            {
                BounceShotAttack attack = player.GetComponent<BounceShotAttack>();
                if (attack != null)
                {
                    attack.PickUpFiole(this);
                }
                else
                {
                    BouncingBallAttack attack2 = player.GetComponent<BouncingBallAttack>();
                    if (attack2 != null)
                        attack2.PickUpFiole(this);
                }
                Destroy(gameObject);
            }
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
