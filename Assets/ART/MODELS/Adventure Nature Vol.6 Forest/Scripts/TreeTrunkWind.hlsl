#ifndef TREE_TRUNK_WIND_INCLUDED
#define TREE_TRUNK_WIND_INCLUDED

void TreeTrunkWind_float(
    float3 Position,
    float WindStrength,
    float WindSpeed,
    float Time,
    float HeightMask,
    out float3 Output
)
{
    // Use height mask directly (0 = bottom/no movement, 1 = top/full movement)
    float heightFactor = HeightMask;
    
    // Create random wind directions using sine waves with different frequencies
    float timeX = Time * WindSpeed;
    float timeZ = Time * WindSpeed * 0.73; // Different frequency for natural variation
    
    // Primary wind movement (slow, large movements)
    float windX = sin(timeX) * cos(timeX * 0.3);
    float windZ = sin(timeZ) * cos(timeZ * 0.41);
    
    // Add noise-like variation using multiple sine waves
    float noiseX = sin(timeX * 2.1 + 1.7) * 0.4 + sin(timeX * 3.7 + 0.8) * 0.2;
    float noiseZ = sin(timeZ * 2.3 + 2.1) * 0.4 + sin(timeZ * 3.1 + 1.3) * 0.2;
    
    // Combine primary movement with noise
    float totalWindX = (windX + noiseX) * WindStrength * heightFactor;
    float totalWindZ = (windZ + noiseZ) * WindStrength * heightFactor;
    
    // Add position-based variation for different parts of the tree
    float posVariation = sin(Position.x * 0.1 + Position.z * 0.1);
    totalWindX += posVariation * WindStrength * 0.2 * heightFactor;
    totalWindZ += posVariation * WindStrength * 0.15 * heightFactor;
    
    // Apply wind displacement
    float3 windOffset = float3(totalWindX, 0, totalWindZ);
    
    Output = Position + windOffset;
}

#endif