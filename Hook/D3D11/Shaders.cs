using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Hook.D3D11.Extensions;
using SharpDX.Direct3D11;

namespace Hook.D3D11
{
    public class Shaders : IDisposable
    {
        private readonly Device _device;
        private readonly Dictionary<Color, PixelShader> _createdShaders;

        public Shaders(Device device)
        {
            _device = device;
            _createdShaders = new Dictionary<Color, PixelShader>();
        }

        public void Generate(IEnumerable<Color> colors)
        {
            lock (_createdShaders)
            {
                Dispose();
                _createdShaders.Clear();

                _createdShaders.Add(Color.White, _device.GenerateShader(1f, 1f, 1f));
                _createdShaders.Add(Color.Red, _device.GenerateShader(1f, 0f, 0f));

                foreach (var color in colors)
                {
                    if (_createdShaders.ContainsKey(color))
                        continue;

                    _createdShaders.Add(color, _device.GenerateShader(color.Rf, color.Gf, color.Bf));
                }
            }
        }

        public PixelShader Get(Color color)
        {
            try
            {
                lock (_createdShaders)
                {
                    if (!_createdShaders.ContainsKey(color))
                        return _createdShaders[Color.White];

                    return _createdShaders[color];
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return null;
        }

        public void Dispose()
        {
            lock (_createdShaders)
            {
                foreach (var createdShader in _createdShaders)
                    createdShader.Value.Dispose();
            }
        }
    }
}
