#version 330

uniform sampler2D TEX;
uniform sampler2D TEX2;
uniform float Time;
uniform vec2 Resolution;

uniform vec4 Settings;

in vec2 oUV;

out vec4 Clr;

vec2 Pixelate(vec2 UV) {
	float PS = 3.0; // Pixel size
	float d = 1.0 / PS;
	
	int fx = int(UV.s * Resolution.x / PS);
	int fy = int(UV.t * Resolution.y / PS);

	float s = PS * (float(fx) + d) / Resolution.x;
	float t = PS * (float(fy) + d) / Resolution.y;

	return vec2(s, t);
}

vec3 FXAA() {
  float FXAA_SPAN_MAX = 8.0;
  float FXAA_REDUCE_MUL = 1.0/8.0;
  float FXAA_REDUCE_MIN = (1.0/128.0);
  
  vec2 texcoordOffset = vec2(1.0f / Resolution.x, 1.0f / Resolution.y);

  vec3 rgbNW = texture2D(TEX, oUV.xy + (vec2(-1.0, -1.0) * texcoordOffset)).xyz;
  vec3 rgbNE = texture2D(TEX, oUV.xy + (vec2(+1.0, -1.0) * texcoordOffset)).xyz;
  vec3 rgbSW = texture2D(TEX, oUV.xy + (vec2(-1.0, +1.0) * texcoordOffset)).xyz;
  vec3 rgbSE = texture2D(TEX, oUV.xy + (vec2(+1.0, +1.0) * texcoordOffset)).xyz;
  vec3 rgbM  = texture2D(TEX, oUV.xy).xyz;
	
  vec3 luma = vec3(0.299, 0.587, 0.114);
  float lumaNW = dot(rgbNW, luma);
  float lumaNE = dot(rgbNE, luma);
  float lumaSW = dot(rgbSW, luma);
  float lumaSE = dot(rgbSE, luma);
  float lumaM  = dot( rgbM, luma);
	
  float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
  float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));
	
  vec2 dir;
  dir.x = -((lumaNW + lumaNE) - (lumaSW + lumaSE));
  dir.y =  ((lumaNW + lumaSW) - (lumaNE + lumaSE));
	
  float dirReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * (0.25 * FXAA_REDUCE_MUL), FXAA_REDUCE_MIN);
	  
  float rcpDirMin = 1.0/(min(abs(dir.x), abs(dir.y)) + dirReduce);
	
  dir = min(vec2(FXAA_SPAN_MAX,  FXAA_SPAN_MAX), max(vec2(-FXAA_SPAN_MAX, -FXAA_SPAN_MAX), dir * rcpDirMin)) * texcoordOffset;
		
  vec3 rgbA = (1.0/2.0) * (texture2D(TEX, oUV.xy + dir * (1.0 / 3.0 - 0.5)).xyz + texture2D(TEX, oUV.xy + dir * (2.0 / 3.0 - 0.5)).xyz);
  vec3 rgbB = rgbA * (1.0/2.0) + (1.0/4.0) * (texture2D(TEX, oUV.xy + dir * (0.0/3.0 - 0.5)).xyz + texture2D(TEX, oUV.xy + dir * (3.0 / 3.0 - 0.5)).xyz);
  float lumaB = dot(rgbB, luma);

  if((lumaB < lumaMin) || (lumaB > lumaMax)) { return rgbA; } else { return rgbB; }
}

float depth(vec2 uv) {
	float n = 1.0; // camera z near
	float f = 2000.0; // camera z far
	float z = texture2D(TEX2, uv).x;
	//return (2.0 * n) / (f + n - z * (f - n));
	return (n * z) / (f - z * (f - n));
}

vec3 gamma(vec3 color) {
	// TODO?
	//return pow(color, vec3(1.0 / 2.0));
	return color;
}

void main() {
	vec3 Tx;

	if (Settings.x == 1 && Settings.y == 0) // FXAA and no wireframe?
		Tx = FXAA();
	else 
		Tx = texture(TEX, Pixelate(oUV)).xyz;

	vec3 Fog = mix(vec3(1, 1, 1), vec3(0 + 0.5, 30.f/255 + 0.5, 30.f/255 + 0.5), depth(oUV));

	Clr = vec4(gamma(Tx * Fog), 1);
}