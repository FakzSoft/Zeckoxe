﻿// Copyright (c) 2019-2021 Faber Leonardo. All Rights Reserved. https://github.com/FaberSanZ
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)



using Vortice.Vulkan;


namespace Vultaik
{
    public class InputAssemblyState
    {
        public VkPrimitiveTopology PrimitiveType { get; set; }
        public bool PrimitiveRestartEnable { get; set; }

        public InputAssemblyState(VkPrimitiveTopology Type, bool RestartEnable = false)
        {
            PrimitiveType = Type;
            PrimitiveRestartEnable = RestartEnable;
        }

        public InputAssemblyState()
        {
            PrimitiveType = VkPrimitiveTopology.TriangleList;
            PrimitiveRestartEnable = false;
        }


        public static InputAssemblyState Default() => new InputAssemblyState()
        {
            PrimitiveRestartEnable = false,
            PrimitiveType = VkPrimitiveTopology.TriangleList
        };

    }
}
