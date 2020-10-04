﻿namespace Zeckoxe.EntityComponentSystem.Technical
{
    internal interface IEntityContainer
    {
        void Add(int entityId);

        void Remove(int entityId);
    }
}
