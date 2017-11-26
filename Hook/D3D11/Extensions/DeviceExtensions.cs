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
float4 main() : SV_Target
{{
    return float4({r},{g},{b},0.7f);
}}");

            using (var pixelShaderBytecode = ShaderBytecode.Compile(shaderText, "main", "ps_4_0"))
            {
                return new PixelShader(device, pixelShaderBytecode);
            }
        }
    }
}
