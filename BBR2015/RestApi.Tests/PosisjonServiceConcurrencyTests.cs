using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Database;
using Database.Entities;
using NUnit.Framework;
using Repository;

namespace RestApi.Tests
{
    [TestFixture]
    public class PosisjonServiceConcurrencyTests
    {
        [Test]
        [Explicit("funker ikke så bra lenger")]
        public void Test_om_mange_tråder_skaper_trøbbel_i_tjenesten()
        {
            var overridableSettings = new OverridableSettings();
            var service = new NoDbPosisjonsService(null, overridableSettings);
            
            var concurrentList = new ConcurrentStack<Exception>();
            var finished = 0;
            var registrert = 0;
            var pollet = 0;
            var tasks = new Task[100];
            for (var i = 0; i < tasks.Length; i++)
            {
                var task = new Task(() =>
                {
                    int j = 0;
                    while (j < 100)
                    {
                        try
                        {
                            service.RegistrerPosisjon("Lag1", "Deltaker1", 1.0, 1.0);
                            //Interlocked.Increment(ref registrert);
                        }
                        catch (Exception e)
                        {
                            concurrentList.Push(e);
                        }
                        Thread.Sleep(1);
                        j++;
                    }

                    Interlocked.Increment(ref finished);
                });
                tasks[i] = task;
            }
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i].Start();
            }

            var pollTasks = new Task[100];
            for (var i = 0; i < pollTasks.Length; i++)
            {
                var task = new Task(() =>
                {
                    int k = 0;
                    while (k < 100)
                    {
                        try
                        {
                            service.HentforLag("Lag1");
                            //Interlocked.Increment(ref pollet);
                        }
                        catch (Exception e)
                        {
                            concurrentList.Push(e);
                        }
                        Thread.Sleep(1);
                        k++;
                    }
                    Interlocked.Increment(ref finished);
                });
                pollTasks[i] = task;
            }
            for (var i = 0; i < pollTasks.Length; i++)
            {
                pollTasks[i].Start();
            }

            while (finished != tasks.Length + pollTasks.Length)
            {
                Thread.Sleep(100);
            }
            Debug.Write(string.Format("{0}-{1}", registrert, pollet));
            Assert.AreEqual(0, concurrentList.Count, "Skulle ikke vært noen Exceptions");
        }
    }

    class NoDbPosisjonsService : PosisjonsService
    {
        public NoDbPosisjonsService(DataContextFactory dataContextFactory, OverridableSettings appSettings) : base(dataContextFactory, appSettings, null)
        {
        }

        protected override ConcurrentDictionary<string, EksternDeltakerPosisjon> HentFraDatabasen()
        {
            return new ConcurrentDictionary<string, EksternDeltakerPosisjon>();
        }

        protected override void Lagre(DeltakerPosisjon posisjon)
        {
            
        }
    }
}
