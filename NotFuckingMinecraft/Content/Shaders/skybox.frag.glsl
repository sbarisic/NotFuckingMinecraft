#version 330

uniform sampler2D TEX;
uniform float Time;

in vec3 oColor;
in vec2 oUV;

out vec4 Clr;

highp float random(vec2 co) {
	highp float a = 12.9898;
	highp float b = 78.233;
	highp float c = 43758.5453;
	highp float dt = dot(co.xy, vec2(a, b));
	highp float sn = mod(dt, 3.14);
	return fract(sin(sn) * c);
}

void main() {
	vec4 Tex = texture(TEX, oUV);
	//Tex.xyz *= 1 - 0.18 * random(oUV * Time); // I don't like grain
	Clr = Tex;
}