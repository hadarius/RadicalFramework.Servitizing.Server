using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Radical.Servitizing.Server.Data.Store;

using Servitizing.Data.Store;
using Event;

public class EventStoreMapping : EntityTypeMapping<Event>
{
    private const string TABLE_NAME = "EventStore";

    public override void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable(TABLE_NAME, "EventDTO");

        builder.Property(p => p.PublishTime)
            .HasColumnType("timestamp");
    }
}