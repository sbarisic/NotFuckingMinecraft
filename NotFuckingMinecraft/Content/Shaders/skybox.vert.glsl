#version 330

uniform sampler2D TEX;
uniform mat4 u_modelview;
uniform mat4 u_projection;
uniform mat4 u_view;
uniform mat4 u_viewrot;

flat in unsigned int Idx;
in vec3 Position;
in vec3 Color;
in vec2 UV;

out vec2 oUV;
out vec3 oColor;

void main() {
	gl_Position = u_projection * u_viewrot * u_modelview * vec4(Position, 1);
	oColor = Color;
	oUV = UV;
}