using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Memory;
using Core.Models;
using Hook.D3D11.Extensions;
using Hook.Infrastructure;
using Vortice.Direct3D11;

namespace Hook.D3D11
{
    public unsafe class DrawIndexedHook : IDisposable
    {
        private const int IndexByteWidthDivider = 100;

        // The hook handler has to be static, so only one DrawIndexedHook can be active at a time.
        private static DrawIndexedHook _instance;

        private readonly ID3D11DeviceContext _deviceContext;
        private readonly FunctionHook _drawIndexedHook;
        private readonly ID3D11DepthStencilState _depthDisabledState;
        private readonly ID3D11BlendState _alphaBlendState;
        private readonly List<ModelInfo> _modelsInScene;
        private readonly Shaders _shaders;
        private readonly ServerInterface _serverInterface;

        private int _currentIndex;
        private int _selectedByteWidth;
        private ModelInfo _currentItem;
        private bool _hasReportedError;

        public bool OnlyRenderSavedModels { get; private set; }
        public bool IsLoggerEnabled { get; private set; }
        public List<ModelInfo> SavedModels { get; set; }

        public DrawIndexedHook(
            ID3D11DeviceContext deviceContext,
            ID3D11DepthStencilState depthDisabledState,
            ID3D11BlendState alphaBlendState,
            Shaders shaders,
            ServerInterface serverInterface
        )
        {
            _deviceContext = deviceContext;
            _depthDisabledState = depthDisabledState;
            _alphaBlendState = alphaBlendState;
            _shaders = shaders;
            _serverInterface = serverInterface;
            _modelsInScene = new List<ModelInfo>();
            SavedModels = new List<ModelInfo>();

            _instance = this;
            _drawIndexedHook = new FunctionHook(
                VirtualTableAddress.GetVtableAddress(_deviceContext.NativePointer, (int) VirtualTableIndices.D3D11DeviceContext.DrawIndexed),
                (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, uint, uint, int, void>)&OnDrawIndexedHook);

            _serverInterface.Message("DrawIndexedHook.ctor");
            _drawIndexedHook.Activate();
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static void OnDrawIndexedHook(IntPtr deviceContextPointer, uint indexCount, uint startIndex, int baseVertexLocation)
        {
            var instance = _instance;

            try
            {
                instance.OnDrawIndexed(deviceContextPointer, indexCount, startIndex, baseVertexLocation);
            }
            catch (Exception e)
            {
                // An exception must not escape into the target process. Only report the first one,
                // since this runs for every draw call.
                if (!instance._hasReportedError)
                {
                    instance._hasReportedError = true;
                    instance._serverInterface.Message($"DrawIndexed hook failed: {e}");
                }
            }
        }

        private void CallOriginal(IntPtr deviceContextPointer, uint indexCount, uint startIndex, int baseVertexLocation)
        {
            var original = (delegate* unmanaged[Stdcall]<IntPtr, uint, uint, int, void>)_drawIndexedHook.FunctionToHook;
            original(deviceContextPointer, indexCount, startIndex, baseVertexLocation);
        }

        private void OnDrawIndexed(IntPtr deviceContextPointer, uint indexCount, uint startIndex, int baseVertexLocation)
        {
            if (indexCount < 7)
            {
                CallOriginal(deviceContextPointer, indexCount, startIndex, baseVertexLocation);
                return;
            }

            if (!OnlyRenderSavedModels)
            {
                CallOriginal(deviceContextPointer, indexCount, startIndex, baseVertexLocation);
            }

            var originalBlendFactor = stackalloc float[4];
            _deviceContext.OMGetBlendState(out var originalBlendState, originalBlendFactor, out var originalMaskRef);
            _deviceContext.OMGetDepthStencilState(out var defaultDepthState, out var stencilRef);
            using (defaultDepthState)
            {
                _currentItem = GetCurrentItem((int)indexCount);

                if (IsLoggerEnabled)
                {
                    _deviceContext.OMSetDepthStencilState(_depthDisabledState, stencilRef);
                    if (_selectedByteWidth == _deviceContext.GetIndexByteWidth() / IndexByteWidthDivider)
                    {
                        if (!_modelsInScene.Contains(_currentItem))
                            _modelsInScene.Add(_currentItem);

                        if (IsCurrentModel(_currentItem, _currentIndex))
                        {
                            DrawCustom(indexCount, startIndex, baseVertexLocation, Color.Red);
                            return;
                        }

                        DrawCustom(indexCount, startIndex, baseVertexLocation, Color.White);
                        return;
                    }
                }

                lock (SavedModels)
                {
                    if (IsSavedModel(_currentItem, out var index))
                    {
                        _deviceContext.OMSetBlendState(_alphaBlendState);
                        _deviceContext.OMSetDepthStencilState(_depthDisabledState, stencilRef);
                        DrawCustom(indexCount, startIndex, baseVertexLocation, SavedModels[index].Color);
                    }
                }
                
                _deviceContext.OMSetDepthStencilState(defaultDepthState, stencilRef);
                _deviceContext.OMSetBlendState(originalBlendState, originalBlendFactor, originalMaskRef);

                originalBlendState?.Dispose();
            }
        }

        private void DrawCustom(uint indexCount, uint startIndex, int baseVertexLocation, Color color)
        {
            // Disable depth testing so we can see the object through walls, and color it with a shader to make it more visible
            _deviceContext.PSSetShader(_shaders.Get(color));
            _deviceContext.DrawIndexed(indexCount, startIndex, baseVertexLocation);
        }

        private ModelInfo GetCurrentItem(int indexCount)
        {
            return _deviceContext.GetModelInfo(indexCount);
        }

        private bool IsCurrentModel(ModelInfo currentModel, int currentIndex)
        {
            return
                _modelsInScene[currentIndex] != null &&
                _modelsInScene[currentIndex].Equals(currentModel);
        }

        private bool IsSavedModel(ModelInfo model, out int savedItemIndex)
        {
            return (savedItemIndex = SavedModels.BinarySearch(model)) >= 0;
        }

        public int ChangeModel(int change)
        {
            // Wrap for easy navigation
            if (_currentIndex + change < 0)
                _currentIndex = _modelsInScene.Count - 1;
            else if (_currentIndex + change >= _modelsInScene.Count)
                _currentIndex = 0;
            else
                _currentIndex += change;

            return _currentIndex;
        }

        public int ChangeSelectedByteWidth(int change)
        {
            _selectedByteWidth += _selectedByteWidth + change < 0
                ? 0
                : change;

            _currentIndex = 0;

            return _selectedByteWidth;
        }

        public bool ToggleLogger()
        {
            _selectedByteWidth = 0;
            _currentIndex = 0;
            IsLoggerEnabled = !IsLoggerEnabled;

            if (!IsLoggerEnabled)
            {
                _modelsInScene.Clear();
                _serverInterface.Message("DrawIndexedHook.ToggleLogger False");
            }

            return IsLoggerEnabled;
        }

        public void RenderSavedModelsOnly(bool enabled)
        {
            OnlyRenderSavedModels = enabled;
        }

        public ModelInfo GetSelectedModel()
        {
            if (_modelsInScene.Count <= _currentIndex)
            {
                return null;
            }

            return _modelsInScene[_currentIndex];
        }

        public void Dispose()
        {
            _drawIndexedHook?.Deactivate();
            _drawIndexedHook?.Dispose();
            _depthDisabledState?.Dispose();
            _alphaBlendState.Dispose();

            // Don't dispose _deviceContext, it will crash the application or game
        }
    }
}