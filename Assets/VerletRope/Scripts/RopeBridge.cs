using System.Collections.Generic;
using UnityEngine;

public class RopeBridge : MonoBehaviour
{
    public Transform StartPoint;
    public Transform EndPoint;

    private LineRenderer lineRenderer;
    [SerializeField] private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    [SerializeField] private float ropeSegLen = 0.25f;
    [SerializeField][Range(2, 100)] private int segmentLength = 35;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Vector2 forceGravity = new Vector2(0f, -1f);

    void Start()
    {
        this.lineRenderer = this.GetComponent<LineRenderer>();
        initSegment();
    }

    private void initSegment()
    {
        Vector3 ropeStartPoint = StartPoint.position;

        for (int i = 0; i < segmentLength; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= ropeSegLen;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawLines();
        DrawSpheres(0.1f);
    }
    void DrawSpheres(float r)
    {
        Gizmos.DrawSphere(StartPoint.transform.position, r);
        Gizmos.DrawSphere(EndPoint.transform.position, r);
    }

    void DrawLines()
    {
        Gizmos.DrawLine(StartPoint.transform.position, EndPoint.transform.position);
    }
#endif

    void Update()
    {
        this.DrawRope();
    }

    private void FixedUpdate()
    {
        this.Simulate();
    }

    private void Simulate()
    {
        updateSegment();

        for (int i = 1; i < this.segmentLength; i++)
        {
            RopeSegment firstSegment = this.ropeSegments[i];
            Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += velocity;
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
            this.ropeSegments[i] = firstSegment;
        }

        //CONSTRAINTS
        for (int i = 0; i < 50; i++)
        {
            this.ApplyConstraint();
        }
    }

    private void updateSegment()
    {
        if (ropeSegments.Count > segmentLength)
        {
            int count = ropeSegments.Count - segmentLength;
            for (int i = 0; i < count; i++)
                ropeSegments.RemoveAt(ropeSegments.Count - 1);
        }
        else if (ropeSegments.Count < segmentLength)
        {
            int count = segmentLength - ropeSegments.Count;
            Vector3 ropeStartPoint = ropeSegments[ropeSegments.Count - 1].posNow;
            for (int i = 0; i < count; i++)
            {
                ropeSegments.Add(new RopeSegment(ropeStartPoint));
                ropeStartPoint.y -= ropeSegLen;
            }
        }
    }

    private void ApplyConstraint()
    {
        //Constrant to First Point 
        RopeSegment firstSegment = this.ropeSegments[0];
        firstSegment.posNow = this.StartPoint.position;
        this.ropeSegments[0] = firstSegment;

        //Constrant to Second Point 
        RopeSegment endSegment = this.ropeSegments[this.ropeSegments.Count - 1];
        endSegment.posNow = this.EndPoint.position;
        this.ropeSegments[this.ropeSegments.Count - 1] = endSegment;

        for (int i = 0; i < this.segmentLength - 1; i++)
        {
            RopeSegment firstSeg = this.ropeSegments[i];
            RopeSegment secondSeg = this.ropeSegments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - this.ropeSegLen);
            Vector2 changeDir = Vector2.zero;

            if (dist > ropeSegLen)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            }
            else if (dist < ropeSegLen)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                this.ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                this.ropeSegments[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                this.ropeSegments[i + 1] = secondSeg;
            }
        }
    }

    private void DrawRope()
    {
        updateSegment();

        float lineWidth = this.lineWidth;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[this.segmentLength];
        for (int i = 0; i < this.segmentLength; i++)
        {
            ropePositions[i] = this.ropeSegments[i].posNow;
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }

    [System.Serializable]
    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }
}
