#ifndef CLOUD_RAYMARCHING_INCLUDED
#define CLOUD_RAYMARCHING_INCLUDED

// Noise Controls Structure
struct NoiseControls {
    float scale;           // Overall noise scale
    float frequency1;      // First octave frequency
    float frequency2;      // Second octave frequency  
    float frequency3;      // Third octave frequency
    float amplitude1;      // First octave amplitude
    float amplitude2;      // Second octave amplitude
    float amplitude3;      // Third octave amplitude
    int octaves;          // Number of octaves
    float lacunarity;     // Frequency multiplier between octaves
    float gain;           // Amplitude multiplier between octaves
    float2 windDirection; // Wind direction vector
    float windSpeed;      // Wind animation speed
    float turbulence;     // Additional turbulence
};

// Quality Controls Structure  
struct QualityControls {
    int maxSteps;         // Maximum raymarching steps
    float stepSize;       // Size of each step
    float maxDistance;    // Maximum ray distance
    float densityThreshold; // Minimum density to consider
    float earlyExitThreshold; // Transmittance threshold for early exit
    int lightingSteps;    // Steps for lighting calculation
    float lightStepSize;  // Step size for lighting
};

// Rendering Controls Structure
struct RenderingControls {
    float coverage;       // Cloud coverage (0-1)
    float density;        // Overall cloud density
    float contrast;       // Cloud contrast
    float brightness;     // Cloud brightness
    float absorption;     // Light absorption
    float scattering;     // Light scattering
    float3 baseColor;     // Base cloud color
    float3 shadowColor;   // Shadow/dark areas color
    float3 highlightColor; // Highlight color
    float rimPower;       // Rim lighting power
    float rimIntensity;   // Rim lighting intensity
    float shadowIntensity; // Shadow intensity
    float softness;       // Edge softness
};

// Noise function for cloud generation
float hash(float2 p) {
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p) {
    float2 i = floor(p);
    float2 f = frac(p);
    
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    
    float2 u = f * f * (3.0 - 2.0 * f);
    
    return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

float fbm(float2 p, int octaves) {
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 0.0;
    
    for (int i = 0; i < octaves; i++) {
        value += amplitude * noise(p);
        p *= 2.0;
        amplitude *= 0.5;
    }
    
    return value;
}

// Advanced FBM with full control
float fbmAdvanced(float2 p, NoiseControls controls) {
    float value = 0.0;
    float amplitude = controls.amplitude1;
    float frequency = controls.frequency1;
    
    // First octave with custom frequency and amplitude
    value += amplitude * noise(p * frequency);
    
    // Additional octaves
    if (controls.octaves > 1) {
        amplitude = controls.amplitude2;
        frequency = controls.frequency2;
        value += amplitude * noise(p * frequency);
    }
    
    if (controls.octaves > 2) {
        amplitude = controls.amplitude3;
        frequency = controls.frequency3;
        value += amplitude * noise(p * frequency);
    }
    
    // Standard fractal octaves for remaining
    amplitude *= controls.gain;
    frequency *= controls.lacunarity;
    
    for (int i = 3; i < controls.octaves; i++) {
        value += amplitude * noise(p * frequency);
        amplitude *= controls.gain;
        frequency *= controls.lacunarity;
    }
    
    return value;
}

// Enhanced cloud density with full noise control
float cloudDensityAdvanced(
    float2 pos, 
    float time, 
    NoiseControls noiseControls,
    RenderingControls renderControls
) {
    // Apply wind animation
    float2 windOffset = noiseControls.windDirection * noiseControls.windSpeed * time;
    float2 animatedPos = pos + windOffset;
    
    // Apply noise scale
    animatedPos *= noiseControls.scale;
    
    // Generate base cloud shape with advanced FBM
    float cloudValue = fbmAdvanced(animatedPos, noiseControls);
    
    // Add turbulence if enabled
    if (noiseControls.turbulence > 0.0) {
        float2 turbPos = animatedPos * 3.0 + float2(time * 0.3, time * 0.1);
        float turb = fbm(turbPos, 3) * noiseControls.turbulence;
        cloudValue += turb;
    }
    
    // Apply coverage control with smooth transitions
    float coverageThreshold = 1.0 - renderControls.coverage;
    float coverageRange = renderControls.softness * 0.5;
    cloudValue = smoothstep(coverageThreshold - coverageRange, 
                           coverageThreshold + coverageRange, cloudValue);
    
    // Apply contrast
    cloudValue = pow(cloudValue, renderControls.contrast);
    
    // Apply density multiplier
    cloudValue *= renderControls.density;
    
    return saturate(cloudValue);
}

// Advanced lighting calculation with full control
float3 calculateCloudLightingAdvanced(
    float density, 
    float3 lightDir, 
    float3 viewDir, 
    float3 lightColor,
    RenderingControls renderControls,
    QualityControls qualityControls,
    float2 samplePos,
    float time
) {
    // Light attenuation through cloud
    float lightAttenuation = exp(-density * renderControls.absorption);
    
    // Forward scattering (Henyey-Greenstein approximation)
    float cosTheta = dot(normalize(lightDir), normalize(viewDir));
    float scatteringPhase = (1.0 - renderControls.scattering * renderControls.scattering) / 
                           pow(1.0 + renderControls.scattering * renderControls.scattering - 
                               2.0 * renderControls.scattering * cosTheta, 1.5);
    
    // Sample light through cloud (simplified light marching)
    float lightEnergy = 1.0;
    if (qualityControls.lightingSteps > 0) {
        for (int i = 0; i < qualityControls.lightingSteps; i++) {
            float2 lightSamplePos = samplePos + lightDir.xy * float(i) * qualityControls.lightStepSize;
            NoiseControls defaultNoise;
            defaultNoise.scale = 2.0;
            defaultNoise.frequency1 = 2.0;
            defaultNoise.frequency2 = 4.0;
            defaultNoise.frequency3 = 8.0;
            defaultNoise.amplitude1 = 1.0;
            defaultNoise.amplitude2 = 0.5;
            defaultNoise.amplitude3 = 0.25;
            defaultNoise.octaves = 3;
            defaultNoise.lacunarity = 2.0;
            defaultNoise.gain = 0.5;
            defaultNoise.windDirection = float2(1.0, 0.5);
            defaultNoise.windSpeed = 0.5;
            defaultNoise.turbulence = 0.0;
            
            float lightDensity = cloudDensityAdvanced(lightSamplePos, time, defaultNoise, renderControls);
            lightEnergy *= exp(-lightDensity * renderControls.absorption * qualityControls.lightStepSize);
            if (lightEnergy < 0.01) break;
        }
    }
    
    // Base color mixing
    float3 cloudColor = lerp(renderControls.shadowColor, renderControls.baseColor, lightAttenuation);
    
    // Add scattered light
    cloudColor += lightColor * scatteringPhase * lightEnergy * renderControls.scattering;
    
    // Rim lighting
    float rimFactor = pow(1.0 - density, renderControls.rimPower);
    float3 rimLight = renderControls.highlightColor * rimFactor * renderControls.rimIntensity;
    cloudColor += rimLight;
    
    // Apply brightness
    cloudColor *= renderControls.brightness;
    
    return cloudColor;
}

// Ultimate cloud raymarching function with full control
void UltimateCloudRaymarching_float(
    float2 UV,
    float3 WorldPos,
    float3 ViewDir,
    float3 LightDir,
    float3 LightColor,
    float Time,
    
    // Noise Controls
    float NoiseScale,
    float Frequency1,
    float Frequency2, 
    float Frequency3,
    float Amplitude1,
    float Amplitude2,
    float Amplitude3,
    int NoiseOctaves,
    float Lacunarity,
    float Gain,
    float2 WindDirection,
    float WindSpeed,
    float Turbulence,
    
    // Quality Controls
    int MaxSteps,
    float StepSize,
    float MaxDistance,
    float DensityThreshold,
    float EarlyExitThreshold,
    int LightingSteps,
    float LightStepSize,
    
    // Rendering Controls
    float Coverage,
    float Density,
    float Contrast,
    float Brightness,
    float Absorption,
    float Scattering,
    float3 BaseColor,
    float3 ShadowColor,
    float3 HighlightColor,
    float RimPower,
    float RimIntensity,
    float ShadowIntensity,
    float Softness,
    
    out float4 Color
) {
    // Setup control structures
    NoiseControls noiseControls;
    noiseControls.scale = NoiseScale;
    noiseControls.frequency1 = Frequency1;
    noiseControls.frequency2 = Frequency2;
    noiseControls.frequency3 = Frequency3;
    noiseControls.amplitude1 = Amplitude1;
    noiseControls.amplitude2 = Amplitude2;
    noiseControls.amplitude3 = Amplitude3;
    noiseControls.octaves = NoiseOctaves;
    noiseControls.lacunarity = Lacunarity;
    noiseControls.gain = Gain;
    noiseControls.windDirection = normalize(WindDirection);
    noiseControls.windSpeed = WindSpeed;
    noiseControls.turbulence = Turbulence;
    
    QualityControls qualityControls;
    qualityControls.maxSteps = MaxSteps;
    qualityControls.stepSize = StepSize;
    qualityControls.maxDistance = MaxDistance;
    qualityControls.densityThreshold = DensityThreshold;
    qualityControls.earlyExitThreshold = EarlyExitThreshold;
    qualityControls.lightingSteps = LightingSteps;
    qualityControls.lightStepSize = LightStepSize;
    
    RenderingControls renderControls;
    renderControls.coverage = Coverage;
    renderControls.density = Density;
    renderControls.contrast = Contrast;
    renderControls.brightness = Brightness;
    renderControls.absorption = Absorption;
    renderControls.scattering = Scattering;
    renderControls.baseColor = BaseColor;
    renderControls.shadowColor = ShadowColor;
    renderControls.highlightColor = HighlightColor;
    renderControls.rimPower = RimPower;
    renderControls.rimIntensity = RimIntensity;
    renderControls.shadowIntensity = ShadowIntensity;
    renderControls.softness = Softness;
    
    // Convert UV to cloud coordinates
    float2 cloudPos = (UV - 0.5) * 10.0;
    
    float totalDensity = 0.0;
    float3 accumulatedColor = float3(0, 0, 0);
    float transmittance = 1.0;
    
    // Adaptive step size based on distance
    float adaptiveStepSize = qualityControls.stepSize;
    
    // Raymarching loop
    for (int i = 0; i < qualityControls.maxSteps; i++) {
        float t = float(i) * adaptiveStepSize;
        if (t > qualityControls.maxDistance) break;
        
        // Sample position along the ray
        float2 samplePos = cloudPos + ViewDir.xy * t;
        
        // Get cloud density at this position
        float density = cloudDensityAdvanced(samplePos, Time, noiseControls, renderControls);
        
        if (density > qualityControls.densityThreshold) {
            // Calculate advanced lighting
            float3 lightColor = calculateCloudLightingAdvanced(
                density, LightDir, ViewDir, LightColor, 
                renderControls, qualityControls, samplePos, Time
            );
            
            // Accumulate color with proper alpha blending
            float alpha = density * adaptiveStepSize * 2.0;
            alpha = saturate(alpha);
            
            accumulatedColor += lightColor * alpha * transmittance;
            transmittance *= (1.0 - alpha);
            totalDensity += alpha;
            
            // Early exit if cloud becomes opaque
            if (transmittance < qualityControls.earlyExitThreshold) break;
        }
        
        // Adaptive step size - smaller steps in denser areas
        adaptiveStepSize = lerp(qualityControls.stepSize * 0.5, qualityControls.stepSize, 
                               1.0 - density);
    }
    
    // Final color composition
    float finalAlpha = saturate(totalDensity);
    Color = float4(accumulatedColor, finalAlpha);
}

#endif // CLOUD_RAYMARCHING_INCLUDED