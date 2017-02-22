using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ThePlanker : MonoBehaviour
{ 
    public Text scoreText;
    public Color32[] gameColors = new Color32[4];
    public GameObject endPanel;   
    private GameObject[] thePlanker;
    private Vector2 plankBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);  
    private const float BOUNDS_SIZE = 3.5f;
    private const float PLANK_BOUNDS_GAIN = 0.25f;
    private const int COMBO_START_GAIN = 5;
    private const float STACK_MOVING_SPEED = 5.0f;
    private const float ERROR_MARGIN = 0.1f;
    private int scoreCount = 0;
    private int combo = 0;
    private Vector3 desiredPosition;
    private Vector3 lastTilePosition;
    private float tileTransition = 0.0f;
    private float tileSpeed = 2.5f;
    private float secondaryPosition;
    private int plankerIndex;
    private bool isMovingOnX = true;
    private bool gameOver = false;
 
   
    
    private void Start()
    {
        thePlanker = new GameObject[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            thePlanker[i] = transform.GetChild(i).gameObject;

        plankerIndex = transform.childCount - 1;
    }


    private void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        go.AddComponent<Rigidbody> ();

        ColorMesh(go.GetComponent<MeshFilter>().mesh);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceTile())
            {
                SpawnTile();
                scoreCount++;
                scoreText.text = scoreCount.ToString();
            }
            else
            {
                EndGame(); 
            }
        }

        MoveTile();

        // Murdam kubche
        transform.position = Vector3.Lerp(transform.position, desiredPosition, STACK_MOVING_SPEED * Time.deltaTime);
    }

    

    private void MoveTile()
    {
        if (gameOver)
            return;
        tileTransition += Time.deltaTime * tileSpeed;
        if (isMovingOnX)
            thePlanker[plankerIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * BOUNDS_SIZE, scoreCount, secondaryPosition);
        else
            thePlanker[plankerIndex].transform.localPosition = new Vector3(secondaryPosition, scoreCount, Mathf.Sin(tileTransition) * BOUNDS_SIZE);
    }

    private void SpawnTile()
    {
        lastTilePosition = thePlanker[plankerIndex].transform.localPosition;
        plankerIndex--;
        if (plankerIndex < 0)
            plankerIndex = transform.childCount - 1;

        desiredPosition = (Vector3.down) * scoreCount;
        thePlanker [plankerIndex].transform.localPosition = new Vector3 (0, scoreCount, 0);
        thePlanker [plankerIndex].transform.localScale = new Vector3 (plankBounds.x, 1, plankBounds.y);

        ColorMesh(thePlanker[plankerIndex].GetComponent<MeshFilter>().mesh);
    }


    private bool PlaceTile()
    {
        Transform t = thePlanker[plankerIndex].transform;

        if (isMovingOnX)
        {
            float deltaX = lastTilePosition.x - t.position.x;
            if (Mathf.Abs(deltaX) > ERROR_MARGIN)
            {
                //IZVUN combo
                combo = 0;
                plankBounds.x -= Mathf.Abs(deltaX);            
                if (plankBounds.x <= 0)
                    return false;

                float middle = lastTilePosition.x + t.localPosition.x / 2;
                t.localScale = new Vector3(plankBounds.x, 1, plankBounds.y);
                CreateRubble
                (
                    new Vector3((t.position.x > 0)
                    ? t.position.x + (t.localScale.x / 2)
                    : t.position.x - (t.localScale.x / 2)
                    , t.position.y
                    , t.position.z),
                    new Vector3 (Mathf.Abs(deltaX), 1, t.localScale.z)
                );   
                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);

            }
            else
            {
                if (combo > COMBO_START_GAIN)
                {
                    plankBounds.x += PLANK_BOUNDS_GAIN;
                    if (plankBounds.x > BOUNDS_SIZE)
                        plankBounds.x = BOUNDS_SIZE; 
                    float middle = lastTilePosition.x + t.localPosition.x / 2;
                    t.localScale = new Vector3(plankBounds.x, 1, plankBounds.y);                  
                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
                }
                combo++;
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
            }
          }
        else
       
        {
                float deltaZ = lastTilePosition.z - t.position.z;
                if (Mathf.Abs(deltaZ) > ERROR_MARGIN)
                {
                    //IZVUN combo
                    combo = 0;
                    plankBounds.y -= Mathf.Abs(deltaZ);
                    if (plankBounds.y <= 0)
                        return false;

                    float middle = lastTilePosition.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(plankBounds.x, 1, plankBounds.y);
                CreateRubble
          (
              new Vector3(t.position.x
              , t.position.y
              , (t.position.z > 0)
              ? t.position.z + (t.localScale.z / 2)
              : t.position.z - (t.localScale.z / 2)),
              new Vector3(t.localScale.z, 1, Mathf.Abs(deltaZ))
          );
                t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));

                }
                else
            {
                if (combo > COMBO_START_GAIN)
                {
                    if (plankBounds.y > BOUNDS_SIZE)
                        plankBounds.y = BOUNDS_SIZE;
                    plankBounds.y += PLANK_BOUNDS_GAIN;
                    float middle = lastTilePosition.z + t.localPosition.z / 2;
                    t.localScale = new Vector3(plankBounds.x, 1, plankBounds.y);
                    t.localPosition = new Vector3(middle - lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
                }
                combo++;
                    t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);
                }
        }
        
        secondaryPosition = (isMovingOnX)
            ? t.localPosition.x
            : t.localPosition.z;
        isMovingOnX = !isMovingOnX;
        
        return true;
    }


    private void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        float f = Mathf.Sin(scoreCount * 0.25f);

        for (int i = 0; i > vertices.Length; i++)
            colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);
        mesh.colors32 = colors;
    }

   private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < 0.33f)
            return Color.Lerp(a, b, t / 0.33f);
        else if (t < 0.66f)
            return Color.Lerp(b, c, (t - 0.33f) / 0.66f);
        else
            return Color.Lerp(c, d, (t - 0.66f) / 0.66f);
    }

   

    private void EndGame()
    {
        if (PlayerPrefs.GetInt("score") < scoreCount)
            PlayerPrefs.SetInt("score", scoreCount);       
        gameOver = true;
        endPanel.SetActive(true);
        thePlanker[plankerIndex].AddComponent<Rigidbody> ();

    }

    public void OnButtonClick(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    

}


    