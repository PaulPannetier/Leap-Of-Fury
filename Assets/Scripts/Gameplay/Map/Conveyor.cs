using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    private List<GameObject> playersInCollider;
    private List<Rigidbody2D> playersRb;

    [SerializeField] private float length, heigth, rotationSpeed = 250f, colliderMarges = 1.05f;
    [SerializeField] private float maxSpeed = 3f, accel = 1f;
    [SerializeField] private bool invert = false;

    [SerializeField] private GameObject cube, circleLeft, circleRight;
    [SerializeField] private CapsuleCollider2D capsuleCollider;

    private void Awake()
    {
        playersInCollider = new List<GameObject>();
        playersRb = new List<Rigidbody2D>();
    }

    private void Update()
    {
        float sign = invert ? -1f : 1f;
         circleLeft.transform.Rotate(Vector3.forward * (-sign * rotationSpeed * Time.deltaTime));
        circleRight.transform.Rotate(Vector3.forward * (-sign * rotationSpeed * Time.deltaTime));

        for (int i = 0; i < playersInCollider.Count; i++)
        {
            float velX = Mathf.Clamp(playersRb[i].velocity.x + sign * accel * Time.deltaTime, -maxSpeed, maxSpeed);
            playersRb[i].velocity = new Vector2(velX, playersRb[i].velocity.y);
        }
    }

    private void GenerateConveyor()
    {
        Vector3 angle = transform.rotation.eulerAngles;
        transform.Rotate(-transform.rotation.eulerAngles);
        cube.transform.localScale = new Vector3(length, heigth, 1f);
        circleLeft.transform.localScale = new Vector3(heigth, heigth, 1f);
        circleRight.transform.localScale = new Vector3(heigth, heigth, 1f);
        circleLeft.transform.position = new Vector3(transform.position.x - length * 0.5f, transform.position.y, transform.position.z);
        circleRight.transform.position = new Vector3(transform.position.x + length * 0.5f, transform.position.y, transform.position.z);
        capsuleCollider.offset = Vector2.zero;
        capsuleCollider.size = new Vector2((length + heigth) * colliderMarges, heigth * colliderMarges);
        transform.Rotate(angle);
    }

    private void OnValidate()
    {
        length = Mathf.Max(0f, length);
        heigth = Mathf.Max(0f, heigth);
        GenerateConveyor();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            playersInCollider.Add(collision.gameObject);
            playersRb.Add(collision.gameObject.GetComponent<Rigidbody2D>());
            print("Enter : " + collision.gameObject.name);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playersInCollider.Remove(collision.gameObject);
            playersRb.Remove(collision.gameObject.GetComponent<Rigidbody2D>());
            print("Exit : " + collision.gameObject.name);
        }
    }
}
