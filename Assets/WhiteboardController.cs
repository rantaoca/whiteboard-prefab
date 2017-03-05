
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

// The whiteboard object must not have parents with a scale defined, since
// calculations are based on local scale.
public class WhiteboardController : MonoBehaviour,
                                    IPointerEnterHandler,
                                    IPointerExitHandler,
                                    IGvrPointerHoverHandler
{
    public Color markerColor;
    public float markerWidthInPixels = 3f;
    public float pixelsPerUnit = 100f;
    public Transform activeState;

    private Vector3 inactiveStatePosition;
    private Quaternion inactiveStateRotation;
    private Texture2D drawingSurface;

    // Drawing State
    private Vector2 lastPoint;
    private bool drawingInProgress = false;

    //Connecting the rectangles
    public float rect_dist = 10;
    private Vector2 prevPoint1;
    private Vector2 prevPoint2;
    private Vector2 prevPoint3;
    private int counter = 0;

    // Use this for initialization
    void Start()
    {
        inactiveStatePosition = transform.position;
        inactiveStateRotation = transform.rotation;
        drawingSurface = GenerateDrawingSurface();
        GetComponent<Renderer>().material.mainTexture = drawingSurface;

        for (int y = 0; y < drawingSurface.height; y++)
        {
            for (int x = 0; x < drawingSurface.width; x++)
            {
                Color color = ((x & y) != 0 ? Color.white : Color.gray);
                drawingSurface.SetPixel(x, y, color);
            }
        }

        //drawTriangle(new Vector2(20, 20), new Vector2(0, 10), new Vector2(1, 0));
        //drawTriangle(new Vector2(5, 5), new Vector2(1, 2), new Vector2(1, 9));


        drawingSurface.Apply();
    }

    // Update is called once per frame.
    void Update()
    {

    }

    // Generates a Texture2D based on the pixelsPerUnit density and the world size of
    // the whiteboard. A whiteboard is assumed to be x units wide and y units tall.
    private Texture2D GenerateDrawingSurface()
    {
        float pixelsWide = transform.localScale.x * pixelsPerUnit;
        float pixelsHigh = transform.localScale.y * pixelsPerUnit;
        Debug.Log("DrawingSurface size: " + pixelsWide + " x " + pixelsHigh);
        return new Texture2D((int)pixelsWide, (int)pixelsHigh);
    }


    // Convert a point in the world to a point on the 2D texture of the whiteboard.
    // World Space - Origin at world origin. Y is up, X and Z are aligned with world.
    // Whiteboard Space - Origin at the center of whiteboard object, in which X and Y
    //                    are aligned with the width and height respectively, and the
    //                    thickness of the whiteboard is in the Z axis. Up/Down in this
    //                    space refers to +/- Y and left/right refers to +/- X
    //                    respectively. The front of the board is facing the +Z direction.
    // Texture Space - 3D space with origin at the the "bottom left" of the whiteboard.
    //                 +X in this space is in this space points towards -X of Whiteboard
    //                 space, while +Y is in the direction of +Y of Whiteboard Space.
    //                 +Z in this space maps to -Z of the Whiteboard Space. This follows
    //                 the left-handed coordinate system of Unity.
    // Texture2DSpace - 2D subspace of Texture Space where Z is ignored. X and Y in
    //                  this space are scaled such that an x value of "width" maps to
    //                  the right edge of the whiteboard and a y value of "height" maps
    //                  to the top edge of the whiteboard.
    // Conversion is calculated as follows:
    // 1. Convert point in World Space to point in Whiteboard Space
    private Vector2 WorldToTexture2D(Vector3 pointInWorldSpace)
    {
        Vector3 pointInWhiteboardSpace = transform.InverseTransformPoint(pointInWorldSpace);
        Vector3 pointInTextureSpace = new Vector3(-1 * pointInWhiteboardSpace.x,
                                                  1 * pointInWhiteboardSpace.y,
                                                  -1 * pointInWhiteboardSpace.z) +
                                      new Vector3(0.5f, 0.5f, 0);
        Vector2 pointInTexture2DSpace =
            new Vector2(pointInTextureSpace.x * drawingSurface.width,
                        pointInTextureSpace.y * drawingSurface.height);
        return pointInTexture2DSpace;
    }

    private void DrawPoint(Vector2 point, bool newStroke)
    {
        print("Drawing point at " + point);
        if (newStroke)
        {
            drawingSurface.SetPixel((int)point.x, (int)point.y, markerColor);
            Debug.Log("Starting new stroke: " + (int)point.x + ", " + (int)point.y);
        }
        else
        {
            Vector2 dist = point - lastPoint;
            dist = dist.normalized;
            Vector2 offset = Quaternion.AngleAxis(90, Vector3.forward) * dist * markerWidthInPixels / 2;
            Vector2 pointB = point + offset;
            Vector2 pointC = point - offset;

            Vector2 pointD = lastPoint - offset;
            Vector2 pointE = lastPoint + offset;

            //only drawing triangles
            //drawTriangle(lastPoint, pointB, pointC);
            //generateLine(lastPoint, point);

            //drawing rectangles
            drawTriangle(pointE, pointB, pointC);
            drawTriangle(pointC, pointE, pointD);

            //storing points to draw connection between rectangles
            if (Vector2.Distance(prevPoint1, pointC) < rect_dist || Vector2.Distance(prevPoint3, pointB) < rect_dist)
            {
                if (prevPoint1 != null && prevPoint2 != null && prevPoint3 != null && counter > 1)
                {
                    drawTriangle(prevPoint1, prevPoint2, pointC);
                    drawTriangle(prevPoint3, prevPoint2, pointB);
                }

                prevPoint1 = pointD;
                prevPoint2 = lastPoint;
                prevPoint3 = pointE;
                counter++;
            }

            //linear function implementation
            //float slope = (point.y - lastPoint.y) / (point.x - lastPoint.x);
            //float intercept = point.y - slope * point.x;
            //Vector2 pointB = new Vector2((-1 / slope) * point.x + markerWidthInPixels, (-1 / slope) * point.y + markerWidthInPixels);
            //Vector2 pointC = new Vector2((-1 / slope) * point.x - markerWidthInPixels, (-1 / slope) * point.y - markerWidthInPixels);
        }
        drawingSurface.Apply();
        lastPoint = point;
    }

    public class GridCoord
    {
        public int x, y;
        public GridCoord(int a, int b)
        {
            x = a;
            y = b;
        }
    }

    private List<GridCoord> generateLine(Vector2 start, Vector2 end)
    {
        List<GridCoord> grid = new List<GridCoord>();

        // Bresenham's line algorithm.
        int flipX = (start.x > end.x) ? 1 : -1;
        int flipY = (start.y > end.y) ? 1 : -1;
        float x0 = 0;
        float y0 = 0;
        float x1 = flipX * (start.x - end.x);
        float y1 = flipY * (start.y - end.y);
        bool swapXY = x1 < y1;
        if (swapXY)
        {
            float tmp = x1;
            x1 = y1;
            y1 = tmp;
        }
        float deltaX = x1 - x0;
        float deltaY = y1 - y0;
        float deltaError = Mathf.Abs(deltaY / deltaX);
        float error = deltaError - 0.5f;
        int y = (int)y0;

        for (int x = (int)x0; x <= (int)x1; x++)
        {
            int drawX = x;
            int drawY = y;
            if (swapXY)
            {
                int tmp = drawX;
                drawX = drawY;
                drawY = tmp;
            }

            drawX = (int)end.x + flipX * drawX;
            drawY = (int)end.y + flipY * drawY;
            drawingSurface.SetPixel(drawX, drawY, markerColor);
            grid.Add(new GridCoord(drawX, drawY));

            error += deltaError;
            if (error >= 0.5f)
            {
                y++;
                error -= 1.0f;
            }
        }

        return grid;
    }

    //fill x coord cells, same y coord
    private void fillCells(GridCoord a, GridCoord b)
    {
        int x0 = a.x;
        int x1 = b.x;
        int temp;

        if(x0 > x1)
        {
            temp = x0;
            x0 = x1;
            x1 = temp;
        }

        for(int i = x0; i <= x1; i++)
        {
            drawingSurface.SetPixel(i, a.y, markerColor);
        }
    }

    private void drawTriangle(Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 temp;

        if (a.y < b.y)
        {
            temp = a;
            a = b;
            b = temp;
        }

        if (a.y < c.y)
        {
            temp = a;
            a = c;
            c = temp;
        }

        if (b.y > c.y)
        {
            temp = b;
            b = c;
            c = temp;
        }

        List<GridCoord> line_AC = generateLine(a, c);
        List<GridCoord> line_CB = generateLine(c, b);
        List<GridCoord> line_AB = generateLine(a, b);

        line_AB.Sort(compareGridCoord);
        line_CB.Sort(compareGridCoord);
        line_AC.Sort(compareGridCoord);

        int ACi = 1;
        int ABi = 1;
        int CBi = 1;

        while (ACi < line_AC.Count && ABi < line_AB.Count)
        {
            fillCells(line_AC[ACi], line_AB[ABi]);
            ACi++;
            ABi++;
            while (ACi < line_AC.Count && line_AC[ACi].y == line_AC[ACi - 1].y)
            {
                ACi++;
            }
            while (ABi < line_AB.Count && line_AB[ABi].y == line_AB[ABi - 1].y)
            {
                ABi++;
            }
        }

        //draw other half of triangle
        while (CBi < line_CB.Count && ABi < line_AB.Count) {
            fillCells(line_CB[CBi], line_AB[ABi]);
            CBi++;
            ABi++;
            while (CBi < line_CB.Count && line_CB[CBi].y == line_CB[CBi - 1].y) {
                CBi++;
            }
            while (ABi < line_AB.Count && line_AB[ABi].y == line_AB[ABi - 1].y) {
                ABi++;
            }
        }

    }

    // Sorts by largest to smallest y.
    private static int compareGridCoord(GridCoord a, GridCoord b)
    {
        if(a.y == b.y)
        {
            return 0;
        }
        else if(a.y < b.y)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }


    public void OnGvrPointerHover(PointerEventData eventData)
    {
        Debug.Log("Pointer hover.");
        if (GvrController.IsTouching)
        {
            Vector3 touchPointInWorldSpace = eventData.pointerCurrentRaycast.worldPosition;
            Vector2 touchPointInTexture2DSpace = WorldToTexture2D(touchPointInWorldSpace);
            DrawPoint(touchPointInTexture2DSpace, GvrController.TouchDown);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered");
        transform.position = activeState.position;
        transform.rotation = activeState.rotation;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exited");
        transform.position = inactiveStatePosition;
        transform.rotation = inactiveStateRotation;
    }
}
