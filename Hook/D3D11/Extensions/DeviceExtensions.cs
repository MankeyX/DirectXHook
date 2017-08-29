using System.Text;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace Hook.D3D11.Extensions
{
    public static class DeviceExtensions
    {
        public static DepthStencilState CreateDepthStencilState(this Device device, bool depth, bool writeEnable)
        {
            var description = new DepthStencilStateDescription
            {
                IsDepthEnabled = depth,
                DepthWriteMask = writeEnable ? DepthWriteMask.All : DepthWriteMask.Zero,
                DepthComparison = Comparison.Always
            };

            return new DepthStencilState(device, description);
        }

        public static PixelShader GenerateShader(this Device device, float r, float g, float b)
        {
            var shaderText = Encoding.ASCII.GetBytes(
$@"
struct VS_OUT
{{
    float4 position : SV_POSITION;
    float4 Color : COLOR0;
}};

float4 main(VS_OUT input) : SV_Target
{{
	float4 fake;
    fake.r = {r};
    fake.g = {g};
    fake.b = {b};
    fake.a = 0.7f;
    return fake;
}}");

            using (var pixelShaderBytecode = ShaderBytecode.Compile(shaderText, "main", "ps_4_0"))
            {
                return new PixelShader(device, pixelShaderBytecode);
            }
        }
    }
}
