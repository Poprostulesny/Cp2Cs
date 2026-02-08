using RayTracing;
using Windowing;

namespace RayTracingDemo;

internal class Program
{
    private static void Main(string[] args)
    {
        var aspect = 16.0 / 9.0;
        var width = 500;
        var samples = 200;
        var maxdepth = 30;

        var scene = new RayTracingAPI.Scene();

        var ground_color = new RayTracingAPI.Color { r = 0.5, g = 0.5, b = 0.5 };
        var ground_material = new RayTracingAPI.Lambertian(ground_color);
        scene.AddSphere(1000, new RayTracingAPI.Center { x = 0, y = -1000, z = 0 }, ground_material);

        var materials = new List<RayTracingAPI.Material> { ground_material };
        var rng = Random.Shared;
        var avoid = new RayTracingAPI.Center { x = 4, y = 0.2, z = 0 };
        for (var a = -11; a < 11; a++)
        for (var b = -11; b < 11; b++)
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

        var big_d_ball = new RayTracingAPI.Dielectric(1.5);
        var big_m_ball = new RayTracingAPI.Metal(0.7, 0.6, 0.5, 0);
        var big_l_ball = new RayTracingAPI.Lambertian(0.4, 0.2, 0.1);
        materials.Add(big_d_ball);
        materials.Add(big_m_ball);
        materials.Add(big_l_ball);
        scene.AddSphere(1, new RayTracingAPI.Center { x = 0, y = 1, z = 0 }, big_d_ball);
        scene.AddSphere(1, new RayTracingAPI.Center { x = -4, y = 1, z = 0 }, big_l_ball);
        scene.AddSphere(1, new RayTracingAPI.Center { x = 4, y = 1, z = 0 }, big_m_ball);


        var cam = new RayTracingAPI.Camera(
            aspect,
            width,
            samples,
            maxdepth,
            20,
            new NativeStructs.Triplet(13, 2, 3),
            new NativeStructs.Triplet(0, 0, 0),
            _defocus_angle: 0.6,
            _focus_dist: 10.0
        );

        var height = (int)(width / aspect);
        height = height < 1 ? 1 : height;
        Viewer.Show(width, height, "Rendering of the Raytracing in a Weekend Cover", updater =>
        {
            var eye = new RayTracingAPI.Eyes("output.png", span =>
            {
                if (updater.IsClosed) return;

                updater.UpdateImage(span);
            }, str =>
            {
                if (updater.IsClosed) return;

                updater.UpdateStatus(str);
            });
            eye.OpenEyes(cam, scene);
        });


        foreach (var material in materials) material.Dispose();

        scene.Dispose();
    }

    private static double Distance(RayTracingAPI.Center a, RayTracingAPI.Center b)
    {
        var dx = a.x - b.x;
        var dy = a.y - b.y;
        var dz = a.z - b.z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    private static double RandomRange(Random rng, double min, double max)
    {
        return min + (max - min) * rng.NextDouble();
    }

    private static RayTracingAPI.Color RandomColor(Random rng)
    {
        return RandomColor(rng, 0.0, 1.0);
    }

    private static RayTracingAPI.Color RandomColor(Random rng, double min, double max)
    {
        return new RayTracingAPI.Color
        {
            r = RandomRange(rng, min, max),
            g = RandomRange(rng, min, max),
            b = RandomRange(rng, min, max)
        };
    }

    private static RayTracingAPI.Color Multiply(RayTracingAPI.Color a, RayTracingAPI.Color b)
    {
        return new RayTracingAPI.Color
        {
            r = a.r * b.r,
            g = a.g * b.g,
            b = a.b * b.b
        };
    }
}