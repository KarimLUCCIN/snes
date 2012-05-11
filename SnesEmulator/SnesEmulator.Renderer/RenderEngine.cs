using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpDX.DXGI;
using SharpDX.Direct3D10;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device1;
using DriverType = SharpDX.Direct3D10.DriverType;
using System.Windows.Interop;
using System.ComponentModel;
using SharpDX.D3DCompiler;
using SharpDX;
using SharpDX.Direct3D;
using System.Diagnostics;

namespace SnesEmulator.Renderer
{
    public class RenderEngine : IDisposable, INotifyPropertyChanged
    {
        private Form hwndRenderingWindow;

        public RenderEngine()
        {

        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        private DX10ImageSource wpfImage;

        public D3DImage  WPFImage
        {
            get { return wpfImage; }
        }
        
        private Texture2D renderBuffer;
        private RenderTargetView renderView;
        private Device device;
        private SwapChain swapChain;

        /// <summary>
        /// Crée le Device DirectX 10 et initialise les resources
        /// </summary>
        public void Initialize(int width, int height)
        {
            if (width <= 2)
                throw new ArgumentOutOfRangeException("width");
            else if (height <= 2)
                throw new ArgumentOutOfRangeException("height");

            Width = width;
            Height = height;

            Clean();

            hwndRenderingWindow = new Form();
            hwndRenderingWindow.Width = hwndRenderingWindow.Height = 100;

            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(Width, Height,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = hwndRenderingWindow.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, desc, out device, out swapChain);

            //device = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport, SharpDX.Direct3D10.FeatureLevel.Level_10_0);

            Texture2DDescription colordesc = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.Shared,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            renderBuffer = new Texture2D(device, colordesc);
            renderView = new RenderTargetView(device, renderBuffer);

            LoadRenderShader();

            wpfImage = new DX10ImageSource();
            wpfImage.SetRenderTargetDX10(renderBuffer);

            RaisePropertyChanged("WPFImage");
        }

        struct RenderVertex
        {
            public Vector4 pos;
            public Vector2 tex;
        }

        InputLayout layout;
        Buffer vertices;
        Buffer indices;
        Buffer constants;
        VertexBufferBinding vertices_binding;

        VertexShader vertexShader;
        PixelShader pixelShader;

        Effect fillEffect;
        EffectPass fillPass;

        private void LoadRenderShader()
        {
            var shaderBytes = ShaderBytecode.Compile(Properties.Resources.FillShader, "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
            fillEffect = new Effect(device, shaderBytes);

            EffectTechnique technique = fillEffect.GetTechniqueByIndex(0); ;
            fillPass = technique.GetPassByIndex(0);


            // Compile Vertex and Pixel shaders
            var vertexShaderByteCode = ShaderBytecode.Compile(Properties.Resources.FillShader, "vs_main", "vs_4_0");
            vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.Compile(Properties.Resources.FillShader, "ps_main", "ps_4_0");
            pixelShader = new PixelShader(device, pixelShaderByteCode);
           
            // Layout from VertexShader input signature
            layout = new InputLayout(device, ShaderSignature.GetInputSignature(vertexShaderByteCode), new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0)
                    });

            // Instantiate Vertex buiffer from vertex data
            vertices = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                                  {
                                      new RenderVertex(){pos = new Vector4(1.0f, -1.0f, 0.0f, 1.0f), tex = new Vector2(1,1)},
                                      new RenderVertex(){pos = new Vector4(-1.0f, -1.0f, 0.0f, 1.0f), tex = new Vector2(0,1)},
                                      new RenderVertex(){pos = new Vector4(-1.0f, 1.0f, 0.0f, 1.0f), tex = new Vector2(0,0)},
                                      new RenderVertex(){pos = new Vector4(1.0f, 1.0f, 0.0f, 1.0f), tex = new Vector2(1,0)},
                                  });
            vertices_binding = new VertexBufferBinding(vertices, Utilities.SizeOf<Vector2>() + Utilities.SizeOf<Vector4>(), 0);

            indices = Buffer.Create(device, BindFlags.IndexBuffer, new short[] { 0, 1, 2, 2, 3, 0 });

            // Create Constant Buffer
            //constants = new Buffer(device, Utilities.SizeOf<Vector2>() + Utilities.SizeOf<Vector4>(), ResourceUsage.Dynamic, BindFlags.ShaderResource, CpuAccessFlags.Write, ResourceOptionFlags.None);



            device.Rasterizer.SetViewports(new Viewport(0, 0, Width, Height, 0.0f, 1.0f)); 
            
            RasterizerStateDescription rsd = new RasterizerStateDescription()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
            };
            RasterizerState rsdState = new RasterizerState(device, rsd);
            device.Rasterizer.State = rsdState;

            options.halfPixel = new Vector2(0.5f / (float)Width,0.5f / (float)Height);
            options.color = new Vector4(1, 0, 1, 1);
        }

        struct ShaderOptions
        {
            public Vector2 halfPixel;
            public Vector4 color;
        }

        ShaderOptions options = new ShaderOptions();

        private object sync = new object();

        bool rendering = false;

        Stopwatch time_i = null;

        public void RenderPass()
        {
            lock (sync)
            {
                if (rendering)
                    return;
                else
                {
                    rendering = true;

                    if(time_i == null)
                    {
                        time_i = new Stopwatch();
                        time_i.Start();
                    }

                    time_i.Stop();

                    var elapsed = time_i.ElapsedMilliseconds;

                    time_i.Restart();

                    try
                    {
                        if (wpfImage != null)
                        {
                            // Clear views
                            device.OutputMerger.SetTargets(renderView);

                            device.ClearRenderTargetView(renderView, new Color4(new Vector4(1, 0, 0, 1)));

                            device.InputAssembler.SetVertexBuffers(0, vertices_binding);
                            device.InputAssembler.SetIndexBuffer(indices, Format.R16_UInt, 0);
                            device.InputAssembler.InputLayout = layout;
                            device.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                            options.color = new Vector4((float)((options.color.X + elapsed * 0.001f) % (Math.PI * 2)), 0, 0, 0);

                            fillEffect.GetVariableByName("halfPixel").AsVector().Set(options.halfPixel);
                            fillEffect.GetVariableByName("OverlayColor").AsVector().Set(options.color);

                            fillPass.Apply();

                            device.DrawIndexed(6, 0, 0);
                            swapChain.Present(0, PresentFlags.None);

                            wpfImage.InvalidateD3DImage();
                        }
                    }
                    finally
                    {
                        rendering = false;
                    }
                }
            }
        }

        #region IDisposable Members

        ~RenderEngine()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clean();

                GC.SuppressFinalize(this);
            }
        }

        private void Clean()
        {
            Disposer.SafeDispose(ref hwndRenderingWindow);

            if (device != null)
            {
                device.ClearState();
                device.Dispose();
                device = null;
            }
            
            Disposer.SafeDispose(ref fillPass);
            Disposer.SafeDispose(ref fillEffect);

            Disposer.SafeDispose(ref vertexShader);
            Disposer.SafeDispose(ref pixelShader);

            Disposer.SafeDispose(ref vertices);
            Disposer.SafeDispose(ref indices);

            Disposer.SafeDispose(ref constants);
            Disposer.SafeDispose(ref layout);

            Disposer.SafeDispose(ref renderBuffer);
            Disposer.SafeDispose(ref renderView);
            Disposer.SafeDispose(ref swapChain);

            Disposer.SafeDispose(ref wpfImage);

            RaisePropertyChanged("WPFImage");
        }
        
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
