using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Totem : MonoBehaviour
{
    private static List<Totem> weakTotems;
    private static List<Totem> strongTotems;

    public static void ResetAllTotems()
    {
        if(weakTotems != null)
        {
            foreach (Totem t in weakTotems)
            {
                t.ResetSelectedPlayer();
            }
        }
        if (strongTotems != null)
        {
            foreach (Totem t in strongTotems)
            {
                t.ResetSelectedPlayer();
            }
        }
    }

    [SerializeField] private List<GameObject> playersInFront;
    [SerializeField] private List<CustomPlayerInput> playersInFrontInput;
    private GameObject selectedPlayer;
    private ParticleSystemRenderer fireRenderer;
    private bool isLerpingFireColor = false;

    [SerializeField] private TotemType totemType;
    [SerializeField] private float lerpColorDuration = 1f;
    [SerializeField] private Vector2 collisionOffset = Vector2.zero, collisionSize = new Vector2(1.5f, 2f);
    [SerializeField] private float interactDuration = 1f;
    [SerializeField] private LayerMask playersMask;

    private enum TotemType
    {
        WeakTotem,
        StrongTotem
    }

    private void Awake()
    {
        fireRenderer = GetComponentInChildren<ParticleSystem>().GetComponent<ParticleSystemRenderer>();

        switch (totemType)
        {
            case TotemType.WeakTotem:
                if(weakTotems == null)
                    weakTotems = new List<Totem>();
                weakTotems.Add(this);
                break;
            case TotemType.StrongTotem:
                if (strongTotems == null)
                    strongTotems = new List<Totem>();
                strongTotems.Add(this);
                break;
            default:
                break;
        }
    }

    private void Start()
    {
        playersInFront = new List<GameObject>();
        playersInFrontInput = new List<CustomPlayerInput>();
        InvokeRepeating(nameof(UpdatePlayersInFront), 0f, 1f / 20f);
        selectedPlayer = null;
    }

    private void Update()
    {
        for (int i = 0; i < playersInFront.Count; i++)
        {
            if(playersInFrontInput[i].upPressedDown)
            {
                if(selectedPlayer == null || selectedPlayer.GetComponent<PlayerCommon>().charIndex != playersInFront[i].GetComponent<PlayerCommon>().charIndex)
                {
                    StartCoroutine(ActivateTotem(playersInFront[i]));
                    break;
                }
            }
        }
    }

    private void UpdatePlayersInFront()
    {
        playersInFront.Clear();
        playersInFrontInput.Clear();
        Collider2D[] cols = Physics2D.OverlapBoxAll((Vector2)transform.position + collisionOffset, collisionSize, 0f, playersMask);
        foreach (Collider2D col in cols)
        {
            if(col.GetComponent<Movement>().isGrounded)
            {
                playersInFront.Add(col.gameObject);
                playersInFrontInput.Add(col.gameObject.GetComponent<CustomPlayerInput>());
            }
        }
    }

    private void ResetSelectedPlayer()
    {
        selectedPlayer = null;
        playersInFront.Clear();
        playersInFrontInput.Clear();
        fireRenderer.material.SetColor("_EmissionColor", Color.white);
    }

    private bool IsOtherTotemContain(in PlayerCommon.CharactersIndex charIndex, in TotemType totemType)
    {
        List<Totem> lstTotem = null;
        switch (totemType)
        {
            case TotemType.WeakTotem:
                lstTotem = weakTotems;
                break;
            case TotemType.StrongTotem:
                lstTotem = strongTotems;
                break;
            default:
                break;
        }

        foreach (Totem t in lstTotem)
        {
            if (t.selectedPlayer != null && t.selectedPlayer.GetComponent<PlayerCommon>().charIndex == charIndex)
                return true;
        }
        return false;
    }

    private IEnumerator ActivateTotem(GameObject player)
    {
        player.GetComponent<Movement>().Freeze();
        yield return Useful.GetWaitForSeconds(interactDuration);
        switch (totemType)
        {
            case TotemType.WeakTotem:
                player.GetComponent<FightController>().enableAttackWeak = true;
                if (selectedPlayer != null)
                {
                    selectedPlayer.GetComponent<FightController>().enableAttackWeak =
                        IsOtherTotemContain(selectedPlayer.GetComponent<PlayerCommon>().charIndex, totemType);
                }
                break;
            case TotemType.StrongTotem:
                player.GetComponent<FightController>().enableAttackStrong = true;
                if (selectedPlayer != null)
                {
                    selectedPlayer.GetComponent<FightController>().enableAttackStrong =
                        IsOtherTotemContain(selectedPlayer.GetComponent<PlayerCommon>().charIndex, totemType);
                }
                break;
            default:
                break;
        }
        selectedPlayer = player;

        Color color = selectedPlayer.GetComponent<PlayerCommon>().color;

        if(isLerpingFireColor)
            StopCoroutine(nameof(LerpFireColor));
        StartCoroutine(LerpFireColor(fireRenderer.material.GetColor("_EmissionColor"), color));

        player.GetComponent<Movement>().UnFreeze();
    }

    private IEnumerator LerpFireColor(Color start, Color end)
    {
        isLerpingFireColor = true;
        float time = Time.time;
        Color c = start;
        float t = 0f;
        while(Time.time - time < lerpColorDuration)
        {
            t = (Time.time - time) / lerpColorDuration;
            c = Color.Lerp(start, end, t);
            fireRenderer.material.color = c;
            fireRenderer.material.SetColor("_EmissionColor", c);
            yield return null;
        }
        isLerpingFireColor = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + collisionOffset, collisionSize);
    }
}
