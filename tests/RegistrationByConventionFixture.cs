﻿

using Microsoft.Practices.Unity.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.RegistrationByConvention;
using Unity.RegistrationByConvention.Exceptions;

namespace Microsoft.Practices.Unity.Tests
{
    [TestClass]
    public class RegistrationByConventionFixture
    {
        [TestMethod]
        public void DoesNotRegisterTypeWithNoLifetimeOrInjectionMembers()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterTypes(new[] {typeof(MockLogger)}, getName: t => "name");

            Assert.IsFalse(container.Registrations.Any(r => r.MappedToType == typeof(MockLogger)));
        }

        [TestMethod]
        public void RegistersTypeWithLifetime()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterTypes(new[] {typeof(MockLogger)}, getName: t => "name",
                getLifetimeManager: t => new ContainerControlledLifetimeManager());

            var registrations = container.Registrations.Where(r => r.MappedToType == typeof(MockLogger)).ToArray();

            Assert.AreEqual(1, registrations.Length);
            Assert.AreSame(typeof(MockLogger), registrations[0].MappedToType);
            Assert.AreEqual("name", registrations[0].Name);
            Assert.IsInstanceOfType(registrations[0].LifetimeManager, typeof(ContainerControlledLifetimeManager));
        }

        [TestMethod]
        public void RegistersTypeWithInjectionMembers()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterTypes(new[] {typeof(MockLogger)}, getName: t => "name",
                getInjectionMembers: t => new InjectionMember[] {new InjectionConstructor()});

            var registrations = container.Registrations.Where(r => r.MappedToType == typeof(MockLogger)).ToArray();

            Assert.AreEqual(1, registrations.Length);
            Assert.AreSame(typeof(MockLogger), registrations[0].RegisteredType);
            Assert.AreSame(typeof(MockLogger), registrations[0].MappedToType);
            Assert.AreEqual("name", registrations[0].Name);
            Assert.IsInstanceOfType(registrations[0].LifetimeManager, typeof(TransientLifetimeManager));
        }

        [TestMethod]
        public void RegistersMappingOnlyWithNoLifetimeOrInjectionMembers()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterTypes(new[] {typeof(MockLogger)}, getName: t => "name",
                getFromTypes: t => t.GetTypeInfo().ImplementedInterfaces);

            var registrations = container.Registrations.Where(r => r.MappedToType == typeof(MockLogger)).ToArray();

            Assert.AreEqual(1, registrations.Length);
            Assert.AreSame(typeof(ILogger), registrations[0].RegisteredType);
            Assert.AreSame(typeof(MockLogger), registrations[0].MappedToType);
            Assert.AreEqual("name", registrations[0].Name);
            Assert.IsInstanceOfType(registrations[0].LifetimeManager, typeof(TransientLifetimeManager));
        }

        [TestMethod]
        public void RegistersMappingAndImplementationTypeWithLifetimeAndMixedInjectionMembers()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterTypes(new[] {typeof(MockLogger)}, getName: t => "name",
                getFromTypes: t => t.GetTypeInfo().ImplementedInterfaces,
                getLifetimeManager: t => new ContainerControlledLifetimeManager());

            var registrations = container.Registrations.Where(r => r.MappedToType == typeof(MockLogger)).ToArray();

            Assert.AreEqual(1, registrations.Length);

            var mappingRegistration = registrations.Single(r => r.RegisteredType == typeof(ILogger));

            Assert.AreSame(typeof(ILogger), mappingRegistration.RegisteredType);
            Assert.AreSame(typeof(MockLogger), mappingRegistration.MappedToType);
            Assert.AreEqual("name", mappingRegistration.Name);
            Assert.IsInstanceOfType(mappingRegistration.LifetimeManager, typeof(ContainerControlledLifetimeManager));
        }

        [TestMethod]
        public void RegistersMappingAndImplementationTypeWithLifetime()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterTypes(new[] {typeof(MockLogger)}, getName: t => "name",
                getFromTypes: t => t.GetTypeInfo().ImplementedInterfaces,
                getLifetimeManager: t => new ContainerControlledLifetimeManager());

            var registrations = container.Registrations.Where(r => r.MappedToType == typeof(MockLogger)).ToArray();

            Assert.AreEqual(1, registrations.Length);

            var mappingRegistration = registrations.Single(r => r.RegisteredType == typeof(ILogger));

            Assert.AreSame(typeof(ILogger), mappingRegistration.RegisteredType);
            Assert.AreSame(typeof(MockLogger), mappingRegistration.MappedToType);
            Assert.AreEqual("name", mappingRegistration.Name);
            Assert.IsInstanceOfType(mappingRegistration.LifetimeManager, typeof(ContainerControlledLifetimeManager));
        }

        [TestMethod]
        public void RegistersUsingTheHelperMethods()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterTypes(
                AllClasses
                    .FromAssemblies(typeof(MockLogger).GetTypeInfo().Assembly)
                    .Where(t => t == typeof(MockLogger)),
                WithMappings.FromAllInterfaces,
                WithName.Default,
                global::Unity.RegistrationByConvention.WithLifetime.ContainerControlled
            );

            var registrations = container.Registrations.Where(r => r.MappedToType == typeof(MockLogger)).ToArray();

            Assert.AreEqual(1, registrations.Length);

            var mappingRegistration = registrations.Single(r => r.RegisteredType == typeof(ILogger));

            Assert.AreSame(typeof(ILogger), mappingRegistration.RegisteredType);
            Assert.AreSame(typeof(MockLogger), mappingRegistration.MappedToType);
            Assert.AreEqual(null, mappingRegistration.Name);
            Assert.IsInstanceOfType(mappingRegistration.LifetimeManager, typeof(ContainerControlledLifetimeManager));
        }

        [TestMethod]
        public void RegistersAllTypesWithHelperMethods()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterTypes(AllClasses.FromLoadedAssemblies(), WithMappings.FromAllInterfaces,
                WithName.TypeName, global::Unity.RegistrationByConvention.WithLifetime.ContainerControlled, overwriteExistingMappings: true);
            var registrations = container.Registrations.Where(r => r.MappedToType == typeof(MockLogger)).ToArray();

            Assert.AreEqual(1, registrations.Length);

            var mappingRegistration = registrations.Single(r => r.RegisteredType == typeof(ILogger));

            Assert.AreSame(typeof(ILogger), mappingRegistration.RegisteredType);
            Assert.AreSame(typeof(MockLogger), mappingRegistration.MappedToType);
            Assert.AreEqual("MockLogger", mappingRegistration.Name);
            Assert.IsInstanceOfType(mappingRegistration.LifetimeManager, typeof(ContainerControlledLifetimeManager));
        }

        public void CanResolveTypeRegisteredWithAllInterfaces()
        {
            var container = new UnityContainer();
            container.RegisterTypes(
                AllClasses.FromAssemblies(typeof(MockLogger).GetTypeInfo().Assembly)
                    .Where(t => t == typeof(MockLogger)), WithMappings.FromAllInterfaces, WithName.Default,
                global::Unity.RegistrationByConvention.WithLifetime.ContainerControlled);

            var logger1 = container.Resolve<ILogger>();
            var logger2 = container.Resolve<ILogger>();

            Assert.IsInstanceOfType(logger1, typeof(MockLogger));
            Assert.AreSame(logger1, logger2);
        }

        public void CanResolveGenericTypeMappedWithMatchingInterface()
        {
            var container = new UnityContainer();
            container.RegisterTypes(AllClasses.FromAssemblies(typeof(IList<>).GetTypeInfo().Assembly),
                WithMappings.FromMatchingInterface, WithName.Default, global::Unity.RegistrationByConvention.WithLifetime.None);

            var list = container.Resolve<IList<int>>();

            Assert.IsInstanceOfType(list, typeof(List<int>));
        }

        [TestMethod]
        public void RegistersTypeAccordingToConvention()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterTypes(new TestConventionWithAllInterfaces());

            var registrations = container.Registrations
                .Where(r => r.MappedToType == typeof(MockLogger) || r.MappedToType == typeof(SpecialLogger)).ToArray();

            Assert.AreEqual(2, registrations.Length);
        }

        [TestMethod]
        public void OverridingExistingMappingWithDifferentMappingThrowsByDefault()
        {
            var container = new UnityContainer();
            container.RegisterType<object, string>();

            AssertExtensions.AssertException<DuplicateTypeMappingException>(
                () => container.RegisterTypes(new[] {typeof(int)}, t => new[] {typeof(object)}));
        }

        [TestMethod]
        public void OverridingNewMappingWithDifferentMappingThrowsByDefault()
        {
            var container = new UnityContainer();

            AssertExtensions.AssertException<DuplicateTypeMappingException>(
                () => container.RegisterTypes(new[] {typeof(string), typeof(int)}, t => new[] {typeof(object)}));
        }

        [TestMethod]
        public void OverridingExistingMappingWithSameMappingDoesNotThrow()
        {
            var container = new UnityContainer();
            container.RegisterInstance("a string");
            container.RegisterType<object, string>();

            container.RegisterTypes(new[] {typeof(string)}, t => new[] {typeof(object)});

            Assert.AreEqual("a string", container.Resolve<object>());
        }

        [TestMethod]
        public void CanNotOverrideExistingMappingWithMappingForDifferentName()
        {
            var container = new UnityContainer();
            container.RegisterType<object, string>("string");
            container.RegisterInstance("string", "a string");
            container.RegisterInstance<int>("int", 42);

            container.RegisterTypes(new[] {typeof(int)}, t => new[] {typeof(object)}, t => "int");

            Assert.AreEqual("a string", container.Resolve<object>("string"));
            Assert.AreEqual(42, container.Resolve<object>("int"));
        }

        [TestMethod]
        public void OverridingExistingMappingWithDifferentMappingReplacesMappingIfAllowed()
        {
            var container = new UnityContainer();
            container.RegisterType<object, string>();
            container.RegisterInstance("a string");
            container.RegisterInstance(42);

            container.RegisterTypes(new[] {typeof(int)}, t => new[] {typeof(object)}, overwriteExistingMappings: true);

            Assert.AreEqual(42, container.Resolve<object>());
        }

        [TestMethod]
        public void OverridingNewMappingWithDifferentMappingReplacesMappingIfAllowed()
        {
            var container = new UnityContainer();
            container.RegisterInstance("a string");
            container.RegisterInstance(42);

            container.RegisterTypes(new[] {typeof(string), typeof(int)}, t => new[] {typeof(object)},
                overwriteExistingMappings: true);

            Assert.AreEqual(42, container.Resolve<object>());
        }

        public class TestConventionWithAllInterfaces : RegistrationConvention
        {
            public override IEnumerable<Type> GetTypes()
            {
                yield return typeof(MockLogger);
                yield return typeof(SpecialLogger);
            }

            public override Func<Type, IEnumerable<Type>> GetFromTypes()
            {
                return t => t.GetTypeInfo().ImplementedInterfaces;
            }

            public override Func<Type, string> GetName()
            {
                return t => t.Name;
            }

            public override Func<Type, ITypeLifetimeManager> GetLifetimeManager()
            {
                return t => new ContainerControlledLifetimeManager();
            }

            public override Func<Type, IEnumerable<InjectionMember>> GetInjectionMembers()
            {
                return null;
            }
        }
    }
}