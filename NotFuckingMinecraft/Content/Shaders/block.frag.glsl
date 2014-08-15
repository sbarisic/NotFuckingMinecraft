#version 330

uniform sampler2D TEX;
uniform vec4 Settings;

flat in int BlockID;
in vec4 oColor;
in vec2 oUV;
in vec3 oPos;
in vec3 Normal;

out vec4 Clr;

highp float random(vec2 co) {
	highp float a = 12.9898;
	highp float b = 78.233;
	highp float c = 43758.5453;
	highp float dt = dot(co.xy, vec2(a, b));
	highp float sn = mod(dt, 3.14);
	return fract(sin(sn) * c);
}

#define WATER 3

vec4 WireColor(int BID) {
	if (BID == WATER)
		return vec4(0, 0, 1, 0.8);

	return vec4(1, 1, 1, 1);
}

void main() {
	vec4 Tex = texture(TEX, oUV);

	float FogFactor = gl_FragCoord.z / gl_FragCoord.w / 1500;
	vec3 Fog = mix(vec3(1, 1, 1), vec3(.65, .65, .65), FogFactor);

	float Mult = 1;
	/*if (Normal.z == 1 || Normal.x == 1 || Normal.y == 1)
		Mult = 1;*/

	Clr = vec4(Tex.rgb * oColor.rgb * Fog * Mult, oColor.a * Tex.a);

	if (Settings.y == 1) // Wireframe
		Clr = WireColor(BlockID) * vec4(Fog, 1);
}