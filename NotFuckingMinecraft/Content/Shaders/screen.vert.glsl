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

out vec2 oUV;

void main() {
	gl_Position = vec4(Position, 1);
	oUV = UV;
}