using System;
using System.Linq;

namespace User.ActiveBeltTensioner.Normalizers
{
    internal static class NormalizerPipeline
    {
        private static readonly INormalizer[] Normalizers =
            typeof(INormalizer).Assembly
                .GetTypes()
                .Where(normalizerType =>
                    typeof(INormalizer).IsAssignableFrom(normalizerType) &&
                    !normalizerType.IsInterface &&
                    !normalizerType.IsAbstract)
                .Select(normalizerType => (INormalizer)Activator.CreateInstance(normalizerType))
                .ToArray();

        public static void Normalize(ref TelemetrySnapshot telemetrySnapshot)
        {
            for (int i = 0; i < Normalizers.Length; i++)
            {
                if (Normalizers[i].Normalize(ref telemetrySnapshot))
                {
                    break;
                }
            }
        }
    }
}
