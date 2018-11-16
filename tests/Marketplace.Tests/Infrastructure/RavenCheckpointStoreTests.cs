using System;
using System.Threading.Tasks;
using AutoFixture;
using EventStore.ClientAPI;
using FluentAssertions;
using Marketplace.Infrastructure.RavenDB;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Xunit;

namespace Marketplace.Tests.Infrastructure
{
    public class RavenCheckpointStoreTests : IDisposable
    {
        public RavenCheckpointStoreTests()
        {
            AutoFixture = new Fixture();
            GetDocumentSession = () => LazyStore.Value.OpenAsyncSession();
        }

        public void Dispose() => LazyStore.Value?.Dispose();

        Func<IAsyncDocumentSession> GetDocumentSession { get; }
        Fixture AutoFixture { get; }

        static readonly Lazy<IDocumentStore> LazyStore = new Lazy<IDocumentStore>(() =>
        {
            var store = new DocumentStore {
                Urls     = new[] {"http://localhost:8080"},
                Database = "Marketplace"
            };

            return store.Initialize();
        });

        [Fact]
        public async Task can_set_and_get_checkpoint()
        {
            var sut = new RavenCheckpointStore(GetDocumentSession);
            var projection = AutoFixture.Create<string>();
            var expectedCheckpoint = new Position();

            Func<Task> setCheckpoint = () => sut.SetCheckpoint(expectedCheckpoint, projection);

            setCheckpoint.Should().NotThrow();

            var checkpoint = await sut.GetLastCheckpoint<Position>(projection);

            checkpoint.Should().BeEquivalentTo(expectedCheckpoint);
        }
    }
}
