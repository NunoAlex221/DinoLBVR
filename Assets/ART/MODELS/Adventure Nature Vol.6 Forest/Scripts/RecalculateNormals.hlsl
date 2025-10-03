#ifndef __MESHROTATION_HLSL__
#define __MESHROTATION_HLSL__

float3 RotateVectorByQuaternion(float4 rotation, float3 vec)
{
    float3 doubleAxis = 2.0 * cross(rotation.xyz, vec);
    return vec + rotation.w * doubleAxis + cross(rotation.xyz, doubleAxis);
}

float4 CreateRotationBetweenVectors(float3 fromDir, float3 toDir)
{
    float4 result;
    float cosAngle = 1.0 + dot(fromDir, toDir);
    
    if (cosAngle < 1e-6) // vectors are opposite
    {
        result.xyz = abs(fromDir.x) > abs(fromDir.z) ? 
                     float3(-fromDir.y, fromDir.x, 0.0) : 
                     float3(0.0, -fromDir.z, fromDir.y);
        result.w = 0.0;
    }
    else
    {
        result.xyz = cross(fromDir, toDir);
        result.w = cosAngle;
    }
    return normalize(result);
}

// Recalculates normal and tangent vectors based on position transformation
// Note: This approximation doesn't preserve twist components of the transformation
void RecalculateSurfaceVectors_float(
    in float3 originalPosition,
    in float3 transformedPosition,
    in float3 inputNormal,
    in float3 inputTangent,
    out float3 outputNormal,
    out float3 outputTangent
)
{
    float4 rotationQuat = CreateRotationBetweenVectors(
        normalize(originalPosition), 
        normalize(transformedPosition)
    );
    
    outputNormal = RotateVectorByQuaternion(rotationQuat, inputNormal);
    outputTangent = RotateVectorByQuaternion(rotationQuat, inputTangent);
}

#endif