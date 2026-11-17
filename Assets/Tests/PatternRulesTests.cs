using NUnit.Framework;
using PatternSurvivor.Patterns.Builder;
using PatternSurvivor.Patterns.Composite;
using PatternSurvivor.Patterns.Decorator;
using PatternSurvivor.Patterns.Flyweight;
using PatternSurvivor.Patterns.Prototype;
using UnityEngine;

namespace PatternSurvivor.Tests
{
    public sealed class PatternRulesTests
    {
        [Test]
        public void PrototypeCloneIsIndependent()
        {
            EnemyPrototype original = EnemyPrototypeCatalog.Get("Tank");
            EnemyPrototype clone = original.Clone();
            clone.CurrentHealth += 10;

            Assert.That(clone, Is.Not.SameAs(original));
            Assert.That(original.CurrentHealth, Is.EqualTo(65));
            Assert.That(clone.CurrentHealth, Is.EqualTo(75));
        }

        [Test]
        public void BuilderCreatesIndependentValidConfigurations()
        {
            WeaponConfig first = new WeaponBuilder().WithDamage(20).Build();
            WeaponConfig second = new WeaponBuilder(first).WithDamage(40).Build();

            Assert.That(first.Damage, Is.EqualTo(20));
            Assert.That(second.Damage, Is.EqualTo(40));
            Assert.That(first, Is.Not.SameAs(second));
            Assert.That(second.FireRate, Is.GreaterThan(0f));
        }

        [Test]
        public void FlyweightReturnsSameReferencePerType()
        {
            EnemySharedData first = EnemyDataCache.Get("Normal");
            EnemySharedData second = EnemyDataCache.Get("Normal");
            EnemySharedData other = EnemyDataCache.Get("Fast");

            Assert.That(first, Is.SameAs(second));
            Assert.That(first, Is.Not.SameAs(other));
        }

        [Test]
        public void CompositeExecutesEachChildOnce()
        {
            var composite = new CompositeSkill();
            var first = new CountingSkill();
            var second = new CountingSkill();
            composite.Add(first);
            composite.Add(second);

            composite.Execute(new SkillContext());

            Assert.That(first.Count, Is.EqualTo(1));
            Assert.That(second.Count, Is.EqualTo(1));
        }

        [Test]
        public void DecoratorInvokesInnerAndAddsEffect()
        {
            var spy = new SpyWeapon();
            var decorator = new FireWeaponDecorator(spy);
            var context = new FireContext
            {
                Direction = Vector2.right,
                Config = WeaponDirector.BuildBasic(),
                SpawnProjectile = (_, _, _, _, _, _) => { }
            };

            decorator.Fire(context);

            Assert.That(spy.FireCount, Is.EqualTo(1));
            Assert.That(context.Effects.HasFlag(ProjectileEffects.Burn), Is.True);
            Assert.That(context.BonusDamage, Is.EqualTo(2));
        }

        private sealed class CountingSkill : ISkill
        {
            public int Count { get; private set; }
            public string Name => "CountingSkill";
            public void Execute(SkillContext context) => Count++;
        }

        private sealed class SpyWeapon : IWeapon
        {
            public int FireCount { get; private set; }
            public WeaponConfig Config { get; } = WeaponDirector.BuildBasic();
            public void Fire(FireContext context) => FireCount++;
        }
    }
}
