using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceAnimationController : MonoBehaviour, FaceExpressionUpdate {
    float morphSpeed = 1f;
    float morphSmile=0, morphfrown = 0;
    bool nextExpress = false;
    SkinnedMeshRenderer skmRenderer;
    Mesh headMesh;
    FaceExpression faceNextExpression;

    public void updateFaceExpression(FaceExpression newFaceialExpression)
    {
        
        faceNextExpression = newFaceialExpression;
        nextExpress = true;
    }

    void Start () {
        skmRenderer = GetComponent<SkinnedMeshRenderer>();
        headMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;

	}
	

	void Update () {
    
        if (nextExpress)
        {
            switch (faceNextExpression.lowerFaceExpression) {
                case "neutral":  {
                        skmRenderer.SetBlendShapeWeight(0, 0);
                        skmRenderer.SetBlendShapeWeight(1, 0);
                        break; }
                case "frown": {
                        skmRenderer.SetBlendShapeWeight(0, 0);
                        skmRenderer.SetBlendShapeWeight(1,faceNextExpression.lowerFaceExpressionPower * 100);
                        break;     }
                case "smile": {
                        skmRenderer.SetBlendShapeWeight(0, faceNextExpression.lowerFaceExpressionPower * 100);
                        skmRenderer.SetBlendShapeWeight(1, 0);
                        break;
                    }
            }
       
            
            nextExpress = false;
        }
    
	}

    
}
interface FaceExpressionUpdate{
    void updateFaceExpression(FaceExpression newFaceialExpression);
    }
