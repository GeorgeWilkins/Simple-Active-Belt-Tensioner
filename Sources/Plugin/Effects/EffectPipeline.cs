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
                .Where(effectType =>
                    typeof(IEffect).IsAssignableFrom(effectType) &&
                    !effectType.IsInterface &&
                    !effectType.IsAbstract)
                .ToArray();

        public EffectPipeline(PhysicsForceCalculator calculator)
        {
            _effects = EffectTypes
                .Select(effectType => (IEffect)Activator.CreateInstance(effectType))
                .OrderBy(effect => effect.Order)
                .ToArray();

            Logging.Current.Info($"SABT: Detected {_effects.Length} effects classes: {string.Join(", ", _effects.Select(e => e.GetType().Name))}");
        }

        public void Apply(DeviceSettings settings, DevicePlugin.TelemetrySnapshot telemetrySnapshot, double millisecondsSinceLastUpdate, ref BeltTensionTargets beltTensionTargets)
        {
            for (int i = 0; i < _effects.Length; i++)
            {
                _effects[i].Apply(settings, telemetrySnapshot, millisecondsSinceLastUpdate, ref beltTensionTargets);
            }
        }
    }
}
