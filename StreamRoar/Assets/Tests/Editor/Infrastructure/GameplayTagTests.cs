using System;
using System.Collections.Generic;
using NUnit.Framework;
using StreamRoar.Infrastructure;

namespace StreamRoar.Tests.Editor.Infrastructure
{
    [TestFixture]
    [Category("Infrastructure")]
    public sealed class GameplayTagTests
    {
        GameplayTagManager m_Manager;

        [SetUp]
        public void SetUp()
        {
            m_Manager = GameplayTagManager.Create(new NativeGameplayTagSource());
        }

        [Test]
        public void RequestTag_RegisteredPath_ReturnsValidTag()
        {
            GameplayTag burn = m_Manager.RequestTag(NativeGameplayTagSource.StateDebuffBurn);

            Assert.That(burn.IsValid, Is.True);
            Assert.That(m_Manager.GetPath(burn), Is.EqualTo(NativeGameplayTagSource.StateDebuffBurn));
        }

        [Test]
        public void RequestTag_UnknownPath_Throws()
        {
            Assert.Throws<ArgumentException>(() => m_Manager.RequestTag("State.Unknown.Thing"));
        }

        [Test]
        public void Register_InvalidPath_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                GameplayTagManager.Create(new InlineTagSource("1Bad", "State.", "State..Burn", "")));
        }

        [Test]
        public void ImplicitParent_IsRequestable()
        {
            GameplayTag debuff = m_Manager.RequestTag(NativeGameplayTagSource.StateDebuff);
            Assert.That(debuff.IsValid, Is.True);
        }

        [Test]
        public void IsChildOf_Burn_IsChildOfDebuffAndState()
        {
            GameplayTag burn = m_Manager.RequestTag(NativeGameplayTagSource.StateDebuffBurn);
            GameplayTag debuff = m_Manager.RequestTag(NativeGameplayTagSource.StateDebuff);
            GameplayTag state = m_Manager.RequestTag(NativeGameplayTagSource.State);

            Assert.That(m_Manager.IsChildOf(burn, burn), Is.True);
            Assert.That(m_Manager.IsChildOf(burn, debuff), Is.True);
            Assert.That(m_Manager.IsChildOf(burn, state), Is.True);
            Assert.That(m_Manager.IsChildOf(debuff, burn), Is.False);
        }

        [Test]
        public void Container_HasExact_AndIncludeChildren()
        {
            GameplayTag burn = m_Manager.RequestTag(NativeGameplayTagSource.StateDebuffBurn);
            GameplayTag debuff = m_Manager.RequestTag(NativeGameplayTagSource.StateDebuff);
            GameplayTag haste = m_Manager.RequestTag(NativeGameplayTagSource.StateBuffHaste);

            var container = new GameplayTagContainer();
            container.Add(burn);

            Assert.That(container.Has(burn), Is.True);
            Assert.That(container.Has(debuff), Is.False);
            Assert.That(container.Has(debuff, includeChildren: true, m_Manager), Is.True);
            Assert.That(container.Has(haste, includeChildren: true, m_Manager), Is.False);
        }

        [Test]
        public void Container_HasAny_HasAll()
        {
            GameplayTag burn = m_Manager.RequestTag(NativeGameplayTagSource.StateDebuffBurn);
            GameplayTag haste = m_Manager.RequestTag(NativeGameplayTagSource.StateBuffHaste);
            GameplayTag attack = m_Manager.RequestTag(NativeGameplayTagSource.AbilityAttack);

            var container = new GameplayTagContainer();
            container.Add(burn);
            container.Add(haste);

            Assert.That(
                container.HasAny(new[] { attack, burn }, includeChildren: false, m_Manager),
                Is.True);
            Assert.That(
                container.HasAll(new[] { burn, haste }, includeChildren: false, m_Manager),
                Is.True);
            Assert.That(
                container.HasAll(new[] { burn, attack }, includeChildren: false, m_Manager),
                Is.False);
        }

        [Test]
        public void DuplicatePath_MergesWithoutThrowing()
        {
            var manager = GameplayTagManager.Create(
                new InlineTagSource(NativeGameplayTagSource.StateDebuffBurn),
                new InlineTagSource(NativeGameplayTagSource.StateDebuffBurn));

            Assert.That(manager.RequestTag(NativeGameplayTagSource.StateDebuffBurn).IsValid, Is.True);
        }

        [Test]
        public void RuntimeRegister_AfterSeal_Throws()
        {
            Assert.Throws<InvalidOperationException>(() =>
                ((IGameplayTagRegistration)m_Manager).Register("Runtime.Only"));
        }

        sealed class InlineTagSource : IGameplayTagSource
        {
            readonly string[] m_Paths;

            public InlineTagSource(params string[] paths)
            {
                m_Paths = paths;
            }

            public void Collect(IGameplayTagRegistration registration)
            {
                foreach (string path in m_Paths)
                    registration.Register(path);
            }
        }
    }
}
