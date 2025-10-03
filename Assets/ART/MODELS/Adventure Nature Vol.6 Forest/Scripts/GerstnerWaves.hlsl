#ifndef ENHANCED_GERSTNER_WAVES_INCLUDED
#define ENHANCED_GERSTNER_WAVES_INCLUDED

// Ultra-stable hash function optimized for large coordinates
float hash(float n) {
    // Use higher precision constants and avoid small multipliers
    n = frac(n * 0.3183098861837907);
    n *= n + 19.19;
    n *= n + n;
    return frac(n);
}

// High-precision noise that works well with large coordinates
float stableNoise(float2 p) {
    // Reduce coordinate magnitude before floor to prevent precision loss
    float2 scaledP = p * 0.01; // Scale down large coordinates
    float2 i = floor(scaledP);
    float2 f = frac(scaledP);
    
    // Higher order smoothstep for better interpolation
    f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
    
    float n = i.x + i.y * 157.0;
    return lerp(lerp(hash(n), hash(n + 1.0), f.x),
                lerp(hash(n + 157.0), hash(n + 158.0), f.x), f.y);
}

void EnhancedGerstnerWaves_float(
    // Inputs
    float2 UV,
    float Time,
    float Amplitude,
    float Wavelength,
    float Speed,
    float Steepness,
    float2 Direction,
    int WaveCount,
    float WindDirection,
    float WindStrength,
    float Choppiness,
    float DeepWaterEffect,
    
    // Outputs
    out float3 Position,
    out float3 Normal,
    out float Foam
)
{
    float3 pos = float3(0, 0, 0);
    float3 norm = float3(0, 1, 0);
    float totalHeight = 0.0;
    float foamAccum = 0.0;
    
    // PRECISION FIX: Use original UV but with precision-safe calculations
    // Don't use frac() as it causes tiling - instead work with original coordinates
    float2 stableUV = UV;
    
    // Use more stable time handling without aggressive modulo
    float stableTime = Time;
    
    // Normalize wind direction
    float2 windDir = float2(cos(WindDirection), sin(WindDirection));
    
    // Golden angle for wave distribution
    float goldenAngle = 2.39996323;
    
    for (int i = 0; i < WaveCount; i++)
    {
        float waveIndex = (float)i;
        
        // Use more stable random generation with larger seeds
        float seed1 = waveIndex * 127.1 + 311.7;
        float seed2 = waveIndex * 269.5 + 183.3;
        float seed3 = waveIndex * 419.2 + 371.9;
        float seed4 = waveIndex * 521.7 + 457.1; // Additional seed for scale variety
        
        float randA = hash(seed1);
        float randB = hash(seed2);
        float randC = hash(seed3);
        float randD = hash(seed4); // Additional random for scale variety
        
        // Wave distribution with better energy falloff
        float normalizedIndex = waveIndex / max(WaveCount - 1, 1);
        float energyFactor = exp(-1.5 * normalizedIndex);
        float sizeFactor = 0.2 + energyFactor * 0.8 + randA * 0.2;
        
        // ADAPTIVE WAVE SCALE VARIETY: Adjusts distribution based on wave count
        float waveCountNormalized = clamp(WaveCount / 50.0, 0.0, 1.0); // Normalize to 0-1 for 50 waves
        
        // Use realistic wave spectrum distribution (similar to Pierson-Moskowitz)
        float frequency = 0.1 + normalizedIndex * 2.0; // Higher frequency for smaller waves
        float spectrumWeight = exp(-1.25 * pow(frequency, -4)) * pow(frequency, -5);
        
        // Adaptive wave type distribution based on wave count
        float waveType = randD;
        float realismScale = 1.0;
        float realismHeight = 1.0;
        
        // For low wave counts, focus on essential wave types
        if (WaveCount <= 30) {
            // Simplified distribution for low wave counts
            if (waveType < 0.25) { // Capillary/small waves - 25%
                realismScale = 0.3 + randA * 0.4; // 0.3x to 0.7x scale
                realismHeight = 0.4 + randB * 0.4; // Smaller height
            }
            else if (waveType < 0.65) { // Main wind waves - 40%
                realismScale = 0.8 + randC * 0.6; // 0.8x to 1.4x scale
                realismHeight = 0.9 + randD * 0.3; // Normal height
            }
            else { // Swell waves - 35%
                realismScale = 1.5 + randD * 1.0; // 1.5x to 2.5x scale
                realismHeight = 1.1 + randA * 0.3; // Slightly larger height
            }
        }
        else {
            // Full realistic distribution for higher wave counts
            if (waveType < 0.15) { // Capillary waves (ripples) - 15%
                realismScale = 0.1 + randA * 0.3; // 0.1x to 0.4x scale
                realismHeight = 0.2 + randB * 0.3; // Much smaller height
            }
            else if (waveType < 0.4) { // Gravity waves (small) - 25%
                realismScale = 0.4 + randB * 0.5; // 0.4x to 0.9x scale
                realismHeight = 0.6 + randC * 0.4; // Smaller height
            }
            else if (waveType < 0.75) { // Wind waves (medium) - 35%
                realismScale = 0.8 + randC * 0.8; // 0.8x to 1.6x scale
                realismHeight = 0.9 + randD * 0.3; // Normal height
            }
            else if (waveType < 0.92) { // Swell waves (large) - 17%
                realismScale = 1.8 + randD * 1.2; // 1.8x to 3.0x scale
                realismHeight = 1.1 + randA * 0.4; // Slightly larger height
            }
            else { // Long period swell - 8%
                realismScale = 3.0 + randA * 2.0; // 3.0x to 5.0x scale
                realismHeight = 1.2 + randB * 0.3; // Moderately larger height
            }
        }
        
        // Apply spectrum weighting for realism (stronger for higher wave counts)
        float spectrumInfluence = 0.3 + waveCountNormalized * 1.7; // 0.3 to 2.0
        realismHeight *= spectrumWeight * spectrumInfluence + 0.5;
        
        // Ensure essential wave coverage for low counts
        if (WaveCount <= 30) {
            // Boost primary wave types for low counts
            float essentialBoost = 1.0 + (1.0 - waveCountNormalized) * 0.3;
            realismHeight *= essentialBoost;
        }
        
        // Combine with original energy distribution
        float finalScale = sizeFactor * realismScale * (0.9 + randB * 0.2);
        
        // PRECISION FIX: Realistic wavelength ranges
        float waveLengthVar = Wavelength * finalScale;
        waveLengthVar = clamp(waveLengthVar, 0.8, 150.0); // Realistic ocean wave range
        float w = 2.0 * 3.14159265359 / waveLengthVar;
        
        // Realistic amplitude scaling following wave physics
        float amp = Amplitude * sizeFactor * realismHeight * (0.7 + randC * 0.3);
        amp = max(amp, 0.005); // Minimum for tiny ripples
        
        // Apply realistic amplitude-wavelength relationship (steeper short waves)
        float physicalHeightLimit = waveLengthVar * 0.14; // Max wave height = 1/7 wavelength
        amp = min(amp, physicalHeightLimit);
        
        // Physical steepness constraint
        float maxSteepness = min(Steepness, 0.8 / (w * amp));
        float steep = maxSteepness * (0.7 + sizeFactor * 0.3) * (0.8 + randA * 0.2);
        
        // Direction with wind influence
        float baseAngle = waveIndex * goldenAngle;
        float windInfluence = WindStrength * (1.0 - normalizedIndex * 0.3);
        float windOffset = atan2(windDir.y, windDir.x);
        float directionVariation = (randB - 0.5) * 2.0 * (1.0 - windInfluence * 0.7);
        
        float angle = baseAngle + windOffset * windInfluence + directionVariation;
        float2 waveDir = normalize(float2(cos(angle), sin(angle)));
        
        // CRITICAL PRECISION FIX: Stable phase calculation
        float qi = steep / (w * amp + 0.001);
        qi = clamp(qi, 0.0, 1.2);
        
        // PRECISION-SAFE phase calculation without tiling
        // Use original UV coordinates but keep intermediate calculations stable
        float spatialPhase = dot(w * waveDir, stableUV);
        float randomPhase = randC * 6.283185307;
        
        // Time-based phase with deep water effect
        float deepWaterFreq = sqrt(9.81 * w) * DeepWaterEffect;
        float timePhase = (Speed * (0.9 + randA * 0.2) + deepWaterFreq) * stableTime;
        
        // STABLE PHASE: Avoid modulo operations that can cause precision jumps
        float theta = spatialPhase + randomPhase + timePhase;
        
        // Gerstner wave calculation
        float cosTheta = cos(theta);
        float sinTheta = sin(theta);
        
        // Choppiness effects
        float horizontalChop = 1.0 + Choppiness * sizeFactor * 0.15;
        float verticalChop = 1.0 + Choppiness * 0.08;
        
        // Wave displacement
        float xDisp = qi * amp * waveDir.x * cosTheta * horizontalChop;
        float zDisp = qi * amp * waveDir.y * cosTheta * horizontalChop;
        float yDisp = amp * sinTheta * verticalChop;
        
        pos.x += xDisp;
        pos.z += zDisp;
        pos.y += yDisp;
        totalHeight += abs(yDisp);
        
        // PRECISION-SAFE normal calculation
        float wa = w * amp * verticalChop;
        float qiWa = qi * wa * horizontalChop;
        
        // Accumulate normal contributions with clamping
        float normalX = waveDir.x * wa * cosTheta;
        float normalZ = waveDir.y * wa * cosTheta;
        float normalY = qiWa * sinTheta;
        
        // Clamp individual contributions to prevent precision issues
        norm.x -= clamp(normalX, -0.5, 0.5);
        norm.z -= clamp(normalZ, -0.5, 0.5);
        norm.y -= clamp(normalY, -0.3, 0.3);
        
        // Foam calculation
        float waveSlope = abs(wa * sinTheta);
        float curvature = abs(qiWa * cosTheta);
        foamAccum += (waveSlope + curvature * 0.4) * sizeFactor;
    }
    
    // Peak sharpening with bounds checking
    float avgHeight = totalHeight / max(WaveCount, 1);
    float peakFactor = clamp(avgHeight / max(Amplitude * 0.6, 0.01), 0.0, 1.5);
    float peakSharpening = 1.0 + peakFactor * 0.2 * Choppiness;
    pos.y *= peakSharpening;
    
    // FINAL NORMAL PROCESSING - Single, stable operation
    // Ensure normal has minimum upward component for stability
    norm.y = max(norm.y, 0.3);
    
    // Clamp horizontal components more aggressively for large surfaces
    norm.x = clamp(norm.x, -0.7, 0.7);
    norm.z = clamp(norm.z, -0.7, 0.7);
    
    // Single normalize operation
    float normLength = length(norm);
    norm = norm / max(normLength, 0.1); // Prevent division by very small numbers
    
    // Foam processing
    float foamThreshold = Amplitude * WaveCount * 0.25;
    Foam = saturate(foamAccum / max(foamThreshold, 0.01));
    Foam = pow(Foam, 0.6);
    
    Position = pos;
    Normal = norm;
}
#endif