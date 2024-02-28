using System.Collections;
using UnityEngine;

public class TMP : MonoBehaviour
{
    private bool isRunning;
    private Movement movement;
    private CustomPlayerInput playerInput;

    [SerializeField] private InputKey startStop = InputKey.Z;
    [SerializeField] private float speed = 1f;

    private void Start()
    {
        movement = GetComponent<Movement>();
        playerInput = GetComponent<CustomPlayerInput>();
    }

    private void Update()
    {
        if(!isRunning && InputManager.GetKeyDown(startStop))
        {
            isRunning = true;
            StartCoroutine(Run());
        }
    }

    private IEnumerator Run()
    {
        movement.Freeze();

        while(true)
        {
            yield return null;
            if (InputManager.GetKeyDown(startStop))
            {
                isRunning = false;
                break;
            }

            movement.Teleport((Vector2)transform.position + playerInput.x * speed * Time.deltaTime * Vector2.right + playerInput.y * speed * Time.deltaTime * Vector2.up);
        }

        movement.UnFreeze();
    }
}
