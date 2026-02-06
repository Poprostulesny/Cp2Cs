using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace RayTracing;

internal static partial class NativeMethods 
{   
    private const string LibName = "rt";
    
    public partial class NativeStructs;

    internal sealed class MaterialSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal MaterialSafeHandle() : base(true) { }

        internal MaterialSafeHandle(nint handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            DestroyMaterial(handle);
            return true;
        }
    }

    internal sealed class SceneSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SceneSafeHandle() : base(true) { }

        internal SceneSafeHandle(nint handle) : base(true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            DestroyScene(handle);
            return true;
        }
    }

   
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RenderCallback(int samples, nint buffer);
    
    [LibraryImport(LibName, EntryPoint = "CreateLambertian")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial nint CreateLambertianRaw(double r, double g, double b);

    internal static MaterialSafeHandle CreateLambertian(double r, double g, double b)
    {
        return new MaterialSafeHandle(CreateLambertianRaw(r, g, b));
    }

    [LibraryImport(LibName, EntryPoint = "CreateDielectric")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial nint CreateDielectricRaw(double refreaction_index);

    internal static MaterialSafeHandle CreateDielectric(double refreaction_index)
    {
        return new MaterialSafeHandle(CreateDielectricRaw(refreaction_index));
    }
    
    [LibraryImport(LibName, EntryPoint = "CreateMetal")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial nint CreateMetalRaw(double r, double g, double b, double fuzz);

    internal static MaterialSafeHandle CreateMetal(double r, double g, double b, double fuzz)
    {
        return new MaterialSafeHandle(CreateMetalRaw(r, g, b, fuzz));
    }

    [LibraryImport(LibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void DestroyMaterial(nint material);

    [LibraryImport(LibName, EntryPoint = "CreateScene")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial nint CreateSceneRaw();

    internal static SceneSafeHandle CreateScene()
    {
        return new SceneSafeHandle(CreateSceneRaw());
    }

    [LibraryImport(LibName, EntryPoint = "SceneAddSphere")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial void SceneAddSphereRaw(nint scene, RayTracing.NativeStructs.Triplet center, double radius,
        nint material);

    internal static void SceneAddSphere(SceneSafeHandle scene, RayTracing.NativeStructs.Triplet center, double radius,
        MaterialSafeHandle material)
    {
        nint sceneHandle = scene.DangerousGetHandle();
        nint materialHandle = material.DangerousGetHandle();
        SceneAddSphereRaw(sceneHandle, center, radius, materialHandle);
        GC.KeepAlive(scene);
        GC.KeepAlive(material);
    }

    [LibraryImport(LibName, EntryPoint = "RenderScene")]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static partial void RenderSceneRaw(RayTracing.NativeStructs.Camera config, nint scene, nint buffer, RenderCallback callback);

    internal static void RenderScene(RayTracing.NativeStructs.Camera config, SceneSafeHandle scene, nint buffer, RenderCallback callback)
    {
        RenderSceneRaw(config, scene.DangerousGetHandle(), buffer, callback);
        GC.KeepAlive(scene);
    }

    [LibraryImport(LibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void DestroyScene(nint scene);

    [LibraryImport(LibName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial int SavePng(int w, int h, nint buffer, [MarshalAs(UnmanagedType.LPStr)] string pathname);


}
