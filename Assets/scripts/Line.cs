using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Line : MonoBehaviour 
{
	public LineRenderer lineRender;
	public List<Vector3> verts;
	
	void InitLineRenderer()
	{
		lineRender = GetComponent<LineRenderer>();
		lineRender.SetColors(Color.white, Color.white);
		lineRender.SetWidth(0.05f, 0.05f);
	}
	
	void AddVerts(Vector3 pos1, Vector3 pos2)
	{
		verts.Add(pos1);
		verts.Add(pos2);
	}
	
	void DrawLines()
	{
		lineRender.SetVertexCount(verts.Count);
		
		for(int i = 0; i < verts.Count; ++i)
			lineRender.SetPosition(i, verts[i]);
	}
}

/* using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineDraw : MonoBehaviour {
    public LineRenderer lineRenderer;
	// Use this for initialization

    public float alphaDelta = 0.1f;

    public float lineLength = 5;
    public float lineWidthStart = 0.01f;
    public float lineWidthEnd = 0.02f;

    public List<Vector3> vertices;

    public Color colorStart = Color.white;
    public Color colorEnd = Color.blue;
    private Color currentColor;
    private float currentWidth;
    private float currentTime;


    public float timeToLerp = 1.0f;


	void Awake () 
    {
        lineRenderer = GetComponent<LineRenderer>();
		
        lineRenderer.SetColors(Color.white, Color.white);
        lineRenderer.SetWidth(lineWidthStart, lineWidthStart);
        
        
        currentColor = colorStart;
        currentWidth = lineWidthStart;
	}

    void Start()
    {
        lineRenderer.SetVertexCount(vertices.Count);
    }
	
	// Update is called once per frame
	void Update () 
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            
            lineRenderer.SetPosition(i, vertices[i]);
        }
	}

    IEnumerator lerpColor(bool fromStart)
    {
        float tColor = 0.0f;
        float lerpRate = 1.0f / timeToLerp;

        while (tColor < 1.0f)
        {
            tColor += (Time.deltaTime * lerpRate);
            if (fromStart == true)
            {
                currentColor = Color.Lerp(colorStart, colorEnd, tColor);
            }

            else
            {
                currentColor = Color.Lerp(colorEnd, colorStart, tColor);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator lerpWidth(bool fromStart)
    {
        float tWidth = 0.0f;
        float lerpRate = 1.0f / timeToLerp;

        while (tWidth < 1.0f)
        {
            tWidth += (Time.deltaTime * lerpRate);
            if (fromStart == true)
            {
                currentWidth = Mathf.Lerp(lineWidthStart, lineWidthEnd, tWidth);
            }

            else
            {
                currentWidth = Mathf.Lerp(lineWidthEnd, lineWidthStart, tWidth);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    


} */