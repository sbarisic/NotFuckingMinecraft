#version 330
// Thanks to <INSERT SHADER SOURCE HERE>

uniform float Time;
uniform vec2 Resolution;

out vec3 Clr;

#ifndef PI
#define PI 3.141592653589793
#endif

varying vec2 surfacePosition;
#define Time Time + (length(surfacePosition)*40. -  pow(length(surfacePosition)*22., 1.11))*sin(surfacePosition.y*0.23)

//http://mouaif.wordpress.com/2009/01/05/photoshop-math-with-glsl-shaders/
#define Blend(base, blend, funcf) 	vec3(funcf(base.r, blend.r), funcf(base.g, blend.g), funcf(base.b, blend.b))
#define BlendNormal(base, blend) 		(blend)
#define BlendOverlay(base, blend) 	Blend(base, blend, BlendOverlayf)
#define BlendOverlayf(base, blend) 	(base < 0.5 ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend)))

#ifndef HALF_PI
#define HALF_PI 1.5707963267948966
#endif

float elasticInOut(float t) {
  return t < 0.5
	? 0.5 * sin(+13.0 * HALF_PI * 2.0 * t) * pow(2.0, 10.0 * (2.0 * t - 1.0))
	: 0.5 * sin(-13.0 * HALF_PI * ((2.0 * t - 1.0) + 1.0)) * pow(2.0, -10.0 * (2.0 * t - 1.0)) + 1.0;
}

float elasticOut(float t) {
  return sin(-13.0 * (t + 1.0) * HALF_PI) * pow(2.0, -10.0 * t) + 1.0;
}

float backOut(float t) {
  float f = 1.0 - t;
  return 1.0 - (pow(f, 3.0) - f * sin(f * PI));
}

highp float random(vec2 co) {
	highp float a = 12.9898;
	highp float b = 78.233;
	highp float c = 43758.5453;
	highp float dt = dot(co.xy, vec2(a, b));
	highp float sn = mod(dt, 3.14);
	return fract(sin(sn) * c);
}

float circle(vec2 uv, float thick, float delay) {
	vec2 p = vec2(uv-0.5);
		
	float t = sin(Time * 1.0 + delay) / 2.0 + 0.5;
	float ease = elasticInOut(t);
	float anim = 30.0 + ease * 40.0;
	
	float size = 0.0 + anim;
	p.x *= Resolution.x/Resolution.y;
	
	float res = min(Resolution.x, Resolution.y);
		float dist = length(p);
	float c = smoothstep(size / res, size/res + 2.0 / res, dist);
	
	float s2 = size-thick;
	float c2 = smoothstep(s2 / res, s2/res + 2.0 / res, dist);
	
	vec2 norm = 2.0 * uv - 1.0;
	float phi = atan(norm.y, norm.x) / PI + Time * 0.15;
	float a = fract(phi);
	
	float rotation = smoothstep(1.5, 0.5, a);
	
	return mix(0.0, c2 - c, rotation);
}

void main() {
	vec2 cuv = gl_FragCoord.xy / Resolution.xy;
	vec3 ccolor = vec3(0, 90./255, 150./255) * 0.1;
	
	vec2 pos = cuv;
	
	pos /= (max(Resolution.x, Resolution.y) * 1.0) / Resolution; //scale it
	pos -= vec2(-0. + 100.0 / Resolution.x, 1.0 - 200.0 / Resolution.y); //offset

	float dist = length(pos);
	dist = smoothstep(2.25, 0.4, dist);
	
	vec3 noise = vec3(random(cuv * 1.5), random(cuv * 2.5), random(cuv));
	ccolor = mix(ccolor, BlendOverlay(ccolor, noise), 0.07);

	ccolor = mix(ccolor, vec3(1.0), circle(cuv, 2.0, 0.0));
	ccolor = mix(ccolor, vec3(1.0), circle(cuv, 12.0, 0.05));
	Clr = ccolor;
}