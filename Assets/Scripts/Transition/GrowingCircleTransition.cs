using UnityEngine;

public class GrowingCircleTransition : MonoBehaviour
{
    private float speed = 3f;
    private float duration = 3f;
    private int nbCircles = 7;
    private Color circleColor = Color.black;

    private float timer;
    private GameObject[] circles;

    [SerializeField] private GameObject circlePrefabs;

    private void Awake()
    {
        //speed = (float)TransitionManager.instance.transitionParam[0];
        //duration = (float)TransitionManager.instance.transitionParam[1];
        //nbCircles = (int)TransitionManager.instance.transitionParam[2];
        //circleColor = (Color)TransitionManager.instance.transitionParam[3];
        timer = 0f;



        circles = new GameObject[nbCircles];
        for (int i = 0; i < nbCircles; i++)
        {
            Vector3 pos = Random.PointInRectangle(Vector2.zero, CameraManager.instance.cameraSize);
            circles[i] = Instantiate(circlePrefabs, pos, Quaternion.identity, CloneParent.cloneParent);
            circles[i].transform.localScale = Vector3.zero;
            circles[i].GetComponent<SpriteRenderer>().color = this.circleColor;
        }
    }

    private void Update()
    {
        for (int i = 0; i < nbCircles; i++)
        {
            circles[i].transform.localScale += Vector3.one * (speed * Time.deltaTime);
        }
        if(timer >= duration)
        {
            //TransitionManager.instance.transitionSceneIsDone = true;
        }

        timer += Time.deltaTime;
    }
}
