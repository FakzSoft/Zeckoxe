﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Zeckoxe.Core;
using Zeckoxe.Desktop;
using Zeckoxe.Engine;
using Zeckoxe.Games;
using Zeckoxe.GLTF;
using Zeckoxe.Graphics;
using Zeckoxe.Physics;
using Buffer = Zeckoxe.Graphics.Buffer;

namespace Samples.Samples
{

    public class LoadGLTF : Game, IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct TransformUniform
        {
            public TransformUniform(Matrix4x4 p, Matrix4x4 m, Matrix4x4 v)
            {
                P = p;
                M = m;
                V = v;
            }

            public Matrix4x4 P;

            public Matrix4x4 M;

            public Matrix4x4 V;

            public void Update(Camera camera, Matrix4x4 m)
            {
                P = camera.Projection;
                M = m;
                V = camera.View;
            }
        }


        public LoadGLTF() : base()
        {
            Parameters.Settings.Validation = false;
            Window.Title += "Zeckoxe Engine - (LoadGLTF) ";
        }




        public Camera Camera { get; set; }
        public GameTime GameTime { get; set; }
        public GLTFLoader GLTFModel { get; set; }
        public int[] Indices { get; set; }
        public VertexPositionNormal[] Vertices { get; set; }


        public Dictionary<string, DescriptorSet> DescriptorSets = new Dictionary<string, DescriptorSet>();
        public Dictionary<string, Buffer> Buffers = new Dictionary<string, Buffer>();
        public Dictionary<string, PipelineState> PipelineStates = new Dictionary<string, PipelineState>();
        public Dictionary<string, ShaderBytecode> Shaders = new Dictionary<string, ShaderBytecode>();

        // TransformUniform 
        public TransformUniform uniform;
        public Matrix4x4 Model;
        public float yaw;
        public float pitch;
        public float roll;


        public override void Initialize()
        {
            base.Initialize();

            Camera = new Camera()
            {
                Mode = CameraType.Free,
                Position = new Vector3(1, -.3f, -3.5f),
            };

            Camera.SetLens(Window.Width, Window.Height);


            // Reset Model
            Model = Matrix4x4.Identity;

            uniform = new TransformUniform(Camera.Projection, Model, Camera.View);


            GLTFModel = new GLTFLoader("Models/model.glb");

            Indices = GLTFModel.Indices;
            Vertices = GLTFModel.GetPositionNormalAsArray();


            CreateBuffers();
            CreatePipelineState();


            // This example only uses one descriptor type (uniform buffer) and only requests one descriptor of this type
            List<DescriptorPool> pool = new()
            {
                new  DescriptorPool(DescriptorType.UniformBuffer, 1),
            };

            DescriptorSets["Descriptor1"] = new DescriptorSet(PipelineStates["Wireframe"], pool);
            DescriptorSets["Descriptor1"].SetUniformBuffer(0, Buffers["ConstBuffer1"]); // Binding 0: Uniform buffer (Vertex shader)


            DescriptorSets["Descriptor2"] = new DescriptorSet(PipelineStates["Solid"], pool);
            DescriptorSets["Descriptor2"].SetUniformBuffer(0, Buffers["ConstBuffer2"]); // Binding 0: Uniform buffer (Vertex shader)

            yaw = 3.15f;
            pitch = 0;
            roll = 3.15f;
        }


        public void CreateBuffers()
        {
            Buffers["VertexBuffer"] = new Buffer(Device, new BufferDescription()
            {
                BufferFlags = BufferFlags.VertexBuffer,
                Usage = GraphicsResourceUsage.Dynamic,
                SizeInBytes = Interop.SizeOf(Vertices),
            });
            Buffers["VertexBuffer"].SetData(Vertices);


            Buffers["IndexBuffer"] = new Buffer(Device, new BufferDescription()
            {
                BufferFlags = BufferFlags.IndexBuffer,
                Usage = GraphicsResourceUsage.Dynamic,
                SizeInBytes = Interop.SizeOf(Indices),
            });
            Buffers["IndexBuffer"].SetData(Indices);


            Buffers["ConstBuffer1"] = new Buffer(Device, new BufferDescription()
            {
                BufferFlags = BufferFlags.ConstantBuffer,
                Usage = GraphicsResourceUsage.Dynamic,
                SizeInBytes = Interop.SizeOf<TransformUniform>(),
            });

            Buffers["ConstBuffer2"] = new Buffer(Device, new BufferDescription()
            {
                BufferFlags = BufferFlags.ConstantBuffer,
                Usage = GraphicsResourceUsage.Dynamic,
                SizeInBytes = Interop.SizeOf<TransformUniform>(),
            });




        }


        public void CreatePipelineState()
        {
            Shaders["Fragment"] = ShaderBytecode.LoadFromFile("Shaders/Transformations/shader.frag", ShaderStage.Fragment);
            Shaders["Vertex"] = ShaderBytecode.LoadFromFile("Shaders/Transformations/shader.vert", ShaderStage.Vertex);


            List<VertexInputAttribute> VertexAttributeDescriptions = new()
            {

                new VertexInputAttribute
                {
                    Binding = 0,
                    Location = 0,
                    Format = PixelFormat.R32G32B32SFloat,
                    Offset = 0,
                },
                new VertexInputAttribute
                {
                    Binding = 0,
                    Location = 1,
                    Format = PixelFormat.R32G32B32SFloat,
                    Offset = 12,
                }
            };

            List<VertexInputBinding> VertexBindingDescriptions = new()
            {
                new VertexInputBinding
                {
                    Binding = 0,
                    InputRate = VertexInputRate.Vertex,
                    Stride = VertexPositionNormal.Size,
                }
            };


            PipelineStates["Wireframe"] = new PipelineState(new PipelineStateDescription() 
            {
                Framebuffer = Framebuffer,

                Layouts =
                {
                    // Binding 0: Uniform buffer (Vertex shader)
                    new DescriptorSetLayout()
                    {
                        Stage = ShaderStage.Vertex,
                        Type = DescriptorType.UniformBuffer,
                        Binding = 0,
                    }
                },

                InputAssemblyState = InputAssemblyState.Default(),

                RasterizationState = new RasterizationState()
                {
                    FillMode = FillMode.Wireframe,
                    CullMode = CullMode.None,
                    FrontFace = FrontFace.Clockwise,
                },
                PipelineVertexInput = new PipelineVertexInput
                {
                    VertexAttributeDescriptions = VertexAttributeDescriptions,
                    VertexBindingDescriptions = VertexBindingDescriptions,
                },
                Shaders =
                {
                    Shaders["Fragment"],
                    Shaders["Vertex"],
                },


            });


            PipelineStates["Solid"] = new PipelineState(new PipelineStateDescription() 
            {
                Framebuffer = Framebuffer,

                Layouts =
                {
                    // Binding 0: Uniform buffer (Vertex shader)
                    new DescriptorSetLayout()
                    {
                        Stage = ShaderStage.Vertex,
                        Type = DescriptorType.UniformBuffer,
                        Binding = 0,
                    }
                },

                InputAssemblyState = InputAssemblyState.Default(),

                RasterizationState = new RasterizationState()
                {
                    FillMode = FillMode.Solid,
                    CullMode = CullMode.None,
                    FrontFace = FrontFace.Clockwise,
                },
                PipelineVertexInput = new PipelineVertexInput
                {
                    VertexAttributeDescriptions = VertexAttributeDescriptions,
                    VertexBindingDescriptions = VertexBindingDescriptions,
                },
                Shaders =
                {
                    Shaders["Fragment"],
                    Shaders["Vertex"],
                },

            });
        }



        public override void Update(GameTime game)
        {
            Camera.Update(game);


            Model = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix4x4.CreateTranslation(-1.0f, 0.0f, 0.0f);
            uniform.Update(Camera, Model);
            Buffers["ConstBuffer1"].SetData(ref uniform);


            Model = Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix4x4.CreateTranslation(1.0f, 0.0f, 0.0f);
            uniform.Update(Camera, Model);
            Buffers["ConstBuffer2"].SetData(ref uniform);


            yaw += 0.0006f * MathF.PI;
        }




        public override void BeginDraw()
        {
            base.BeginDraw();

            CommandBuffer commandBuffer = Context.CommandBuffer;

            commandBuffer.BeginFramebuffer(Framebuffer, .5f, .5f, .5f);
            commandBuffer.SetViewport(Window.Width, Window.Height, 0, 0);
            commandBuffer.SetScissor(Window.Width, Window.Height, 0, 0);

            commandBuffer.SetVertexBuffers(new Buffer[] { Buffers["VertexBuffer"] });
            commandBuffer.SetIndexBuffer(Buffers["IndexBuffer"]);



            commandBuffer.SetGraphicPipeline(PipelineStates["Wireframe"]);
            commandBuffer.BindDescriptorSets(DescriptorSets["Descriptor1"]);
            commandBuffer.DrawIndexed(Indices.Length, 1, 0, 0, 0);


            commandBuffer.SetGraphicPipeline(PipelineStates["Solid"]);
            commandBuffer.BindDescriptorSets(DescriptorSets["Descriptor2"]);
            commandBuffer.DrawIndexed(Indices.Length, 1, 0, 0, 0);
        }



        public void Dispose()
        {
            Adapter.Dispose();
        }
    }
}
