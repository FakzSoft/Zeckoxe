﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Zeckoxe.EntityComponentSystem.Technical.Serialization.BinarySerializer.ConverterAction
{
    internal static class UnmanagedConverter
    {
        private static readonly MethodInfo _writeMethod = typeof(UnmanagedConverter).GetTypeInfo().GetDeclaredMethod(nameof(Write));
        private static readonly MethodInfo _writeArrayMethod = typeof(UnmanagedConverter).GetTypeInfo().GetDeclaredMethod(nameof(WriteArray));
        private static readonly MethodInfo _readMethod = typeof(UnmanagedConverter).GetTypeInfo().GetDeclaredMethod(nameof(Read));
        private static readonly MethodInfo _readArrayMethod = typeof(UnmanagedConverter).GetTypeInfo().GetDeclaredMethod(nameof(ReadArray));

        [SuppressMessage("Performance", "RCS1242:Do not pass non-read-only struct by read-only reference.")]
        private static void Write<T>(in StreamWriterWrapper writer, in T value) where T : unmanaged => writer.Write(value);

        [SuppressMessage("Performance", "RCS1242:Do not pass non-read-only struct by read-only reference.")]
        private static void WriteArray<T>(in StreamWriterWrapper writer, in T[] value) where T : unmanaged => writer.WriteArray(value);

        private static T Read<T>(in StreamReaderWrapper reader) where T : unmanaged => reader.Read<T>();

        private static T[] ReadArray<T>(in StreamReaderWrapper reader) where T : unmanaged => reader.ReadArray<T>();

        public static (WriteAction<T>, ReadAction<T>) GetActions<T>() => (
            (WriteAction<T>)_writeMethod.MakeGenericMethod(typeof(T)).CreateDelegate(typeof(WriteAction<T>)),
            (ReadAction<T>)_readMethod.MakeGenericMethod(typeof(T)).CreateDelegate(typeof(ReadAction<T>)));

        public static (WriteAction<T>, ReadAction<T>) GetArrayActions<T>() => (
            (WriteAction<T>)_writeArrayMethod.MakeGenericMethod(typeof(T).GetElementType()).CreateDelegate(typeof(WriteAction<T>)),
            (ReadAction<T>)_readArrayMethod.MakeGenericMethod(typeof(T).GetElementType()).CreateDelegate(typeof(ReadAction<T>)));
    }
}
