#version 330

uniform sampler2D TEX;
uniform mat4 u_modelview;
uniform mat4 u_projection;
uniform mat4 u_view;
uniform mat4 u_viewrot;
uniform float Time;

in vec4 Data;
in vec3 Position;
in vec3 Color;
in vec2 UV;

flat out int BlockID;
out vec2 oUV;
out vec4 oColor;
out vec3 oPos;
out vec3 Normal;

highp float random(vec2 co) {
	highp float a = 12.9898;
	highp float b = 78.233;
	highp float c = 43758.5453;
	highp float dt = dot(co.xy, vec2(a, b));
	highp float sn = mod(dt, 3.14);
	return fract(sin(sn) * c);
}

vec3 Randomize(vec3 Ps, float XA, float YA, float ZA) {
	float XOFF = XA / 2;
	float YOFF = YA / 2;
	float ZOFF = ZA / 2;

	vec3 V = (u_modelview * vec4(Ps, 1)).xyz;
	return vec3(Ps.x + (XOFF - random(V.yz) * XA),
	Ps.y + (YOFF - random(V.xz) * YA), Ps.z + (ZOFF - random(V.xy) * ZA));
}

#define WATER 3

#define BLOCK_SIZE 10.0
#define HALF_BLOCK_SIZE 0.5

void main() {
	vec4 Clr = vec4(Color, 1);
	vec3 Pos = Position;

	if (Data.w == WATER) {
		Pos = Randomize(Position, 0, 0, 0.1 * BLOCK_SIZE * sin(Time * 100) * 5);
		Pos.z += sin(Time * 100) / 10 * BLOCK_SIZE - 0.35 * BLOCK_SIZE;
	}

	gl_Position = u_projection * u_viewrot * u_view * u_modelview * vec4(Pos, 1);

	BlockID = int(Data.w);
	Normal = (u_modelview * vec4(Data.xyz, 0)).xyz;
	oColor = Clr;
	oUV = UV;
	oPos = gl_Position.xyz;
}