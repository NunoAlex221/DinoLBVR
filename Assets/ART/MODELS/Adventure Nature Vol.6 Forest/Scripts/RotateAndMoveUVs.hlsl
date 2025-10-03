#ifndef ROTATE_AND_MOVE_UVS_INCLUDED
#define ROTATE_AND_MOVE_UVS_INCLUDED

void RotateAndMoveUVs_float(float2 UV, float Angle, float Speed, float Time, out float2 RotatedUV)  
{
    // Convert Angle from Degrees to Radians
    float rad = Angle * (3.14159265 / 180.0);
    float cosA = cos(rad);
    float sinA = sin(rad);

    // Move the UVs based on the angle
    float2 motion = float2(cosA, sinA) * Speed * Time;

    // Add the motion to the world-space UVs
    UV += motion;

    // No rotation applied to UVs here because the motion is already aligned with the angle
    RotatedUV = UV;
}

#endif // ROTATE_AND_MOVE_UVS_INCLUDED
