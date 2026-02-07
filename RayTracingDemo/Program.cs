using Avalonia.Media;
using RayTracing;
using System.Diagnostics;
using Windowing;
namespace RayTracingDemo;

class Program
{
    static void Main(string[] args)
    {
        
        
        double aspect = 16.0 / 9.0;
        int width = 500;
        int samples = 10;
        int maxdepth = 20;
        var scene = new RayTracingAPI.Scene();
        var ground_color = new RayTracingAPI.Color { r = 0.5, g =0.5, b = 0.5 };
        var ground_material = new RayTracingAPI.Lambertian(ground_color);
        scene.AddSphere(1000, new RayTracingAPI.Center{x=0,y=-1000,z=0}, ground_material);
            
        var materials = new List<RayTracingAPI.Material> { ground_material };
        var rng = Random.Shared;
        var avoid = new RayTracingAPI.Center { x = 4, y = 0.2, z = 0 };
        for (int a = -11; a < 11; a++)
        {
            for (int b = -11; b < 11; b++)
            {
                var choose_mat = rng.NextDouble();
                var center = new RayTracingAPI.Center
                {
                    x = a + 0.9 * rng.NextDouble(),
                    y = 0.2,
                    z = b + 0.9 * rng.NextDouble()
                };

                if (Distance(center, avoid) > 0.9)
                {
                    RayTracingAPI.Material sphere_material;

                    if (choose_mat < 0.8)
                    {
                        var albedo = Multiply(RandomColor(rng), RandomColor(rng));
                        sphere_material = new RayTracingAPI.Lambertian(albedo);
                    }
                    else if (choose_mat < 0.95)
                    {
                        var albedo = RandomColor(rng, 0.5, 1.0);
                        var fuzz = RandomRange(rng, 0.0, 0.5);
                        sphere_material = new RayTracingAPI.Metal(albedo, fuzz);
                    }
                    else
                    {
                        sphere_material = new RayTracingAPI.Dielectric(1.5);
                    }

                    materials.Add(sphere_material);
                    scene.AddSphere(0.2, center, sphere_material);
                }
            }
        }
        var cam = new RayTracingAPI.Camera(
            _aspect_ratio: aspect, 
            _image_width: width, 
            _samples_per_pixel: samples, 
            _max_depth: maxdepth,
            _vfov: 20,
            lookfrom: new NativeStructs.Triplet(13, 2, 3),
            lookat: new NativeStructs.Triplet(0, 0, 0),
            _defocus_angle: 0.6,
            _focus_dist: 10.0
        );
        int height = (int)(width /aspect);
        height = (height < 1) ? 1 : height;
        Viewer.Show(width, height, "Rendering of the Ryatracing in a Weekend Cover", updater =>
        {
            var eye = new RayTracingAPI.Eyes("output.png", span =>
            {
                if (updater.IsClosed)
                {
                    return;
                }

                updater.UpdateImage(span);
            }, str =>
            {
                if (updater.IsClosed)
                {
                    return;
                }

                updater.UpdateStatus(str);

            });
            eye.OpenEyes(cam, scene);
        });  
            
            
        
        foreach (var material in materials)
        {
            material.Dispose();
        }

        scene.Dispose();
    }

    static double Distance(RayTracingAPI.Center a, RayTracingAPI.Center b)
    {
        var dx = a.x - b.x;
        var dy = a.y - b.y;
        var dz = a.z - b.z;
        return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
    }

    static double RandomRange(Random rng, double min, double max)
    {
        return min + ((max - min) * rng.NextDouble());
    }

    static RayTracingAPI.Color RandomColor(Random rng)
    {
        return RandomColor(rng, 0.0, 1.0);
    }

    static RayTracingAPI.Color RandomColor(Random rng, double min, double max)
    {
        return new RayTracingAPI.Color
        {
            r = RandomRange(rng, min, max),
            g = RandomRange(rng, min, max),
            b = RandomRange(rng, min, max)
        };
    }
    
    static RayTracingAPI.Color Multiply(RayTracingAPI.Color a, RayTracingAPI.Color b)
    {
        return new RayTracingAPI.Color
        {
            r = a.r * b.r,
            g = a.g * b.g,
            b = a.b * b.b
        };
    }

    
}
