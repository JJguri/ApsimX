﻿using Models;
using Models.Core;
using Models.Core.Interfaces;
using Models.Functions;
using Models.Storage;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace UnitTests.Core
{

    [Serializable]
    class ModelWithLinks : Model
    {
        [Link]
        public Zone[] zones = null;

    }

    [Serializable]
    class ModelWithIFunctions : Model
    {
        [Link]
        public IFunction model2 = null;

    }

    [Serializable]
    class IFunctionProxy : Model, IFunction
    {
        public double value;

        public double Value(int arrayIndex = -1)
        {
            return value;
        }
    }

    [Serializable]
    class ModelWithScopedLinkByName : Model
    {
        [ScopedLinkByName]
        public Zone zone2 = null;

    }

    [Serializable]
    class ModelWithScopedLink : Model
    {
        [ScopedLink]
        public Zone zone2 = null;
    }


    [Serializable]
    class ModelWithChildLink : Model
    {
        [ChildLink]
        public Zone zone2 = null;
    }

    [Serializable]
    class ModelWithChildLinkByName : Model
    {
        [ChildLinkByName]
        public Zone zone2 = null;
    }

    [Serializable]
    class ModelWithParentLink : Model
    {
        [ParentLink]
        public Zone zone = null;

        [ParentLink]
        public Simulation sim = null;
    }

    [Serializable]
    class ModelWithLinkByPath : Model
    {
        [LinkByPath(Path = "[zone2].irrig1")]
        public IIrrigation irrigation1 = null;

        [LinkByPath(Path = ".Simulations.Simulation.zone2.irrig2")]
        public IIrrigation irrigation2 = null;
    }

    [Serializable]
    class ModelWithServices : Model
    {
        [Link]
        public IDataStore storage = null;

        [Link]
        public ILocator locator = null;

        [Link]
        public IEvent events = null;
    }

    [TestFixture]
    class LinksTests
    {

        /// <summary>Ensure the old style [Link] attribute still works.</summary>
        [Test]
        public void EnsureOldStyleLinkWorks()
        {
            var sim = new Simulation()
            {
                Children = new List<Model>()
                {
                    new Clock(),
                    new MockSummary(),
                    new Zone(),
                    new Zone(),
                    new ModelWithLinks()
                }
            };
            Apsim.ParentAllChildren(sim);

            var links = new Links();
            links.Resolve(sim, true);

            var modelWithLinks = sim.Children[4] as ModelWithLinks;
            Assert.AreEqual(modelWithLinks.zones.Length, 2);
            Assert.NotNull(modelWithLinks.zones[0]);
            Assert.NotNull(modelWithLinks.zones[1]);
        }

        /// <summary>Ensure the old style IFunction are linked correctly i.e. treated specially.</summary>
        [Test]
        public void EnsureIFunctionLinksCorrectly()
        {
            var sim = new Simulation()
            {
                Children = new List<Model>()
                {
                    new Clock(),
                    new MockSummary(),
                    new Zone(),
                    new Zone(),
                    new ModelWithIFunctions()
                    {
                        Children = new List<Model>()
                        {
                            new IFunctionProxy()
                            {
                                value = 1,
                                Name = "model1"
                            },
                            new IFunctionProxy()
                            {
                                value = 2,
                                Name = "model2"
                            },
                            new IFunctionProxy()
                            {
                                value = 3,
                                Name = "model3"
                            }
                        }
                    }
                }
            };
            Apsim.ParentAllChildren(sim);

            var links = new Links();
            links.Resolve(sim, true);

            var model = sim.Children[4] as ModelWithIFunctions;

            Assert.AreEqual(model.model2.Value(), 2);
        }

        /// <summary>Ensure a [ScopedLinkByName] works.</summary>
        [Test]
        public void EnsureScopedLinkByNameWorks()
        {
            var sim = new Simulation()
            {
                Children = new List<Model>()
                {
                    new Clock(),
                    new MockSummary(),
                    new Zone() { Name = "zone1" },
                    new Zone() { Name = "zone2" },
                    new ModelWithScopedLinkByName()
                }
            };
            Apsim.ParentAllChildren(sim);

            var links = new Links();
            links.Resolve(sim, true);

            var model = sim.Children[4] as ModelWithScopedLinkByName;
            Assert.AreEqual(model.zone2.Name, "zone2");
        }

        /// <summary>Ensure a [ScopedLink] finds the closest match</summary>
        [Test]
        public void EnsureScopedLinkWorks()
        {
            var sim = new Simulation()
            {
                Children = new List<Model>()
                {
                    new Clock(),
                    new MockSummary(),
                    new Zone() { Name = "zone1" },
                    new Zone() { Name = "zone2" },
                    new ModelWithScopedLink()
                }
            };
            Apsim.ParentAllChildren(sim);

            var links = new Links();
            links.Resolve(sim, true);

            // Should find the closest match.
            var model = sim.Children[4] as ModelWithScopedLink;
            Assert.AreEqual(model.zone2.Name, "zone1");
        }

        /// <summary>Ensure a [ChildLink] finds works</summary>
        [Test]
        public void EnsureChildLinkWorks()
        {
            var sim = new Simulation()
            {
                Children = new List<Model>()
                {
                    new Clock(),
                    new MockSummary(),
                    new ModelWithChildLink()
                    {
                        Children = new List<Model>()
                        {
                            new Zone() { Name = "zone1" },
                        }
                    },
                }
            };
            Apsim.ParentAllChildren(sim);

            var links = new Links();
            links.Resolve(sim, true);

            // Should find zone1 as a match i.e. not use the zones name when doing a match.
            var model = sim.Children[2] as ModelWithChildLink;
            Assert.AreEqual(model.zone2.Name, "zone1");

            // If we now add another child, resolve should fail as there are two matches.
            model.Children.Add(new Zone() { Name = "zone2" }); // added to modelWithChildLink
            Apsim.ParentAllChildren(sim);
            Assert.Throws<Exception>(() =>
            {
                links.Resolve(sim, true);
            });
        }

        /// <summary>Ensure a [ChildLinkByName] finds works</summary>
        [Test]
        public void EnsureChildLinkByNameWorks()
        {
            var sim = new Simulation()
            {
                Children = new List<Model>()
                {
                    new Clock(),
                    new MockSummary(),
                    new ModelWithChildLinkByName()
                    {
                        Children = new List<Model>()
                        {
                            new Zone() { Name = "zone1" },
                            new Zone() { Name = "zone2" }
                        }
                    },
                }
            };
            Apsim.ParentAllChildren(sim);

            var links = new Links();
            links.Resolve(sim, true);

            // Should find zone2 as a match as it uses the fields name.
            var model = sim.Children[2] as ModelWithChildLinkByName;
            Assert.AreEqual(model.zone2.Name, "zone2");
        }

        /// <summary>Ensure a [ParentLink] works</summary>
        [Test]
        public void EnsureParentLinkWorks()
        {
            var sim = new Simulation()
            {
                Children = new List<Model>()
                {
                    new Clock(),
                    new MockSummary(),
                    new Zone()
                    {
                        Name = "zone1",
                        Children  = new List<Model>()
                        {
                            new ModelWithParentLink()
                        }
                    },
                    new Zone() { Name = "zone2" }
                }
            };
            Apsim.ParentAllChildren(sim);

            var links = new Links();
            links.Resolve(sim, true);

            // Should find the closest match.
            var model = sim.Children[2].Children[0] as ModelWithParentLink;
            Assert.AreEqual(model.zone.Name, "zone1");
            Assert.AreEqual(model.sim.Name, "Simulation");
        }

        /// <summary>Ensure a [LinkByPath] works</summary>
        [Test]
        public void EnsureLinkByPathWorks()
        {
            var sim = new Simulation()
            {
                Children = new List<Model>()
                {
                    new Clock(),
                    new MockSummary(),
                    new Zone()
                    {
                        Name = "zone1",
                        Children  = new List<Model>()
                        {
                            new ModelWithLinkByPath()
                        }
                    },
                    new Zone()
                    {
                        Name = "zone2",
                        Children  = new List<Model>()
                        {
                            new MockIrrigation() { Name = "irrig1" },
                            new MockIrrigation() { Name = "irrig2" }
                        }
                    },
                }
            };
            Apsim.ParentAllChildren(sim);

            var links = new Links();
            links.Resolve(sim, true);

            var model = sim.Children[2].Children[0] as ModelWithLinkByPath;
            var zone2 = sim.Children[3];
            Assert.AreEqual(model.irrigation1, zone2.Children[0]);
            Assert.AreEqual(model.irrigation2, zone2.Children[1]);
        }

        /// <summary>Ensure link can resolve services</summary>
        [Test]
        public void EnsureServicesResolve()
        {
            var simulations = new Simulations()
            {
                Children = new List<Model>()
                {
                    new DataStore(),
                    new Simulation()
                    {
                        Children = new List<Model>()
                        {
                            new Clock(),
                            new MockSummary(),
                            new ModelWithServices()
                            
                        }
                    }
                 }
            };
            Apsim.ParentAllChildren(simulations);

            var links = new Links();
            links.Resolve(simulations.Children[1], true);

            var modelWithServices = simulations.Children[1].Children[2] as ModelWithServices;
            Assert.IsNotNull(modelWithServices.storage);
            Assert.IsNotNull(modelWithServices.locator);
            Assert.IsNotNull(modelWithServices.events);
        }

    }
}
