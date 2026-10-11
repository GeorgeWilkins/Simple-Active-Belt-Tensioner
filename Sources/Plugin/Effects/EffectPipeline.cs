using SimHub;
using System;
using System.Linq;
using User.ActiveBeltTensioner;

namespace User.ActiveBeltTensioner.Effects
{
    public sealed class EffectPipeline
    {
        private readonly IEffect[] _effects;

        private static readonly Type[] EffectTypes =
            typeof(IEffect).Assembly
                .GetTypes()
                .Where(t =>
                    typeof(IEffect).IsAssignableFrom(t) &&
                    !t.IsInterface &&
                    !t.IsAbstract)
                .ToArray();

        public EffectPipeline(PhysicsForceCalculator calculator)
        {
            _effects = EffectTypes
                .Select(t => (IEffect)Activator.CreateInstance(t, calculator))
                .ToArray();

            Logging.Current.Info($"SABT: Detected {_effects.Length} effects classes: {string.Join(", ", _effects.Select(e => e.GetType().Name))}");
        }

        public void Apply(DevicePlugin.TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets)
        {
            for (int i = 0; i < _effects.Length; i++)
            {
                _effects[i].Apply(telemetrySnapshot, millisecondsSinceLastUpdate, ref beltTensionTargets);
            }
        }
    }
}

