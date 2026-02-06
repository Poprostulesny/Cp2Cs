using System.Runtime.InteropServices;

namespace RayTracing;

public class RayTracingAPI
{
    public struct Color
    {
        public double r, g, b;
    }

    public struct Center
    {
        public double x, y, z;
    }

    public struct Triplet
    {
        public double x, y, z;
    }
    public abstract class Material : IDisposable
    {
        private NativeMethods.MaterialSafeHandle? _handle;
        internal NativeMethods.MaterialSafeHandle Handle => _handle ?? throw new ObjectDisposedException(nameof(Material));

        internal Material(NativeMethods.MaterialSafeHandle handle)
        {
            _handle = handle;
        }

        public void Dispose()
        {
            if (_handle != null)
            {
                _handle.Dispose();
                _handle = null;
            }
            GC.SuppressFinalize(this);
        }
    }
    
    public class Metal : Material
    {
        public Color color;
        public double fuzz;
        
        public Metal(Color _color, double _fuzz):base(NativeMethods.CreateMetal(_color.r, _color.g, _color.b , _fuzz))
        {
            color = _color;
            fuzz = _fuzz;
           

        }

        public Metal(double r, double g, double b, double _fuzz) : base(NativeMethods.CreateMetal( r, g,b ,_fuzz))
        {
            color = new Color
            {
                r = r,
                g = g,
                b = b
            };
            fuzz = _fuzz;

        }

    }

    public class Dielectric : Material
    {
        public double RefractiveIndex;

        public Dielectric(double refractiveIndex) : base(NativeMethods.CreateDielectric(refractiveIndex))
        {
            RefractiveIndex = refractiveIndex;
        }
        
    }

    public class Lambertian : Material
    {
        public Color color;
        public Lambertian(Color _color):base(NativeMethods.CreateLambertian(_color.r, _color.g, _color.b))
        {
            color = _color;
        }

        public Lambertian(double r, double g, double b) : base(NativeMethods.CreateLambertian( r, g,b))
        {
            color = new Color
            {
                r = r,
                g = g,
                b = b
            };
        }
    }

    public class Camera
    {
        internal NativeStructs.Camera native_camera;
       
        public Camera(double _aspect_ratio = 1.0,
            int _image_width = 100,
            int _samples_per_pixel = 10,
            int _max_depth = 10,
            double _vfov = 90,
            NativeStructs.Triplet? lookfrom = null,
            NativeStructs.Triplet? lookat = null,
            NativeStructs.Triplet? vup = null,
            double _defocus_angle = 0,
            double _focus_dist = 10)
        {
            
            native_camera.aspect_ratio      = _aspect_ratio;  // Ratio of image width over height
            native_camera.image_width       = _image_width;  // Rendered image width in pixel count
            native_camera.samples_per_pixel= _samples_per_pixel;   // Count of random samples for each pixel
            native_camera.max_depth         = _max_depth;   // Maximum number of ray bounces into scene
            
            native_camera.vfov     = _vfov;              // Vertical view angle (field of view)
            native_camera.defocus_angle = _defocus_angle;  // Variation angle of rays through each pixel
            native_camera.focus_dist = _focus_dist;
          
            
            if (lookfrom == null)
            {
                native_camera.lookfrom = new NativeStructs.Triplet{x=0,y=0,z=0};
            }
            else
            {
                native_camera.lookfrom = lookfrom.Value;
            }

            if (lookat == null)
            {
                native_camera.lookat = new NativeStructs.Triplet{x=0,y=0,z=-1};
            }
            else
            {
                native_camera.lookat = lookat.Value;
            }
            if (vup == null)
            {
                native_camera.vup = new NativeStructs.Triplet{x=0,y=1,z=0};
            }
            else
            {
                native_camera.vup = vup.Value;
            }
            
            
        }
    }

    public class Scene : IDisposable
    {
        private NativeMethods.SceneSafeHandle? _handle;
        internal NativeMethods.SceneSafeHandle Handle => _handle ?? throw new ObjectDisposedException(nameof(Scene));
        public Scene()
        {
            _handle = NativeMethods.CreateScene();
        }
        
        public void Dispose()
        {
            if (_handle != null)
            {
                _handle.Dispose();
                _handle = null;
            }
            GC.SuppressFinalize(this);
        }

        public void AddSphere(double _radius, Center _center, Material _material)
        {
            NativeStructs.Triplet c = new  NativeStructs.Triplet{x=_center.x,y=_center.y,z=_center.z};
            NativeMethods.SceneAddSphere(Handle, c, _radius, _material.Handle);
        }
        
    }

    public class Eyes
    {
        public string? path_to_save;
        private readonly NativeMethods.RenderCallback _callback;
        public Eyes(string? _path_to_save = null)
        {
            path_to_save= _path_to_save;
            _callback = OnRender;
        }

        private void OnRender(int samples, nint buffer)
        {
        }
        
        public void OpenEyes(Camera camera, Scene scene)
        {
            int height = (int)(camera.native_camera.image_width / camera.native_camera.aspect_ratio);
            height = (height < 1) ? 1 : height;
            byte[] buffer = new byte[height * 4 * camera.native_camera.image_width];
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {   
                nint ptr = handle.AddrOfPinnedObject();
                NativeMethods.RenderScene(camera.native_camera, scene.Handle, ptr, _callback);
                if (path_to_save != null)
                {
                    NativeMethods.SavePng(camera.native_camera.image_width, height,  ptr, path_to_save);
                }
            }
            finally{
                handle.Free();
            }
        }
           
    }

}
