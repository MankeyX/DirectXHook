using System;
using System.Collections.Generic;
using Core.Models;
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
            lock (_createdShaders)
            {
                try
                {
                    if (!_createdShaders.ContainsKey(color))
                        return _createdShaders[Color.White];
                }
                catch
                {
                    EnsureDefaultColors();
                    return _createdShaders[Color.White];
                }

                return _createdShaders[color];
            }
        }

        private void EnsureDefaultColors()
        {
            if (!_createdShaders.ContainsKey(Color.White))
                _createdShaders.Add(Color.White, _device.GenerateShader(1f, 1f, 1f));

            if (!_createdShaders.ContainsKey(Color.Red))
                _createdShaders.Add(Color.Red, _device.GenerateShader(1f, 0f, 0f));
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
